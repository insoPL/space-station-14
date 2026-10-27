using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Piping.Unary.Components;

namespace Content.Server.Atmos.Portable;

public sealed partial class SpaceHeaterSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;

    [SubscribeLocalEvent]
    private void OnDeviceUpdated(Entity<SpaceHeaterComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        if (!TryComp<GasThermoMachineComponent>(ent, out var thermoMachine))
        {
            return;
        }

        // If in automatic temperature mode, check if we need to adjust the heat exchange direction
        if (ent.Comp.Mode == SpaceHeaterMode.Auto)
        {
            var environment = _atmosphereSystem.GetContainingMixture(ent.Owner, args.Grid, args.Map);
            if (environment == null)
                return;

            if (environment.Temperature <= thermoMachine.TargetTemperature - (thermoMachine.TemperatureTolerance + ent.Comp.AutoModeSwitchThreshold))
            {
                thermoMachine.Cp = ent.Comp.HeatingCp;
            }
            else if (environment.Temperature >= thermoMachine.TargetTemperature + (thermoMachine.TemperatureTolerance + ent.Comp.AutoModeSwitchThreshold))
            {
                thermoMachine.Cp = ent.Comp.CoolingCp;
            }
            Dirty(ent, thermoMachine);
        }
    }

}
