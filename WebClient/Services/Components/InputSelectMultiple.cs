using System.Diagnostics.CodeAnalysis;
using Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Shared.DTOs;

namespace WebClient.Services.Components;

public class InputSelectMultiple<TValue> : InputBase<List<TValue>>
{
    [Parameter] public List<SelectDto> Items { get; set; } = [];
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool ShowPlaceholder { get; set; } = false;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Value != null)
            CurrentValue = Value;
        // اگر مقدار اولیه null بود، خالی کن تا مقایسه‌ها درست کار کنه
        else if (CurrentValue is null) CurrentValue = new List<TValue>();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "select");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "multiple", "multiple");
        builder.AddAttribute(3, "class", CssClass);

        // binder برای گرفتن آرایه‌ی مقادیر انتخاب‌شده
        builder.AddAttribute(4, "onchange",
            EventCallback.Factory.CreateBinder<string[]>(this, OnChange, null));

        int seq = 5;

        if (ShowPlaceholder && !string.IsNullOrWhiteSpace(Placeholder))
        {
            builder.OpenElement(seq++, "option");
            builder.AddAttribute(seq++, "disabled", true);
            builder.AddContent(seq++, Placeholder);
            builder.CloseElement();
        }

        var cmp = EqualityComparer<TValue>.Default;

        foreach (var item in Items)
        {
            var val = (TValue)Convert.ChangeType(item.Id.ToString(), typeof(TValue));
            builder.OpenElement(seq++, "option");
            builder.AddAttribute(seq++, "value", item.Id.ToString());
            if (CurrentValue.Any(v => cmp.Equals(v, val)))
                builder.AddAttribute(seq++, "selected", true);

            builder.AddContent(seq++, item.Title);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private void OnChange(string[] values)
    {
        var list = new List<TValue>(values.Length);
        foreach (var s in values)
        {
            if (typeof(TValue).IsEnum)
                list.Add((TValue)Enum.Parse(typeof(TValue), s));
            else
                list.Add((TValue)Convert.ChangeType(s, typeof(TValue))!);
        }

        CurrentValue = list;
    }

    // برای multiple نیاز خاصی به پارس رشته نداریم
    protected override bool TryParseValueFromString(string? value, out List<TValue> result,
        out string? validationErrorMessage)
    {
        result = new();
        validationErrorMessage = null;
        return true;
    }
}