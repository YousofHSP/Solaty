using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Contracts;
using Domain.Entities;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Service.Message;

namespace Service.Jobs
{
    public class DailyJob
    {
        private readonly IRepository<Notification> _notificationRepository;

        public DailyJob(IRepository<Notification> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }


        public async Task RunAsync()
        {
            var now = DateTimeOffset.Now;
        }
    }
}