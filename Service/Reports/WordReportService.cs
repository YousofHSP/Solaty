using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Service.Reports.Contracts;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Service.Reports
{
    public class WordReportService : IWordReportService
    {
        private readonly IWebHostEnvironment _env;

        public WordReportService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<MemoryStream> Get()
        {

            string templatePath = Path.Combine(_env.WebRootPath, "Templates", "Template.docx");

            if (!System.IO.File.Exists(templatePath))
                throw new NotFoundException("قالب Word پیدا نشد.");

            var products = new List<(string Name, int Qty, decimal Price)>
            {
                ("خودکار", 2, 5000),
                ("دفتر", 1, 20000),
                ("مداد", 5, 3000)
            };

            using var memoryStream = new MemoryStream();

            using (var document = DocX.Load(templatePath))
            {
                document.ReplaceText("{FullName}", "علی رضایی");
                document.ReplaceText("{Date}", DateTime.Now.ToString("yyyy/MM/dd"));

                // پیدا کردن جایی که جدول باید درج بشه
                var placeholder = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{ProductTable}"));
                if (placeholder != null)
                {
                    // ایجاد جدول برای محصولات
                    var table = document.AddTable(products.Count + 1, 3);
                    table.Alignment = Alignment.center;
                    table.Design = TableDesign.TableGrid;

                    // اضافه کردن سرفصل‌های جدول
                    table.Rows[0].Cells[0].Paragraphs[0].Append("نام کالا").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("تعداد").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("قیمت").Bold();

                    // اضافه کردن داده‌ها به جدول
                    for (int i = 0; i < products.Count; i++)
                    {
                        table.Rows[i + 1].Cells[0].Paragraphs[0].Append(products[i].Name);
                        table.Rows[i + 1].Cells[1].Paragraphs[0].Append(products[i].Qty.ToString());
                        table.Rows[i + 1].Cells[2].Paragraphs[0].Append(products[i].Price.ToString("N0"));
                    }

                    // حذف متن placeholder و جایگزینی جدول
                    placeholder.InsertTableAfterSelf(table);
                    placeholder.ReplaceText("{ProductTable}", "");
                }
                else
                {
                    throw new InvalidOperationException("محل قرارگیری جدول پیدا نشد.");
                }

                document.SaveAs(memoryStream);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
