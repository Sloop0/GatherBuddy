using System;
using System.Reflection;
using Dalamud;
using Dalamud.Plugin;
using GatherBuddy.Gui;
using GatherBuddy.Managers;
using SDL2;

namespace GatherBuddy
{
    public partial class GatherBuddy : IDalamudPlugin
    {
        public string Name
            => "GatherBuddy";

        public static string Version = string.Empty;

        public static GatherBuddyConfiguration Config         { get; private set; } = null!;
        public static GatherBuddyA.GameData    GameData       { get; private set; } = null!;
        public static ClientLanguage           Language       { get; private set; } = ClientLanguage.English;
        public static CommandManager           CommandManager { get; private set; } = null!;

        public readonly  Identificator Identificator;
        public readonly  Gatherer      Gatherer;
        public readonly  AlarmManager  Alarms;
        private readonly Interface     _gatherInterface;
        private readonly FishingTimer  _fishingTimer;

        public GatherBuddy(DalamudPluginInterface pluginInterface)
        {
            Dalamud.Initialize(pluginInterface);
            Version        = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            Config         = GatherBuddyConfiguration.Load();
            Language       = Dalamud.ClientState.ClientLanguage;
            GameData       = new GatherBuddyA.GameData(Dalamud.GameData);
            CommandManager = new CommandManager(Dalamud.SigScanner);
            Identificator  = new Identificator();

            Gatherer         = new Gatherer(CommandManager);
            Alarms           = Gatherer.Alarms;
            _gatherInterface = new Interface(this);
            _fishingTimer    = new FishingTimer(Gatherer!.FishManager, Gatherer!.WeatherManager);

            if (!FishManager.GetSaveFileName().Exists)
                Gatherer!.FishManager.SaveFishRecords();
            else
                Gatherer!.FishManager.LoadFishRecords();

            Dalamud.ClientState.TerritoryChanged           += Gatherer!.OnTerritoryChange;
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
            _fishingTimer.Dispose();
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= OnGatherBuddy;
            Dalamud.PluginInterface.UiBuilder.Draw         -= _gatherInterface!.Draw;
            Dalamud.ClientState.TerritoryChanged           -= Gatherer!.OnTerritoryChange;
            (Gatherer as IDisposable).Dispose();
            DisposeCommands();
        }
    }
}
