using System;
using System.Reflection;
using Dalamud;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GatherBuddy.Alarms;
using GatherBuddy.Config;
using GatherBuddy.FishTimer.Parser;
using GatherBuddy.Gui;
using GatherBuddy.Plugin;
using GatherBuddy.SeFunctions;
using GatherBuddy.Weather;
using Lumina.Excel.GeneratedSheets;

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
    public static FishLog        FishLog        { get; private set; } = null!;
    public static EventFramework EventFramework { get; private set; } = null!;
    public static FishingParser  FishingParser  { get; private set; } = null!;

    internal readonly GatherGroup.Manager GatherGroupManager;
    internal readonly AlarmManager        AlarmManager;
    internal readonly WindowSystem        WindowSystem;
    internal readonly Interface           Interface;
    internal readonly Executor            Executor;
    internal readonly SpearfishingHelper  SpearfishingHelper;

    internal readonly GatherBuddyIpc Ipc;
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
        FishLog            = new FishLog();
        EventFramework     = new EventFramework(Dalamud.SigScanner);
        FishingParser      = new FishingParser();
        Executor           = new Executor();
        GatherGroupManager = GatherGroup.Manager.Load();
        AlarmManager       = new AlarmManager();

        SpearfishingHelper = new SpearfishingHelper(GameData);

        InitializeCommands();

        WindowSystem = new WindowSystem(Name);
        Interface    = new Interface(this);
        WindowSystem.AddWindow(Interface);
        Dalamud.PluginInterface.UiBuilder.Draw         += WindowSystem.Draw;
        Dalamud.PluginInterface.UiBuilder.Draw         += SpearfishingHelper.Draw;
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
        Dalamud.PluginInterface.UiBuilder.Draw         -= SpearfishingHelper.Draw;
        Dalamud.PluginInterface.UiBuilder.Draw         -= WindowSystem.Draw;
        Interface.Dispose();
        WindowSystem.RemoveAllWindows();
        DisposeCommands();
        Time.Dispose();
        Icons.DefaultStorage.Dispose();
    }
}
