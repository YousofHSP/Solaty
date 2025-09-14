using Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.Role;

public partial class RoleIndex : ComponentBase
{
    private List<RoleResDto> _list = [];
    private RoleDto _data = new();
    private List<Permission> _permissions = [];
    private readonly IndexDto _indexDto = new();
    private Timer? _debounceTimer;
    private int _total;
    private string _modalTitle = "";
    private bool _modalIsBusy;
    private bool _isBusy;
    private bool _isLoading = true;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public IBaseService BaseService { get; set; } = null!;
    [Inject] public ToastService ToastService { get; set; } = null!;


    private async Task GetData()
    {
        var result = await BaseService.Post<IndexDto, IndexResDto<RoleResDto>>("v1/Role/Index", _indexDto);
        if (result is not null)
        {
            _list = result.Data;
            _indexDto.Page = result.Page;
            _indexDto.Limit = result.Limit;
            _total = result.Total;
            _isLoading = false;
            var permissions =
                await BaseService.Get<List<Permission>>("v1/Role/AllPermissions");

            if (permissions is not null)
            {
                _permissions = permissions;
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

    private async Task Delete()
    {
        await Js.InvokeVoidAsync("closeModal", "deleteModal");
        _isLoading = true;
        try
        {
            var deleteResult = await BaseService.Delete($"v1/Role/{_data.Id}");
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
            RoleResDto? userResult;

            if (_modalTitle == "ویرایش")
            {
                userResult = await BaseService.Patch<RoleDto, RoleResDto>("v1/Role/Update", _data);
            }
            else
            {
                userResult = await BaseService.Post<RoleDto, RoleResDto>("v1/Role/Create", _data);
            }

            if (userResult is not null)
            {
                ToastService.ShowSuccess("اطلاعات  با موفقیت ثبت شد");
                await Js.InvokeVoidAsync("closeModal", "dataModal");
                await GetData();
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ShowCreateModal()
    {
        _modalTitle = "ایجاد";
        _data = new();
        await Js.InvokeVoidAsync("openModal", "dataModal");
    }

    private async Task ShowEditModal(int id)
    {
        await Js.InvokeVoidAsync("openModal", "dataModal");
        _modalTitle = "ویرایش";
        _modalIsBusy = true;
        try
        {
            var result = await BaseService.Get<RoleResDto>($"v1/Role/{id}");
            if (result is not null)
            {
                _data.Title = result.Title;
                _data.Name = result.Name;
                _data.Id = result.Id;
                _data.Permissions = result.Permissions;
                _modalIsBusy = false;
                StateHasChanged();
            }
        }
        finally
        {
            _modalIsBusy = false;
        }
    }

    private async Task ShowDeleteWarning(int id)
    {
        await Js.InvokeVoidAsync("openModal", "deleteModal");
        _data = new() { Id = id };
    }

    private void OnPermissionChanged(ChangeEventArgs e, string value)
    {
        bool isChecked = e.Value is bool b && b;
        if (isChecked)
        {
            if (!_data.Permissions.Contains(value))
                _data.Permissions.Add(value);
        }
        else
        {
            _data.Permissions.Remove(value);
        }
    }
}