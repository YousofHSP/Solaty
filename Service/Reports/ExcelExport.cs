using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ClosedXML.Excel;
using Common.Utilities;
using Presentation.Attributes;
using Service.Reports.Contracts;
using Shared.Attribute;

namespace Service.Reports;

public class ExcelExport : IExcelExport
{
    public byte[] Execute<T>(List<T> list)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Data");
        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var selectedProps = props
            .Where(p => p.GetCustomAttribute<ExcelIgnoreAttribute>() == null)
            .Select(p => new
            {
                Property = p,
                DisplayName = p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name
            }).ToList();


        for (var i = 0; i < selectedProps.Count ; i++)

        {
            worksheet.Cell(1, i + 1).Value = selectedProps[i].DisplayName;
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.AliceBlue;
        }

        for (var row = 0; row < list.Count; row++)
        {
            var item = list[row];
            for (var col = 0; col < selectedProps.Count; col++)
            {
                var rawValue = selectedProps[col].Property.GetValue(item);
                var cellValue = rawValue switch
                {
                    DateTime dt => dt.ToShamsi(),
                    DateTimeOffset dt => dt.ToShamsi(),
                    bool b => b ? "بله" : "خیر",
                    null => "",
                    _ => rawValue.ToString()
                };
                worksheet.Cell(row + 2, col + 1).Value = cellValue;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}