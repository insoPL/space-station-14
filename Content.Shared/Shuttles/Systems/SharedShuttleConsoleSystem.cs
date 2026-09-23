using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Systems;

public abstract partial class SharedShuttleConsoleSystem : EntitySystem
{
    [Dependency] protected ActionBlockerSystem ActionBlockerSystem = default!;

    [Serializable, NetSerializable]
    protected sealed class PilotComponentState : ComponentState
    {
        public NetEntity? Console { get; }

        public PilotComponentState(NetEntity? uid)
        {
            Console = uid;
        }
    }

    [SubscribeLocalEvent]
    protected virtual void HandlePilotShutdown(Entity<PilotComponent> ent, ref ComponentShutdown args)
    {
        ActionBlockerSystem.UpdateCanMove(ent);
    }

    [SubscribeLocalEvent]
    private void OnStartup(Entity<PilotComponent> ent, ref ComponentStartup args)
    {
        ActionBlockerSystem.UpdateCanMove(ent);
    }

    [SubscribeLocalEvent]
    private void HandleMovementBlock(Entity<PilotComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;
        if (ent.Comp.Console == null)
            return;

        args.Cancel();
    }
}
