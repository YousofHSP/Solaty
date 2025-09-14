using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Exceptions;
using Data.Contracts;
using Domain.Entities;
using Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Model.Contracts;
using Shared;
using Shared.DTOs;

namespace Service.Model
{
    public class PasswordHistoryService : IPasswordHistoryService
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<PasswordHistory> _repository;
        private readonly ISettingService _settingService;

        public PasswordHistoryService(ISettingService settingService, IRepository<PasswordHistory> repository, IPasswordHasher<User> passwordHasher, UserManager<User> userManager)
        {
            _settingService = settingService;
            _repository = repository;
            _passwordHasher = passwordHasher;
            _userManager = userManager;
        }

        public async Task AddAsync(PasswordHistory model, CancellationToken ct)
        {
            await _repository.AddAsync(model, ct);

        }


        public async Task<PasswordHistory> CheckPasswordHistoryAsync(string password, User user, int createrUserId, CancellationToken ct)
        {
            var passwordHistoryCount = await _settingService.GetValueAsync<int>(SettingKey.PasswordHistoryCount);
            if (passwordHistoryCount > 0)
            {
                var passwordHistories = await _repository.TableNoTracking
                    .Where(i => i.UserId == user.Id)
                    .Take(passwordHistoryCount)
                    .ToListAsync();
                foreach (var item in passwordHistories)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, item.PasswordHash, password);
                    if (result == PasswordVerificationResult.Success)
                        throw new BadRequestException("رمز عبور نباید تکراری باشد");
                }


            }
            
            var model = new PasswordHistory
            {
                UserId = user.Id,
                CreatorUserId = createrUserId,
            };
            return model;
        }
    }
}
