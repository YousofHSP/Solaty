using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Log: IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string CallSite { get; set; }
        public int ThreadId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string PhysicalPath { get; set; }
        public string RequestId { get; set; }
        public string UserAgent { get; set; }
        public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
    }
}
