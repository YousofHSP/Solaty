using System.Text.Json;
using Common.Utilities;
using Data;
using Data.Contracts;
using Domain.Entities;
using Domain.Entities.Common;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.DTOs;

namespace Service.Model
{
    public class HashEntityValidator : IHashEntityValidator
    {
        private readonly IRepository<Audit> _auditRepository;
        private readonly IRepository<AuditCheck> _auditCheckRepository;
        private readonly IRepository<Setting> _settingRepository;
        private readonly ILogger<HashEntityValidator> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public HashEntityValidator(IRepository<Audit> auditRepository, ILogger<HashEntityValidator> logger, ApplicationDbContext dbContext, IServiceProvider serviceProvider, IRepository<AuditCheck> auditCheckRepository, IRepository<Setting> settingRepository)
        {
            _auditRepository = auditRepository;
            _logger = logger;
            _context = dbContext;
            _serviceProvider = serviceProvider;
            _auditCheckRepository = auditCheckRepository;
            _settingRepository = settingRepository;
        }

        public async Task<bool> IsValidAsync(IHashedEntity entity, int userId, CancellationToken ct, bool stopApp = true)
        {
            var checkAudit = await _settingRepository.TableNoTracking
                .FirstOrDefaultAsync(i => i.Id == (int)SettingKey.AuditLogEnabled);
            if (checkAudit.Value != "1")
                return true;
            var hash = GetPropertiesHas(entity);
            var isValid = hash == entity.Hash;

            if (!isValid)
            {
                var model = entity.GetType().Name;
                _logger.LogError($"Entity is not valid EntityName: {model} , EntityId: {entity.Id}");
                var result = await RestoreEncryptedFieldsAsync(entity);
                var auditCheck = new AuditCheck
                {
                    TablesCheckCount = 1,
                    CreatorUserId = userId,
                    AuditCheckDetails = new List<AuditCheckDetail>
                    {
                        new AuditCheckDetail{ Model = model, ModelId = entity.Id, Status= AuditCheckDetailStatus.Invalid, AuditCreateDate = result}
                    }
                };
                await _auditCheckRepository.AddAsync(auditCheck, ct);
                if (stopApp)
                {
                    var appModeSetting = await _settingRepository.GetByIdAsync(ct, (int)SettingKey.AppMode);
                    appModeSetting.Value = "0";
                    await _settingRepository.UpdateAsync(appModeSetting, ct);
                    _logger.LogWarning("App stoped becuses Entity is not valid");

                }
            }
            return isValid;

        }
        private string GetPropertiesHas(IHashedEntity entity)
        {
            var entityType = _context.Model.FindEntityType(entity.GetType());
            if (entityType == null)
                throw new InvalidOperationException("Entity type metadata not found");

            var props = entityType.GetProperties()
                .Where(p => p.Name != nameof(IHashedEntity.Id) &&
                            p.Name != nameof(IHashedEntity.Hash) &&
                            p.Name != nameof(IHashedEntity.SaltCode))
                .OrderBy(p => p.Name).ToList();

            var dict = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                var value = prop.PropertyInfo?.GetValue(entity)?.ToString() ?? string.Empty;
                dict[prop.Name] = value;
            }

            var json = JsonSerializer.Serialize(dict);
            return SecurityHelpers.GetSha256Hash(json, entity.SaltCode);
        }
        public async Task<DateTimeOffset> RestoreEncryptedFieldsAsync(IHashedEntity entity)
        {
            var modelName = entity.GetType().Name;
            var audit = await _auditRepository.TableNoTracking.OrderBy(i => i.Id).FirstOrDefaultAsync(i => i.Model == modelName && i.ModelId == entity.Id);
            if (audit is null)
                throw new ApplicationException($"Audit Not Found model name: {modelName}, model id: {entity.Id}");

            //var decryptedValues = SecurityHelpers.DecryptAes(audit.NewValue);
            var fileds = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(audit.NewValue);
            entity.Hash = audit.Hash;
            _context.Attach(entity);
            entity.SaltCode = audit.SaltCode;
            foreach (var kv in fileds)
            {
                if (kv.Key == "Id") continue;

                var property = entity.GetType().GetProperty(kv.Key);
                if (property != null && property.CanWrite)
                {
                    var propertyType = property.PropertyType;
                    var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    try
                    {
                        object? value = null;

                        if (kv.Value.ValueKind == JsonValueKind.Null)
                        {
                            // اگر پراپرتی nullable نیست، نباید مقدار null ست بشه
                            //if (Nullable.GetUnderlyingType(propertyType) == null)
                            //    throw new ApplicationException($"مقدار null نمی‌تواند به پراپرتی غیرnullable ({propertyType.Name}) اختصاص یابد: {kv.Key}");

                            value = null;
                        }
                        else
                        {
                            value = kv.Value.Deserialize(targetType);
                        }

                        property.SetValue(entity, value);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"خطا در تبدیل پراپرتی {kv.Key} به نوع {propertyType.Name}: مقدار = {kv.Value}", ex);
                    }
                }
            }
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogWarning($"Entity Restored EntityName: {modelName} , EntityId: {entity.Id}, With AuditId: {audit.Id}");
            return audit.CreateDate;
        }

        public async Task<AuditCheck> ValidateAllHashedEntitiesAsync(int userId, CancellationToken ct)
        {
            var auditCheckModel = new AuditCheck
            {
                CreatorUserId = userId,
                TablesCheckCount = 0,
                AuditCheckDetails = new(),
            };

            var hashedEntityTypes = typeof(IHashedEntity).Assembly
                .GetTypes()
                .Where(t => t.IsClass &&
                            !t.IsAbstract &&
                            typeof(IHashedEntity).IsAssignableFrom(t));


            foreach (var type in hashedEntityTypes)
            {
                var repositoryType = typeof(IRepository<>).MakeGenericType(type);
                dynamic repository = _serviceProvider.GetService(repositoryType);
                if (repository == null)
                {
                    _logger.LogWarning($"Repository not found for type {type.Name}");
                    continue;
                }

                auditCheckModel.TablesCheckCount++;
                // TableNoTracking استفاده می‌کنیم چون فقط خواندن است
                IEnumerable<IHashedEntity> entities = await Task.Run(() =>
                    ((IQueryable)repository.TableNoTracking)
                    .Cast<IHashedEntity>()
                    .ToList());

                foreach (var entity in entities)
                {
                    var auditCheckDetail = new AuditCheckDetail
                    {
                        AuditCheckId = auditCheckModel.Id,
                        Model = entity.GetType().Name,
                        ModelId = entity.Id,
                        Status = AuditCheckDetailStatus.Valid

                    };
                    var hash = GetPropertiesHas(entity);

                    if (hash != entity.Hash)
                    {
                        auditCheckDetail.Status = AuditCheckDetailStatus.Invalid;
                        auditCheckDetail.AuditCreateDate = await RestoreEncryptedFieldsAsync(entity);
                    }
                    auditCheckModel.AuditCheckDetails.Add(auditCheckDetail);
                }
            }
            await _auditCheckRepository.AddAsync(auditCheckModel, ct);
            return auditCheckModel;
        }
    }
}
