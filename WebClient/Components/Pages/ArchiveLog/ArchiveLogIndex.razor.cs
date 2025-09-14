using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;

namespace WebClient.Components.Pages.Log;

public partial class ArchiveLogIndex : ComponentBase
{
    // همه‌ی این فیلدها protected شدن
    protected List<ArchiveLogResDto> _list = [];
    protected readonly IndexDto _indexDto = new();
    protected ArchiveRequestDto _archiveRequest = new();
    protected bool _isLoading = true;
    protected bool _isBusy;
    protected bool _modalIsBusy;
    protected int _total;

    [Inject] protected IBaseService BaseService { get; set; } = null!;
    [Inject] protected IJSRuntime Js { get; set; } = null!;
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
        var result = await BaseService.Post<IndexDto, IndexResDto<ArchiveLogResDto>>("v1/Log/ArchiveLogsIndex", _indexDto);
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

    protected async Task ShowArchiveModal()
    {
        _archiveRequest = new();
        _modalIsBusy = false;
        await Js.InvokeVoidAsync("openModal", "archiveModal");
    }

    protected async Task HandleArchiveSubmit()
    {
        try
        {
            _isBusy = true;
            var result = await BaseService.Post<ArchiveRequestDto, object>("v1/Log/ArchiveLogs", _archiveRequest);
            if (result is not null)
            {
                ToastService.ShowSuccess("آرشیو با موفقیت ایجاد شد");
                await Js.InvokeVoidAsync("closeModal", "archiveModal");
                await GetData();
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    protected async Task RestoreArchive(ArchiveLogResDto dto)
    {
        try
        {
            var result = await BaseService.Post($"v1/Log/RestoreArchive?archiveLogId={dto.Id}");
            if (result)
            {
                ToastService.ShowSuccess("آرشیو با موفقیت بازگردانی شد");
                // await GetData();
            }
        }
        catch
        {
            ToastService.ShowError("خطا در بازگردانی");
        }
    }
}
