using Domain.Entities;

namespace Shared.DTOs
{
    public class LogResDto : BaseDto<LogResDto, Log>
    {
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string CallSite { get; set; }
        public int ThreadId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string RequestId { get; set; }
        public string UserAgent { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }

}
