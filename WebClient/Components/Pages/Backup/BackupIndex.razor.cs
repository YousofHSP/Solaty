using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.Backup;

public partial class BackupIndex : ComponentBase
{
    private List<BackupDto> _list = [];
    private readonly IndexDto _indexDto = new();
    private Timer? _debounceTimer;
    private int _total;
    private bool _isLoading = true;
    private bool _isBackupBusy;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public IBaseService BaseService { get; set; } = null!;
    [Inject] public ToastService ToastService { get; set; } = null!;

    private async Task GetData()
    {
        var result = await BaseService.Post<IndexDto, IndexResDto<BackupDto>>("v1/System/BackupsList", _indexDto);
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
            _list = [];
            _total = 0;
            _isLoading = false;
        }
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        _indexDto.Search = e.Value?.ToString() ?? "";

        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new Timer(500) { AutoReset = false };
        _debounceTimer.Elapsed += async (_, _) =>
        {
            await InvokeAsync(GetData);
        };
        _debounceTimer.Start();
    }

    private async Task PageChanged((int Page, int Limit) args)
    {
        _indexDto.Page = args.Page;
        _indexDto.Limit = args.Limit;
        await GetData();
    }

    private async Task CreateBackup()
    {
        _isBackupBusy = true;
        try
        {
            var result = await BaseService.Get("v1/System/BackupDatabase");
            if (result)
            {
                ToastService.ShowSuccess("بکاپ با موفقیت ایجاد شد");
                await GetData();
            }
            else
            {
                ToastService.ShowError("خطا در ایجاد بکاپ");
            }
        }
        finally
        {
            _isBackupBusy = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetData();
        }
    }

    private async Task Download(BackupDto item)
    {
        var bytes = await BaseService.GetFile($"v1/System/download/{item.Id}");
        if (bytes is not null)
        {
            await Js.InvokeVoidAsync("downloadFileFromBytes", item.FileName, bytes);
        }
        else
        {
            ToastService.ShowError("دانلود فایل با خطا مواجه شد");
        }
    }

}

