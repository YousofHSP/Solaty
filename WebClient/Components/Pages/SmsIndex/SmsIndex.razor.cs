using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.SmsIndex;

public partial class SmsIndex : ComponentBase
{
    private List<SmsLogResDto> _list = new();
    private readonly IndexDto _indexDto = new();
    private Timer? _debounceTimer;
    private int _total;
    private bool _isLoading = true;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public IBaseService BaseService { get; set; } = null!;
    [Inject] public ToastService ToastService { get; set; } = null!;

    private async Task GetData()
    {
        var result = await BaseService.Post<IndexDto, IndexResDto<SmsLogResDto>>("v1/Sms/Index", _indexDto);
        if (result is not null)
        {
            _list = result.Data;
            _indexDto.Page = result.Page;
            _indexDto.Limit = result.Limit;
            _total = result.Total;
        }
        else
        {
            _list = new List<SmsLogResDto>();
            _total = 0;
        }
        _isLoading = false;
        StateHasChanged();
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

    private async Task ReSend(int id)
    {
        try
        {
            var dto = new ReSendDto { Id = id };
            var result = await BaseService.Post("v1/Sms/ReSend", dto);
            if (result)
            {
                ToastService.ShowSuccess("پیامک با موفقیت ارسال شد");
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"خطا در ارسال مجدد پیامک: {ex.Message}");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetData();
        }
    }
}
