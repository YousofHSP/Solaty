using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Reports.Contracts
{
    public interface IWordReportService
    {
        Task<MemoryStream> Get();
    }
}
