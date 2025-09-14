using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.Setting;

public partial class SettingIndex : ComponentBase
{
    private List<SettingResDto> _list = [];
    private SettingDto _data = new();
    private readonly IndexDto _indexDto = new();
    private Timer? _debounceTimer;
    private int _total;
    private string _modalTitle = "";
    private bool _modalIsBusy;
    private bool _isLoading = true;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public IBaseService BaseService { get; set; } = null!;
    [Inject] public ToastService ToastService { get; set; } = null!;

    private async Task GetData()
    {
        var result = await BaseService.Post<IndexDto, IndexResDto<SettingResDto>>("v1/Setting/Index", _indexDto);
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetData();
        }
    }

    private async Task ShowEditModal(SettingResDto item)
    {
        await Js.InvokeVoidAsync("openModal", "dataModal");
        _modalTitle = "ویرایش";
        _data.Id = item.Id;
        _data.Value = item.Value;
        _data.Enable = item.Enable;
    }

    public async Task DoneForm()
    {
        ToastService.ShowSuccess("اطلاعات با موفقیت ثبت شد");
        await Js.InvokeVoidAsync("closeModal", "dataModal");
        await GetData();
    }
}
