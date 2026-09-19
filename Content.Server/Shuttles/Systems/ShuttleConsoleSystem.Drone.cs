using Content.Server.Shuttles.Events;
using Content.Shared.Shuttles.Components;
using Content.Shared.Station.Components;
using Content.Shared.UserInterface;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleConsoleSystem
{
    /// <summary>
    /// Gets the drone console target if applicable otherwise returns itself.
    /// </summary>
    public EntityUid? GetDroneConsole(EntityUid consoleUid)
    {
        var getShuttleEv = new ConsoleShuttleEvent
        {
            Console = consoleUid,
        };

        RaiseLocalEvent(consoleUid, ref getShuttleEv);
        return getShuttleEv.Console;
    }

    [SubscribeLocalEvent]
    private void OnDronePilotConsoleOpen(Entity<DroneConsoleComponent> ent, ref AfterActivatableUIOpenEvent args)
    {
        ent.Comp.Entity = GetShuttleConsole(ent);
    }

    [SubscribeLocalEvent]
    private void OnDronePilotConsoleClose(Entity<DroneConsoleComponent> ent, ref BoundUIClosedEvent args)
    {
        // Only if last person closed UI.
        if (!_ui.IsUiOpen(ent.Owner, args.UiKey))
            ent.Comp.Entity = null;
    }

    [SubscribeLocalEvent]
    private void OnCargoGetConsole(Entity<DroneConsoleComponent> ent, ref ConsoleShuttleEvent args)
    {
        args.Console = GetShuttleConsole(ent);
    }

    /// <summary>
    /// Gets the relevant shuttle console to proxy from the drone console.
    /// </summary>
    private EntityUid? GetShuttleConsole(Entity<DroneConsoleComponent> ent)
    {
        var stationUid = _station.GetOwningStation(ent);

        if (stationUid == null)
            return null;

        // I know this sucks but needs device linking or something idunno
        var query = AllEntityQuery<ShuttleConsoleComponent, TransformComponent>();

        while (query.MoveNext(out var cUid, out _, out var xform))
        {
            if (xform.GridUid == null ||
                !TryComp<StationMemberComponent>(xform.GridUid, out var member) ||
                member.Station != stationUid)
            {
                continue;
            }

            foreach (var compType in ent.Comp.Components.Values)
            {
                if (!HasComp(xform.GridUid, compType.Component.GetType()))
                    continue;

                return cUid;
            }
        }

        return null;
    }
}
