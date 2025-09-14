using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Data.Contracts;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DTOs;

namespace Domain.Model
{
    public class SettingService : ISettingService
    {
        private readonly IRepository<Setting> _repository;
        private readonly IMapper _mapper;

        public SettingService(IRepository<Setting> repository,  IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<T> GetValueAsync<T>(SettingKey key)
        {
            var item = await _repository.TableNoTracking.FirstOrDefaultAsync(i => i.Id == (int)key);
            if (item == null || string.IsNullOrEmpty(item.Value))
                return default;

            return Convert<T>(item.Value);


        }
        public T GetValue<T>(SettingKey key)
        {
            var item = _repository.TableNoTracking.FirstOrDefault(i => i.Id == (int)key);
            if (item == null || string.IsNullOrEmpty(item.Value))
                return default;

            return Convert<T>(item.Value);


        }

        public async Task SetValueAsync(SettingKey key, string value, CancellationToken ct)
        {
            var item = await _repository.Table.FirstOrDefaultAsync(i => i.Id == (int)key, ct);
            if(item is null)
                throw new NotFoundException("تنظیم پیدا نشد");
            item.Value = value;
            await _repository.UpdateAsync(item, ct);

        }

        private T Convert<T>(string value)
        {
            var type = typeof(T);
            if (type == typeof(int))
            {
                return (T)(object)int.Parse(value);
            }

            if (type == typeof(string[]))
            {
                return (T)(object)value.Split(',');
            }

            if (type == typeof(List<string>))
            {
                return (T)(object)value.Split(',').Select(x => x.Trim()).ToList();
            }
            if (type == typeof(int[]))
            {
                return (T)(object)value.Split(',').Select(int.Parse).ToArray();
            }

            if (type == typeof(List<int>))
            {
                return (T)(object)value.Split(',').Select(int.Parse).ToList();
            }

            return (T)(object)value;

        }

        public async Task NotifyClientsAsync()
        {
            var settings = await _repository.TableNoTracking
                .ProjectTo<SettingDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            // await _hubContext.Clients.All.SendAsync("ReceiveSettings", settings);
        }
    }

}
