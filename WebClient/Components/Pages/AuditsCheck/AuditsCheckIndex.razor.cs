using Microsoft.AspNetCore.Components;
using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebClient.Services.Common;
using WebClient.Services.Components;

namespace WebClient.Components.Pages.Sys
{
    public partial class AuditsCheckIndex : ComponentBase
    {
        protected List<AuditCheckResDto> _list = new();
        protected readonly IndexDto _indexDto = new();
        protected bool _isLoading = true;
        protected bool _isBusy;
        protected int _total;

        [Inject] protected IBaseService BaseService { get; set; } = null!;
        [Inject] protected NavigationManager Navigation { get; set; } = null!;
        [Inject] protected ToastService ToastService { get; set; } = null!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GetData();
            }
        }

        protected async Task GetData()
        {
            var result = await BaseService.Post<IndexDto, IndexResDto<AuditCheckResDto>>("v1/System/AuditsCheckIndex", _indexDto);
            if (result is not null)
            {
                _list = result.Data;
                _indexDto.Page = result.Page;
                _indexDto.Limit = result.Limit;
                _total = result.Total;
                _isLoading = false;
                StateHasChanged();
            }
            else
            {
                // در صورت لزوم خطای کلی را نمایش می‌دهیم (توست در BaseService هم خطاها را نشان می‌دهد)
                _list = new();
                _isLoading = false;
            }
        }

        protected async Task PageChanged((int Page, int Limit) args)
        {
            _indexDto.Page = args.Page;
            _indexDto.Limit = args.Limit;
            await GetData();
        }

        /// <summary>
        /// فراخوانی endpoint که بررسی جداول را اجرا می‌کند.
        /// کنترلر یک ApiResult&lt;AuditCheckResDto&gt; برمی‌گرداند، بنابراین اینجا همان DTO را می‌خوانیم.
        /// پس از دریافت موفق، لیست را رفرش می‌کنیم.
        /// </summary>
        protected async Task RunAuditCheck()
        {
            try
            {
                _isBusy = true;

                var auditDto = await BaseService.Get<AuditCheckResDto>("v1/System/AuditsCheck");

                if (auditDto is not null)
                {
                    ToastService.ShowSuccess("بررسی جداول با موفقیت انجام شد");
                    // رفرش لیست (ثبت رکورد جدید در سرور به احتمال زیاد انجام شده)
                    await GetData();
                }
                else
                {
                    // اگر پاسخ null بود، پیام خطا نشان دهیم
                    ToastService.ShowError("پاسخ نامعتبر از سرور");
                }
            }
            catch
            {
                ToastService.ShowError("خطا در بررسی جداول");
            }
            finally
            {
                _isBusy = false;
            }
        }

        protected void GoToDetails(long id)
        {
            Navigation.NavigateTo($"/System/AuditsCheck/{id}");
        }
    }
}
