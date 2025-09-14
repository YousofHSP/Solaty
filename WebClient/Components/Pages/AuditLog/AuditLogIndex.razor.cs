using Microsoft.AspNetCore.Components;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;

namespace WebClient.Components.Pages.Log;

public partial class AuditLogIndex : ComponentBase
{
    protected List<AuditDto> _list = [];
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
        var result = await BaseService.Post<IndexDto, IndexResDto<AuditDto>>("v1/Log/AuditIndex", _indexDto);
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
