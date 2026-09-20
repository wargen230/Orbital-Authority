// src/OrbitalAuthority.Client/SimulationClockController.cs
using System;
using Microsoft.Xna.Framework.Input;

namespace OrbitalAuthority.Gameplay.Utils;

public sealed class SimulationClockController
{
    private readonly SimulationClock _clock;
    private KeyboardState _prev;

    // Доступные скорости
    private static readonly double[] SpeedSteps =
    {
        0.01, 0.1, 0.25, 0.5, 1.0, 2.0, 5.0, 10.0, 50.0, 100.0
    };
    private int _speedIndex = 4;   // начинаем с 1.0

    public SimulationClockController(SimulationClock clock)
    {
        _clock = clock;
        _clock.TimeScale = SpeedSteps[_speedIndex];
    }

    public void Update(KeyboardState keyboard)
    {
        // Пауза — Space
        if (IsPressed(keyboard, Keys.Space))
            _clock.IsPaused = !_clock.IsPaused;

        // Ускорение — клавиша ]
        if (IsPressed(keyboard, Keys.OemCloseBrackets) ||
            IsPressed(keyboard, Keys.PageUp))
        {
            _speedIndex = Math.Min(_speedIndex + 1, SpeedSteps.Length - 1);
            _clock.TimeScale = SpeedSteps[_speedIndex];
        }

        // Замедление — клавиша [
        if (IsPressed(keyboard, Keys.OemOpenBrackets) ||
            IsPressed(keyboard, Keys.PageDown))
        {
            _speedIndex = Math.Max(_speedIndex - 1, 0);
            _clock.TimeScale = SpeedSteps[_speedIndex];
        }

        // Сброс времени — Backspace
        if (IsPressed(keyboard, Keys.Back))
            _clock.TimeScale = 1.0;

        _prev = keyboard;
    }

    private bool IsPressed(KeyboardState now, Keys key)
        => now.IsKeyDown(key) && !_prev.IsKeyDown(key);
}