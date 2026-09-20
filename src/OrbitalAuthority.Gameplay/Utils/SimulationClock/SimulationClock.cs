namespace OrbitalAuthority.Gameplay.Utils;

public sealed class SimulationClock
{
    public double TotalTime { get; private set; }         // секунды симуляции
    public double Dt { get; set; } = 3600.0;              // базовый шаг
    public double TimeScale { get; set; } = 1.0;
    public bool IsPaused { get; set; }

    public void Advance(int ticks, double dtPerTick)
    {
        TotalTime += ticks * dtPerTick;
    }

    public double Days => TotalTime / 86400.0;
    public double Years => Days / 365.25;
}