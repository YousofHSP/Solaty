namespace WebClient.Services.Components;

public class ToastService
{
    public class ToastMessage
    {
        public string Message { get; set; }
        public string Type { get; set; }
    }

    public event Action? OnChange;
    public List<ToastMessage> ToastList { get; set; } = new();

    public void ShowSuccess(string message)
    {
        AddToast(message, "success");
    }

    public void ShowError(string message)
    {
        AddToast(message, "error");
    }

    private void AddToast(string message, string type)
    {
        var context = SynchronizationContext.Current;
        var toast = new ToastMessage { Message = message, Type = type};
        ToastList.Add(toast);
        OnChange?.Invoke();

        Task.Delay(3000).ContinueWith(_ =>
        {
            context?.Post(__ =>
            {
                ToastList.Remove(toast);
                OnChange?.Invoke();

            }, null);
        });
    }
}