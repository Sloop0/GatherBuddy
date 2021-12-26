using GatherBuddy.Caching;
using GatherBuddy.Classes;
using GatherBuddy.SeFunctions;
using ImGuiScene;

namespace GatherBuddy.Gui.Cache;

internal struct Header
{
    public long         DrawHour;
    public Territory    Territory;
    public TextureWrap? CurrentWeather;
    public TextureWrap? NextWeather;

    public void Setup()
        => Update(0, Territory.Invalid);

    public void Update(long hour, Territory territory)
    {
        if (hour - DrawHour < 8 && Territory == territory)
            return;

        DrawHour  = hour - (hour & 0b111);
        Territory = territory;
        if (territory.Id == 0)
        {
            CurrentWeather = null;
            NextWeather    = null;
            return;
        }

        var weathers = global::GatherBuddy.Weather.WeatherManager.GetForecast(Territory, 2, SeTime.ServerTime);
        CurrentWeather = Icons.DefaultStorage[weathers[0].Weather.Data.Icon];
        NextWeather    = Icons.DefaultStorage[weathers[1].Weather.Data.Icon];
    }
}
