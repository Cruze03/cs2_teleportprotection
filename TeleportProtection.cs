using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Config;

namespace TeleportProtection;

[MinimumApiVersion(111)]
public class TeleportProtection : BasePlugin, IPluginConfig<TeleportProtectionConfig>
{
    public override string ModuleName => "Teleport Protection";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Cruze";
    public override string ModuleDescription => "Gives player god mode for X seconds when they hit trigger_teleport";
    
    public bool[] g_bGodmode = new bool[64];
    public CounterStrikeSharp.API.Modules.Timers.Timer?[] g_hProtectionTimer = new CounterStrikeSharp.API.Modules.Timers.Timer[65];
    public CounterStrikeSharp.API.Modules.Timers.Timer? g_hSpawnProtectionTimer = null;
    public bool g_bSpawnProtection = true;
    public TeleportProtectionConfig Config { get; set; } = new();

    public void OnConfigParsed(TeleportProtectionConfig config)
    {
        Config = config;
        
        if(Config.ProtectionTime <= 0.0f)
        {
            Config.ProtectionTime = 1.0f;
        }
    }

    public override void Load(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    private HookResult OnTakeDamage(DynamicHook h)
    {
        var entindex = h.GetParam<CEntityInstance>(0).Index;
    
        if (entindex == 0)
            return HookResult.Continue;

        var pawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>((int)entindex);
        
        if (pawn.OriginalController.Value is not { } player)
            return HookResult.Continue;

        var damageinfo = h.GetParam<CTakeDamageInfo>(1);
        
        if(g_bGodmode[player.Slot])
        {
            damageinfo.Damage = 0;
        }
        
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        for(int i = 0; i <= Server.MaxPlayers; i++)
        {
            g_bGodmode[i] = false;
            DeleteTimer(g_hProtectionTimer[i]);
            g_hProtectionTimer[i] = null;
        }
        g_bSpawnProtection = true;

        DeleteTimer(g_hSpawnProtectionTimer);
        if(Config.RoundStartDelay > 0)
            g_hSpawnProtectionTimer = AddTimer(Config.RoundStartDelay, () => Timer_DisableSpawnProtection());
        else
            g_bSpawnProtection = false;
        return HookResult.Continue;
    }

    public void Timer_DisableSpawnProtection()
    {
        g_hSpawnProtectionTimer = null;
        g_bSpawnProtection = false;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if(!Config.RemoveProtectionOnWeaponFire)
        {
            return HookResult.Continue;
        }
        
        if(@event.Userid == null || @event.Userid.IsBot || @event.Userid.IsHLTV)
            return HookResult.Continue;
        
        int slot = @event.Userid.Slot;
        
        if(g_hProtectionTimer[slot] != null)
        {
            g_bGodmode[slot] = false;
            DeleteTimer(g_hProtectionTimer[slot]);
            g_hProtectionTimer[slot] = null;
            PrintToChat(@event.Userid, Config.TeleportProtectionRemoveMessage);
        }
        return HookResult.Continue;
    }

    [EntityOutputHook("trigger_teleport", "OnStartTouch")]
    public HookResult OnTouchStart(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if(g_bSpawnProtection)
        {
            return HookResult.Continue;
        }
        
        var pawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>((int)activator.Index);

        if (pawn.OriginalController.Value is not { } player)
        {
            return HookResult.Continue;
        }
        
        if(player.IsBot || player.IsHLTV)
            return HookResult.Continue;
        
        if(g_hProtectionTimer[player.Slot] == null)
		{
            g_hProtectionTimer[player.Slot] = AddTimer(Config.ProtectionTime, () => Timer_ResetPlayer(player, player.Slot));
            if(!string.IsNullOrWhiteSpace(Config.TeleportProtectionMessage))
                PrintToChat(player, Config.TeleportProtectionMessage);
        }
        return HookResult.Continue;
    }

    public void Timer_ResetPlayer(CCSPlayerController player, int slot)
    {
        g_hProtectionTimer[slot] = null;
        g_bGodmode[slot] = false;
        if(player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }
        if(!string.IsNullOrWhiteSpace(Config.TeleportProtectionRemoveMessage))
            PrintToChat(player, Config.TeleportProtectionRemoveMessage);
    }

    public string ReplaceTags(string text)
    {
        text = text.Replace("{DEFAULT}", $"{ChatColors.Default}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{WHITE}", $"{ChatColors.White}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{DARKRED}", $"{ChatColors.Darkred}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{GREEN}", $"{ChatColors.Green}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{LIGHTYELLOW}", $"{ChatColors.LightYellow}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{LIGHTBLUE}", $"{ChatColors.LightBlue}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{OLIVE}", $"{ChatColors.Olive}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{LIME}", $"{ChatColors.Lime}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{RED}", $"{ChatColors.Red}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{PURPLE}", $"{ChatColors.Purple}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{GRAY}", $"{ChatColors.Grey}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{GREY}", $"{ChatColors.Grey}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{YELLOW}", $"{ChatColors.Yellow}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{GOLD}", $"{ChatColors.Gold}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{SILVER}", $"{ChatColors.Silver}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{BLUE}", $"{ChatColors.Blue}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{DARKBLUE}", $"{ChatColors.DarkBlue}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{BLUEGREY}", $"{ChatColors.BlueGrey}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{MAGENTA}", $"{ChatColors.Magenta}", StringComparison.OrdinalIgnoreCase);
        text = text.Replace("{LIGHTRED}", $"{ChatColors.LightRed}", StringComparison.OrdinalIgnoreCase);

        text = text.Replace("{DELAY}", Config.ProtectionTime.ToString(), StringComparison.OrdinalIgnoreCase);

	    return text;
    }

    private static void LogError(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void DeleteTimer(CounterStrikeSharp.API.Modules.Timers.Timer? timer)
    {
        if(timer != null)
        {
            timer.Kill();
        }
    }

    public void PrintToChat(CCSPlayerController player, string text)
    {
        player.PrintToChat($" {ReplaceTags(text)}");
    }
}