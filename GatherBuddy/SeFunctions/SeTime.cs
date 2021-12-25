using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using GatherBuddy.Time;

namespace GatherBuddy.SeFunctions;

public static unsafe class SeTime
{
    private static readonly Framework* Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();

    public static TimeStamp GetServerTime()
        => new(Framework == null ? TimeStamp.UtcNow : Framework->ServerTime * 1000);

    public static TimeStamp GetEorzeaTime()
        => new(Framework == null ? TimeStamp.UtcNow.ConvertToEorzea() : Framework->EorzeaTime * 1000);

    public static TimeStamp ServerTime         { get; private set; }
    public static TimeStamp EorzeaTime         { get; private set; }
    public static int       EorzeaTotalMinute  { get; private set; }
    public static int       EorzeaTotalHour    { get; private set; }
    public static int       EorzeaMinuteOfDay  { get; private set; }
    public static int       EorzeaHourOfDay    { get; private set; }
    public static int       EorzeaMinuteOfHour { get; private set; }

    public static event Action? Updated;
    public static event Action? HourChanged;
    public static event Action? WeatherChanged;     
    public static void Update()
    {
        ServerTime         = GetServerTime();
        EorzeaTime         = GetEorzeaTime();
        var minute = EorzeaTime.CurrentMinute;
        if (minute != EorzeaTotalMinute)
        {
            EorzeaTotalMinute  = minute;
            EorzeaMinuteOfDay  = EorzeaTotalMinute % RealTime.MinutesPerDay;
            EorzeaMinuteOfHour = EorzeaMinuteOfDay % RealTime.MinutesPerHour;
        }

        var hour = EorzeaTotalMinute / RealTime.MinutesPerHour;
        if (hour != EorzeaTotalHour)
        {
            EorzeaTotalHour = hour;
            EorzeaHourOfDay = EorzeaMinuteOfDay / RealTime.MinutesPerHour;
            HourChanged?.Invoke();
            if (EorzeaHourOfDay >> 3 == 0)
                WeatherChanged?.Invoke();
        }
        Updated?.Invoke();
    }
}
