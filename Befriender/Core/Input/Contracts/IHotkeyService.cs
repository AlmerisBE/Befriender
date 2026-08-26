namespace Befriender.Core.Input.Contracts;

using System;

public interface IHotkeyService {
    event Action? OnHotkeyPressed;
}