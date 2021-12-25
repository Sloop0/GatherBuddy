using System;
using System.Reflection;
using Dalamud;
using Dalamud.Plugin;
using GatherBuddy.Caching;
using GatherBuddy.Gui;
using GatherBuddy.Managers;

namespace GatherBuddy;

public partial class GatherBuddy : IDalamudPlugin
{
    public string Name
        => "GatherBuddy";

    public static string Version = string.Empty;

    public static GatherBuddyConfiguration Config         { get; private set; } = null!;
    public static GameData                 GameData       { get; private set; } = null!;
    public static ClientLanguage           Language       { get; private set; } = ClientLanguage.English;
    public static CommandManager           CommandManager { get; private set; } = null!;
    public static Weather.Manager          WeatherManager { get; private set; } = null!;

    public readonly  Identificator       Identificator;
    public readonly  Alarms.Manager      Alarms;
    public readonly  FishManager         FishManager;
    public readonly  NodeTimeLine        NodeTimeLine;
    private readonly Interface           _gatherInterface;
    private readonly FishTimer.FishTimer _fishTimer;

    public GatherBuddy(DalamudPluginInterface pluginInterface)
    {
        Dalamud.Initialize(pluginInterface);
        Version        = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        Config         = GatherBuddyConfiguration.Load();
        Language       = Dalamud.ClientState.ClientLanguage;
        GameData       = new GameData(Dalamud.GameData);
        WeatherManager = new Weather.Manager();
        FishManager    = new FishManager();
        NodeTimeLine   = new NodeTimeLine();
        CommandManager = new CommandManager(Dalamud.SigScanner);
        Identificator  = new Identificator();

        Alarms           = new Alarms.Manager();
        _fishTimer       = new FishTimer.FishTimer(FishManager, WeatherManager);
        _gatherInterface = new Interface(this);

        Dalamud.PluginInterface.UiBuilder.Draw         += _gatherInterface!.Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi += OnGatherBuddy;

        if (Config.AlarmsEnabled)
            Alarms.Enable(true);

        if (Config.OpenOnStart)
            OnGatherBuddy();

        InitializeCommands();
    }

    void IDisposable.Dispose()
    {
        _gatherInterface.Dispose();
        _fishTimer.Dispose();
        Alarms.Dispose();
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= OnGatherBuddy;
        Dalamud.PluginInterface.UiBuilder.Draw         -= _gatherInterface!.Draw;
        DisposeCommands();
    }
}
