using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Portable;

namespace Content.Server.Atmos.Portable;

public sealed partial class SpaceHeaterSystem : SharedSpaceHeaterSystem
{
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private EntityQuery<GasThermoMachineComponent> _thermoQuery = default!;

    [SubscribeLocalEvent]
    private void OnDeviceUpdateded(Entity<SpaceHeaterComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        // If in automatic temperature mode, check if we need to adjust the heat exchange direction
        if (ent.Comp.Mode != SpaceHeaterMode.Auto || !ent.Comp.Active)
            return;

        var environment = _atmosphereSystem.GetTileMixture(ent.Owner);
        if (environment == null)
            return;

        if (!_thermoQuery.TryComp(ent, out var thermoMachine))
            return;

        float tolerance = thermoMachine.TemperatureTolerance + ent.Comp.AutoModeSwitchThreshold;

        bool isHeating = thermoMachine.Cp == ent.Comp.HeatingCp;
        bool isCooling = thermoMachine.Cp == ent.Comp.CoolingCp;



        //XD

        //TODO Make sure apparence setters dont go too hot
        if (environment.Temperature <= thermoMachine.TargetTemperature - tolerance)
        {
            thermoMachine.Cp = ent.Comp.HeatingCp;
        }
        else if (environment.Temperature >= thermoMachine.TargetTemperature + tolerance)
        {
            thermoMachine.Cp = ent.Comp.CoolingCp;
        }


        if (!thermoMachine.HysteresisState)
        {
            SetStandBy(ent);
        }
        else if (environment.Temperature <= thermoMachine.TargetTemperature - tolerance)
        {
            SetHeating(ent);
        }
        else if (environment.Temperature >= thermoMachine.TargetTemperature + tolerance)
        {
            SetCooling(ent);
        }
    }
}
