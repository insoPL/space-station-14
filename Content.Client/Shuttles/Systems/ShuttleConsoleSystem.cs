using Content.Shared.Input;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.GameStates;

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

    protected override void HandlePilotShutdown(Entity<PilotComponent> ent, ComponentShutdown args)
    {
        base.HandlePilotShutdown(ent, args);
        if (_playerManager.LocalEntity != ent) return;

        _input.Contexts.SetActiveContext("human");
    }

    [SubscribeLocalEvent]
    private void OnHandleState(Entity<PilotComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not PilotComponentState state) return;

        var console = EnsureEntity<PilotComponent>(state.Console, ent);

        if (console == null)
        {
            ent.Comp.Console = null;
            _input.Contexts.SetActiveContext("human");
            return;
        }

        if (!HasComp<ShuttleConsoleComponent>(console))
        {
            Log.Warning($"Unable to set Helmsman console to {console}");
            return;
        }

        ent.Comp.Console = console;
        ActionBlockerSystem.UpdateCanMove(ent);
        _input.Contexts.SetActiveContext("shuttle");
    }
}
