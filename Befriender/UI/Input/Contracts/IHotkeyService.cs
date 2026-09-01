namespace Befriender.UI.Input.Contracts;

using System;

public interface IHotkeyService {
    event Action? OnHotkeyPressed;
}