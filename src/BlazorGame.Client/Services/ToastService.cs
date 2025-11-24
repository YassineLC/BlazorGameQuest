using System;
using System.Collections.Generic;

namespace BlazorGame.Client.Services;

public enum ToastLevel { Info, Success, Warning, Error }

public class ToastMessage
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ToastLevel Level { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
}

public class ToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnHide;

    public void Show(string message, string title = "", ToastLevel level = ToastLevel.Info)
    {
        var m = new ToastMessage { Message = message, Title = title, Level = level };
        OnShow?.Invoke(m);
    }

    public void Hide(Guid id) => OnHide?.Invoke(id);
}
