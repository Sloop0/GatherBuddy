﻿using System;
using System.Linq;
using GatherBuddy.Caching;
using GatherBuddy.Classes;
using GatherBuddy.Managers;
using GatherBuddy.Utility;
using GatherBuddy.Weather;
using ImGuiNET;
using ImGuiScene;

namespace GatherBuddy.Gui.Cache
{
    internal class Weather
    {
        private static WeatherManager? _weather;

        public const int NumWeathers = 8;

        public readonly DateTime[] WeatherTimes;
        public readonly string[]   WeatherTimeStrings;

        public readonly CachedWeather[] Weathers;

        private long _totalHour;

        public          string Filter;
        public          string FilterLower;
        public readonly float  FilterSize;

        public Weather(WeatherManager weather)
        {
            var hour = TimeStamp.UtcNow.TotalEorzeaHours();
            _weather           = weather;
            WeatherTimes       = WeatherManager.NextWeatherChangeTimes(NumWeathers, TimeStamp.Epoch.AddEorzeaHours(-16));
            WeatherTimeStrings = new string[NumWeathers];
            _totalHour         = hour - 8;
            Filter             = "";
            FilterLower        = "";
            Weathers           = CachedWeather.CreateWeatherCache();
            FilterSize         = Weathers.Max(c => ImGui.CalcTextSize(c.Zone).X);
            Update(hour);
        }

        public void Update(long totalHour)
        {
            if (totalHour - _totalHour < 8)
                return;

            UpdateWeather();
            UpdateTimes((totalHour - _totalHour) / 8);
            _totalHour = totalHour - ((totalHour % RealTime.HoursPerDay) & 0b111);
        }

        private void UpdateTimes(long diff)
        {
            for (var i = 0; i < NumWeathers; ++i)
            {
                WeatherTimes[i]       = WeatherTimes[i].AddMilliseconds(TimeStamp.Epoch.AddEorzeaHours(8 * diff));
                WeatherTimeStrings[i] = WeatherTimes[i].TimeOfDay.ToString();
            }
        }

        private void UpdateWeather()
        {
            for (var i = 0; i < Weathers.Length; ++i)
                Weathers[i].Update(i);
        }


        public readonly struct CachedWeather
        {
            public readonly string        Zone;
            public readonly string        ZoneLower;
            public readonly string[]      WeatherNames;
            public readonly TextureWrap[] Icons;

            public CachedWeather(string name)
            {
                Zone         = name;
                ZoneLower    = name.ToLowerInvariant();
                WeatherNames = new string[NumWeathers];
                Icons        = new TextureWrap[NumWeathers];
            }

            public void Update(int idx)
            {
                var timeline = _weather!.UniqueZones[idx];
                timeline.Update(NumWeathers);
                var icons = Service<Icons>.Get();
                for (var i = 0; i < NumWeathers; ++i)
                {
                    WeatherNames[i] = timeline.List[i].Weather.Name;
                    Icons[i]        = icons[timeline.List[i].Weather.Icon];
                }
            }

            public static CachedWeather[] CreateWeatherCache()
            {
                var ret = new CachedWeather[_weather!.UniqueZones.Count];
                for (var i = 0; i < _weather.UniqueZones.Count; ++i)
                {
                    ret[i] = new CachedWeather(_weather.UniqueZones[i].Territory.Name[GatherBuddy.Language]);
                    ret[i].Update(i);
                }

                return ret;
            }
        }
    }
}
