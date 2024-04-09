using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace CS2Knife;

public class Plugin : BasePlugin
{
    public override string ModuleName => "knife";
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "kokona";
    public override string ModuleDescription => "https://github.com/lsm0318";

    private readonly Dictionary<int, bool> _cmdCooldown = new();

    public override void Load(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        Console.WriteLine("CS2Knife Plugin loaded");
    }

    public override void Unload(bool hotReload)
    {
        Console.WriteLine("CS2Knife Plugin unloaded");
    }

    private HookResult OnTakeDamage(DynamicHook hook)
    {
        var entity = hook.GetParam<CEntityInstance>(0);
        var damageInfo = hook.GetParam<CTakeDamageInfo>(1);
        var attacker = new CCSPlayerPawn(damageInfo.Attacker.Value!.Handle);
        var victim = new CCSPlayerController(entity.Handle);

        if (attacker.TeamNum == victim.TeamNum)
        {
            if (damageInfo is { BitsDamageType: 4, AmmoType: 255 })
            {
                return HookResult.Handled;
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _cmdCooldown.Clear();

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if (@event.Text.ToLower() is "!drop" or "!d" or ".d")
        {
            GivePlayerKnife(@event.Userid);
        }

        return HookResult.Continue;
    }

    private void GivePlayerKnife(int userId)
    {
        if (_cmdCooldown.TryGetValue(userId, out var isCooldown) && isCooldown)
        {
            return;
        }

        var player = Utilities.GetPlayerFromUserid(userId);

        if (!player.IsValid || !player.PawnIsAlive)
        {
            return;
        }

        for (var i = 0; i < 5; i++)
        {
            player.GiveNamedItem(CsItem.Knife);
        }

        _cmdCooldown[userId] = true;
    }
}