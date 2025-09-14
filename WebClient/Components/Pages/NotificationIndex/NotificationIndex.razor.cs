using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.NotificationIndex;

public partial class NotificationIndex : ComponentBase
{
    // لیست اعلان‌ها
    private List<NotificationResDto> _list = [];

    // DTO برای فیلتر، صفحه‌بندی و جستجو
    private readonly IndexDto _indexDto = new();

    private Timer? _debounceTimer;
    private int _total;
    private bool _isLoading = true;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public IBaseService BaseService { get; set; } = null!;
    [Inject] public ToastService ToastService { get; set; } = null!;

    // گرفتن داده‌ها از API
    private async Task GetData()
    {
        var result = await BaseService.Post<IndexDto, IndexResDto<NotificationResDto>>(
            "v1/User/UserNotifications", _indexDto);

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

    // سرچ با تاخیر (debounce)
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

    // صفحه‌بندی
    private async Task PageChanged((int Page, int Limit) args)
    {
        _indexDto.Page = args.Page;
        _indexDto.Limit = args.Limit;
        await GetData();
    }

    // اولین بار که صفحه لود میشه
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetData();
        }
    }
}
