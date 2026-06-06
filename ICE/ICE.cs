using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;
using ECommons.GameHelpers;
using ICE.ConfigFiles;
using ICE.IPC;
using ICE.Ui;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using Pictomancy;
using System.Collections.Generic;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using ICE.Scheduler.Handlers.PictoStuff;
using ICE.Utilities.GatheringHelper.RouteLoader;

namespace ICE;

public sealed partial class ICE : IDalamudPlugin
{
    public static string Name => "ICE";

    internal static ICE P = null!;
    private Config config;
    public static Config C => P.config;
    public static PctContext PictoService;

    // Missing ECommons PluginService. Update to Svc when ECommons get updated
    [PluginService] public static IUnlockState UnlockState { get; set; } = null!;
    
    public MissionTimer MissionTimer { get; private set; }

    // Window's that I use, base window to the settings... need these to actually show shit 
    internal WindowSystem windowSystem;
    internal MainWindow mainWindow;
    internal OverlayWindow overlayWindow;
    internal DebugWindow debugWindow;
    internal InfoWindow infoWindow;
    internal Window_ExternalDetails externalDetails;

    // Taskmanager from Ecommons
    internal TaskManager TaskManager;

    // Internal IPC's that I use for... well plugins. 
    internal LifestreamIPC Lifestream;
    internal NavmeshIPC Navmesh;
    internal PandoraIPC Pandora;
    internal ArtisanIPC Artisan;
    internal VislandIPC Visland;
    internal AutoHookIPC AutoHook;
    internal IceCosmicExplorationIPC IceIpc;

    public ICE(IDalamudPluginInterface pi)
    {
        P = this;
        ECommonsMain.Init(pi, P, Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        new ECommons.Schedulers.TickScheduler(Load);
        PictoService = PctService.Initialize(pi);
    }
    public void Load()
    {
        EzConfig.Migrate<Config>();
        config = EzConfig.Init<Config>();

        //IPC's that are used
        Lifestream = new();
        Navmesh = new();
        Pandora = new();
        Artisan = new();
        Visland = new();
        AutoHook = new();
        IceIpc = new();

        // all the windows
        windowSystem = new();
        mainWindow = new();
        overlayWindow = new();
        debugWindow = new();
        infoWindow = new();
        externalDetails = new();

        EzCmd.Add("/icecosmic", OnCommand, """
            打开插件界面
            /ice help - 显示所有命令
            /ice clear - 清空所有任务
            /ice stop - 停止 ICE
            /ice start - 启动 ICE
            /ice add | remove | toggle | only
            /ice flag [id] - 打开地图并标记采集区域
            """);
        EzCmd.Add("/ice", OnCommand);
        EzCmd.Add("/IceCosmic", OnCommand);
        Init();
        Svc.Framework.Update += Tick;
        Svc.PluginInterface.UiBuilder.Draw += OnDraw;

        TaskManager = new(new(showDebug: false, timeLimitMS: 10 * 60 * 3000));
        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            mainWindow.IsOpen = true;
        };
        Svc.PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            mainWindow.IsOpen = true;
            C.SelectedTab = WindowSelection.MiscSettings;
        };

        // timer stuff
        MissionTimer = new MissionTimer();

        DictionaryCreation();
        Task_Gamba.EnsureGambaWeightsInitialized();
        CosmicHelper.UpdateCriticalWeather();
        TestLoadRoutes();
        CosmicHelper.Task_UpdateRelicMissionInfo();
        GatheringRouteLoader.LoadAllRoutes();

        MigrateConfigSettings();
        _ = Sounds.SoundPlayer.InitializeAsync();

        UpdateMissingGathering();
    }

    private static void Init()
    {
        ExcelHelper.Init();
        ConsumableInfo.Init();
    }

    private void Tick(object _)
    {
        if (PlayerHelper.IsInCosmicZone())
        {
            if (Player.Available)
            {
                if (EzThrottler.Throttle("Update Character Stats"))
                {
                    CosmicHelper.Task_UpdateRelicMissionInfo();
                }


                PlayerHandlers.Tick();
                if (SchedulerMain.State != IceState.Idle)
                    SchedulerMain.Tick();
                WeatherForecastHandler.Tick();
            }
            else
            {
                if (SchedulerMain.State != IceState.Idle)
                    PlayerHandlers.DisablePlugin();
                if (PlayerHandlers.PlayerFirstCosmicZone)
                    PlayerHandlers.PlayerFirstCosmicZone = false;
            }
            GenericManager.Tick();
            TextAdvancedManager.Tick();
            YesAlreadyManager.Tick();
        }
        else
        {
            if (SchedulerMain.State != IceState.Idle)
                SchedulerMain.DisablePlugin();
        }
    }

    private void OnDraw()
    {
        if (PlayerHelper.IsInCosmicZone() && Player.Available)
        {
            

            PictoManager.DrawPicto();
        }
    }

    public void Dispose()
    {
        GenericHelpers.Safe(() => Svc.Framework.Update -= Tick);
        GenericHelpers.Safe(() => Svc.PluginInterface.UiBuilder.Draw -= OnDraw);
        GenericHelpers.Safe(() => Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw);
        GenericHelpers.Safe(TextAdvancedManager.UnlockTA);
        GenericHelpers.Safe(YesAlreadyManager.Unlock);
        GenericHelpers.Safe(PictoService.Dispose);
        ECommonsMain.Dispose();
        PictoService.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        var subcommands = args.Split(' ');

        if (subcommands.Length == 0 || args == "")
        {
            mainWindow.IsOpen = !mainWindow.IsOpen;
            return;
        }

        var firstArg = subcommands[0];

        if (firstArg.ToLower() == "d" || firstArg.ToLower() == "debug")
        {
            debugWindow.IsOpen = true;
            return;
        }
        else if (firstArg.ToLower() == "i")
        {
            infoWindow.IsOpen = true;
            return;
        }
        else if (firstArg.ToLower() == "s" || firstArg.ToLower() == "settings")
        {
            mainWindow.IsOpen = true;
            C.SelectedTab = WindowSelection.MiscSettings;
            return;
        }
        else if (firstArg.ToLower() == "clear")
        {
            foreach (var mission in C.MissionConfig)
            {
                mission.Value.Enabled = false;
            }
            C.Save();
        }
        else if (firstArg.ToLower() == "stop")
        {
            SchedulerMain.DisablePlugin();
        }
        else if (firstArg.ToLower() == "start")
        {
            SchedulerMain.EnablePlugin();
        }
        else if (firstArg.ToLower() == "add")
        {
            uint[] ids = [.. subcommands.Skip(1).Select(uint.Parse)];
            var idSet = new HashSet<uint>(ids);
            if (ids.Length == 0) return;

            foreach (var id in idSet)
            {
                if (C.MissionConfig.TryGetValue(id, out var mission))
                {
                    mission.Enabled = true;
                }
            }
            C.Save();
        }
        else if (firstArg.ToLower() == "remove")
        {
            uint[] ids = [.. subcommands.Skip(1).Select(uint.Parse)];
            var idSet = new HashSet<uint>(ids);
            if (ids.Length == 0) return;

            foreach (var id in idSet)
            {
                if (C.MissionConfig.TryGetValue(id, out var mission))
                {
                    mission.Enabled = false;
                }
            }
            C.Save();
        }
        else if (firstArg.ToLower() == "toggle")
        {
            uint[] ids = [.. subcommands.Skip(1).Select(uint.Parse)];
            var idSet = new HashSet<uint>(ids);
            if (ids.Length == 0) return;

            foreach (var id in idSet)
            {
                if (C.MissionConfig.TryGetValue(id, out var mission))
                {
                    mission.Enabled = !mission.Enabled;
                }
            }
            C.Save();
        }
        else if (firstArg.ToLower() == "only")
        {
            uint[] ids = [.. subcommands.Skip(1).Select(uint.Parse)];
            var idSet = new HashSet<uint>(ids);
            if (ids.Length == 0) return;

            foreach (var mission in C.MissionConfig.Where(x => x.Value.Enabled))
            {
                mission.Value.Enabled = false;
            }
            foreach (var id in idSet)
            {
                if (C.MissionConfig.TryGetValue(id, out var mission))
                {
                    mission.Enabled = true;
                }
            }
        }
        else if (firstArg.ToLower() == "flag")
        {
            if (subcommands.Length != 2) return;
            if (!PlayerHelper.IsInCosmicZone()) return;

            int missionId = int.Parse(subcommands[1]);
            var info = CosmicHelper.SheetMissionDict.FirstOrDefault(mission => mission.Key == missionId);
            if (info.Value == default) return;
            if (info.Value.MarkerId == 0) return;

            Utils.SetGatheringRing(info.Value.TerritoryId, (int)info.Value.MapPosition.X, (int)info.Value.MapPosition.Y, info.Value.Radius, info.Value.Name);
        }
        else if (firstArg.ToLower() == "help")
        {
            string helpMessage = $"- - ICE 命令帮助 - - \n" +
                                 $"/ice help - 显示所有可用命令\n" +
                                 $"/ice - 打开主界面\n" +
                                 $"/ice s - 打开设置界面\n" +
                                 $" - - - 任务相关 - - - \n" +
                                 $"/ice stop - 停止 ICE\n" +
                                 $"/ice start - 启动 ICE\n" +
                                 $"以下命令支持单个或多个任务 ID（空格分隔）\n" +
                                 $"例：/ice add 10 155 185\n" +
                                 $"/ice add (ids) - 启用指定任务\n" +
                                 $"/ice remove (ids) - 禁用/移除指定任务\n" +
                                 $"/ice toggle (ids) - 切换指定任务状态\n" +
                                 $"/ice only (ids) - 仅保留指定任务为启用\n" +
                                 $"/ice flag (id) - 打开地图并标记该任务区域（若存在）\n";
            Svc.Chat.Print(helpMessage);
        }
    }

    public void TestLoadRoutes()
    {

    }
}
