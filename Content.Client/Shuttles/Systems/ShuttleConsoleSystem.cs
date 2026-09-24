using Content.Shared.Input;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Robust.Client.Input;
using Robust.Client.Player;

namespace Content.Client.Shuttles.Systems;

public sealed partial class ShuttleConsoleSystem : SharedShuttleConsoleSystem
{
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        var shuttle = _input.Contexts.New("shuttle", "common");
        shuttle.AddFunction(ContentKeyFunctions.ShuttleStrafeUp);
        shuttle.AddFunction(ContentKeyFunctions.ShuttleStrafeDown);
        shuttle.AddFunction(ContentKeyFunctions.ShuttleStrafeLeft);
        shuttle.AddFunction(ContentKeyFunctions.ShuttleStrafeRight);
        shuttle.AddFunction(ContentKeyFunctions.ShuttleRotateLeft);
        shuttle.AddFunction(ContentKeyFunctions.ShuttleRotateRight);
        shuttle.AddFunction(ContentKeyFunctions.ShuttleBrake);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _input.Contexts.Remove("shuttle");
    }

    protected override void HandlePilotShutdown(Entity<PilotComponent> ent, ref ComponentShutdown args)
    {
        base.HandlePilotShutdown(ent, ref args);
        if (_playerManager.LocalEntity != ent) return;

        _input.Contexts.SetActiveContext("human");
    }

    [SubscribeLocalEvent]
    private void OnAfterHandleState(Entity<PilotComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var console = ent.Comp.Console;

        if (console == null)
        {
            _input.Contexts.SetActiveContext("human");
            return;
        }

        if (!HasComp<ShuttleConsoleComponent>(console))
        {
            Log.Warning($"Unable to set Helmsman console to {console}");
            return;
        }

        ActionBlockerSystem.UpdateCanMove(ent);
        _input.Contexts.SetActiveContext("shuttle");
    }
}
