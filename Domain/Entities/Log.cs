using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Log: IEntity<long>
    {
        [Key]
        public long Id { get; set; }

        public DateTime TimeStamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; }= string.Empty;
        public string Exception { get; set; }= string.Empty;
        public string CallSite { get; set; }= string.Empty;
        public int ThreadId { get; set; }
        public string UserName { get; set; }= string.Empty;
        public string IpAddress { get; set; }= string.Empty;
        public string PhysicalPath { get; set; }= string.Empty;
        public string RequestId { get; set; }= string.Empty;
        public string UserAgent { get; set; }= string.Empty;
        public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
    }
}
