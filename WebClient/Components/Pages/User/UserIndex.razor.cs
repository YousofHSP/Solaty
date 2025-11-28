using Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.User;

public partial class UserIndex : ComponentBase
{
    private List<UserResDto> _list = [];
    private List<SelectDto> _roles = [];
    private readonly IndexDto _indexDto = new();
    private UserDto _data = new();
    private bool _isLoading = true;
    private bool _isBusy;
    private bool _modalIsBusy;
    private string _userModalTitle = "";
    private int _total;
    private Timer? _debounceTimer;

    [Inject] private IBaseService BaseService { get; set; } = null!;
    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public ToastService ToastService { get; set; } = null!;

    private async Task GetData()
    {
        var result = await BaseService.Post<IndexDto, IndexResDto<UserResDto>>("v1/User/Index", _indexDto);
        if (result is not null)
        {
            _list = result.Data;
            _indexDto.Page = result.Page;
            _indexDto.Limit = result.Limit;
            _total = result.Total;
            _isLoading = false;
            var roles =
                await BaseService.Get<List<SelectDto>>("v1/Role/GetSelectList");

            if (roles is not null)
            {
                _roles = roles;
            }

            StateHasChanged();
        }
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        _indexDto.Search = e.Value?.ToString() ?? "";

        // اگر تایمر قبلی وجود داشت ریست بشه
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        // یه تایمر جدید با 1 ثانیه تأخیر
        _debounceTimer = new Timer(500) { AutoReset = false };
        _debounceTimer.Elapsed += async (_, _) =>
        {
            // صدا زدن سرور
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
        try
        {
            if (firstRender)
            {
                await GetData();
            }
        }
        catch

        {
            Console.WriteLine("error");
        }
    }

    private async Task ShowCreateModal()
    {
        _data = new();
        _userModalTitle = "ایجاد";
        await Js.InvokeVoidAsync("openModal", "dataModal");
    }

    private async Task ShowEditModal(long id)
    {
        await Js.InvokeVoidAsync("openModal", "dataModal");
        _userModalTitle = "ویرایش";
        _modalIsBusy = true;
        try
        {
            var result = await BaseService.Get<UserResDto>($"v1/User/{id}");
            if (result is not null)
            {
                _data.RoleIds = result.RoleIds;
                _data.BirthDate = result.BirthDate;
                _data.FullName= result.FullName;
                _data.Email = result.Email;
                _data.Enable = result.Enable;
                _data.PhoneNumber = result.PhoneNumber;
                _data.Id = result.Id;
                _modalIsBusy = false;
                StateHasChanged();
            }
        }
        finally
        {
            _modalIsBusy = false;
        }
    }

    private async Task ShowDeleteWarning(long id)
    {
        await Js.InvokeVoidAsync("openModal", "deleteModal");
        _data = new UserDto { Id = id };
        StateHasChanged();
    }

    private async Task Delete()
    {
        await Js.InvokeVoidAsync("closeModal", "deleteModal");
        _isLoading = true;
        try
        {
            var deleteResult = await BaseService.Delete($"v1/User/{_data.Id}");
            if (deleteResult)
            {
                ToastService.ShowSuccess(" حذف شد");
                await GetData();
            }
        }
        finally
        {
            _data = new();
            _isLoading = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            _isBusy = true;
            UserResDto? userResult;

            if (_userModalTitle == "ویرایش")
            {
                userResult = await BaseService.Patch<UserDto, UserResDto>("v1/User/Update", _data);
            }
            else
            {
                userResult = await BaseService.Post<UserDto, UserResDto>("v1/User/Create", _data);
            }

            if (userResult is not null)
            {
                ToastService.ShowSuccess("اطلاعات کاربر با موفقیت ثبت شد");
                await Js.InvokeVoidAsync("closeModal", "dataModal");
                await GetData();
            }
        }
        finally
        {
            _isBusy = false;
        }
    }
}