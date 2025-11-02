using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using WebClient.Services.Components;
using Timer = System.Timers.Timer;

namespace WebClient.Components.Pages.Profile;

public partial class Profile : ComponentBase
{
    private UserProfileResDto _profile = new();
    private UserProfileDto _profileDto = new();
    private ChangeProfileImageDto _imageModel = new();
    private SetNewEmailDto _changeEmail = new();
    private SetNewPhoneNumberDto _changePhone = new();
    private ChangePasswordDto _changePassword = new();

    private bool _isLoading = true;
    private bool _isBusy = false;
    private bool _isUploading = false;

    [Inject] private IBaseService BaseService { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private ToastService ToastService { get; set; } = null!;

    private class StringResDto
    {
        public string Value { get; set; } = string.Empty;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadProfile();
        }
    }

    private async Task LoadProfile()
    {
        _isLoading = true;
        try
        {
            var result = await BaseService.Get<UserProfileResDto>("v1/User/GetProfile");
            if (result != null)
            {
                _profile = result;
                _profileDto = new()
                {
                    BirthDate = result.BirthDate,
                    FullName= result.FullName,
                };
            }
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task UpdateProfile()
    {
        _isBusy = true;
        try
        {
            var result = await BaseService.Post<UserProfileDto>("v1/User/ChangeProfile", _profileDto);
            if (result)
            {
                ToastService.ShowSuccess("پروفایل شما با موفقیت بروزرسانی شد");
                await LoadProfile();
            }


        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task UpdateProfileImage()
    {
        if (_imageModel.File == null) return;

        _isUploading = true;
        try
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(_imageModel.File.OpenReadStream()), "file", _imageModel.File.FileName);

            var result = await BaseService.Post<MultipartFormDataContent>("v1/User/ChangeProfileImage", content);
            if (result)
            {
                ToastService.ShowSuccess("عکس پروفایل با موفقیت بروزرسانی شد");
            }
        }
        finally
        {
            _isUploading = false;
        }
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {


            // فایل را به یک MemoryStream بخوانید
            var ms = new MemoryStream();
            await file.OpenReadStream(file.Size).CopyToAsync(ms);
            ms.Position = 0; // مهم: موقع ساخت FormFile باید Position=0 باشد

            // ایجاد IFormFile
            IFormFile formFile = new FormFile(ms, 0, ms.Length, file.Name, file.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = file.ContentType
            };
            _imageModel.File = formFile;
        }
    }

    private async Task ChangeEmail()
    {
        _isBusy = true;
        try
        {
            var result = await BaseService.Post<SetNewEmailDto>("v1/User/SetNewEmail", _changeEmail);
            if (result)
            {
                ToastService.ShowSuccess("ایمیل با موفقیت تغییر کرد");
                await Js.InvokeVoidAsync("closeModal", "emailModal");
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ChangePhone()
    {
        _isBusy = true;
        try
        {
            var result = await BaseService.Post<SetNewPhoneNumberDto>("v1/User/SetNewPhoneNumber", _changePhone);
            if (result)
            {
                ToastService.ShowSuccess("شماره موبایل با موفقیت تغییر کرد");
                await Js.InvokeVoidAsync("closeModal", "phoneModal");
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ChangePassword()
    {
        _isBusy = true;
        try
        {
            var result = await BaseService.Post<ChangePasswordDto>("v1/User/ChangePassword", _changePassword);
            if (result)
            {
                ToastService.ShowSuccess("رمز عبور با موفقیت تغییر کرد");
                await Js.InvokeVoidAsync("closeModal", "passwordModal");
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task OpenModal(string modalId)
    {
        await Js.InvokeVoidAsync("openModal", modalId.Trim());
    }
}
