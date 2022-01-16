using System;
using System.Reflection;
using Dalamud;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GatherBuddy.Alarms;
using GatherBuddy.Caching;
using GatherBuddy.Config;
using GatherBuddy.Gui;
using GatherBuddy.SeFunctions;
using GatherBuddy.Weather;

namespace GatherBuddy;

public partial class GatherBuddy : IDalamudPlugin
{
    public const string InternalName = "GatherBuddy";

    public string Name
        => InternalName;

    public static string Version = string.Empty;

    public static Configuration  Config   { get; private set; } = null!;
    public static GameData       GameData { get; private set; } = null!;
    public static ClientLanguage Language { get; private set; } = ClientLanguage.English;
    public static SeTime         Time     { get; private set; } = null!;


    public static WeatherManager WeatherManager { get; private set; } = null!;
    public static UptimeManager  UptimeManager  { get; private set; } = null!;


    internal readonly GatherGroup.Manager GatherGroupManager;
    internal readonly AlarmManager        AlarmManager;
    internal readonly WindowSystem        WindowSystem;
    internal readonly Interface           Interface;
    internal readonly Executor            Executor;
    internal readonly GatherBuddyIpc      Ipc;
    //    internal readonly WotsitIpc Wotsit;

    public GatherBuddy(DalamudPluginInterface pluginInterface)
    {
        Dalamud.Initialize(pluginInterface);
        Version  = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        Config   = Configuration.Load();
        Language = Dalamud.ClientState.ClientLanguage;
        GameData = new GameData(Dalamud.GameData);
        Time     = new SeTime();

        WeatherManager     = new WeatherManager(GameData);
        UptimeManager      = new UptimeManager(GameData);
        Executor           = new Executor();
        GatherGroupManager = GatherGroup.Manager.Load();
        AlarmManager       = new AlarmManager();

        InitializeCommands();

        WindowSystem = new WindowSystem(Name);
        Interface    = new Interface(this);
        WindowSystem.AddWindow(Interface);
        Dalamud.PluginInterface.UiBuilder.Draw         += WindowSystem.Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi += Interface.Toggle;

        Ipc = new GatherBuddyIpc(this);
        //Wotsit = new WotsitIpc();
    }

    void IDisposable.Dispose()
    {
        UptimeManager.Dispose();
        Ipc.Dispose();
        //Wotsit.Dispose();
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= Interface.Toggle;
        Dalamud.PluginInterface.UiBuilder.Draw         -= WindowSystem.Draw;
        Interface.Dispose();
        WindowSystem.RemoveAllWindows();
        DisposeCommands();
        Time.Dispose();
        Icons.DefaultStorage.Dispose();
    }
}
