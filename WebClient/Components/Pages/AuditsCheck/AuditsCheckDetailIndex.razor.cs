using Microsoft.AspNetCore.Components;
using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebClient.Services.Common;
using WebClient.Services.Components;

namespace WebClient.Components.Pages.Sys
{
    public partial class AuditsCheckDetailIndex : ComponentBase
    {
        [Parameter] public int AuditCheckId { get; set; }

        protected List<AuditCheckDetailResDto> _list = new();
        protected readonly IndexDto _indexDto = new();
        protected bool _isLoading = true;
        protected int _total;

        [Inject] protected IBaseService BaseService { get; set; } = null!;
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
            var url = $"v1/System/AuditsCheckDetailIndex/{AuditCheckId}";
            var result = await BaseService.Post<IndexDto, IndexResDto<AuditCheckDetailResDto>>(url, _indexDto);
            if (result is not null)
            {
                _list = result.Data;
                _indexDto.Page = result.Page;
                _indexDto.Limit = result.Limit;
                _total = result.Total;
                _isLoading = false;
                StateHasChanged();
            }
        }

        protected async Task PageChanged((int Page, int Limit) args)
        {
            _indexDto.Page = args.Page;
            _indexDto.Limit = args.Limit;
            await GetData();
        }
    }
}
