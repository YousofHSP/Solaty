using System.Linq.Expressions;

namespace Service.Reports.Contracts;

public interface IExcelExport
{
    public byte[] Execute<TEntity>(List<TEntity> list);
}