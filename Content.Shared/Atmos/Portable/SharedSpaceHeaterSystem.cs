using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.UserInterface;

namespace Content.Shared.Atmos.Portable;

public sealed partial class SharedSpaceHeaterSystem : EntitySystem
{
    [Dependency] private SharedPowerReceiverSystem _receiver = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private EntityQuery<GasThermoMachineComponent> _thermoQuery = default!;

    [SubscribeLocalEvent]
    private void OnInit(Entity<SpaceHeaterComponent> ent, ref MapInitEvent args)
    {
        if (!_thermoQuery.TryComp(ent, out var thermoMachine))
            return;

        thermoMachine.Cp = ent.Comp.HeatingCp;
        thermoMachine.HeatCapacity = ent.Comp.PowerConsumption;
        Dirty(ent, thermoMachine);
    }

    [SubscribeLocalEvent]
    private void OnBeforeOpened(Entity<SpaceHeaterComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnUIActivationAttempt(Entity<SpaceHeaterComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (Comp<TransformComponent>(ent).Anchored)
            return;

        if (!args.Silent)
            _popup.PopupEntity(Loc.GetString("comp-space-heater-unanchored", ("device", Loc.GetString("comp-space-heater-device-name"))), ent, args.User);

        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnPowerChanged(Entity<SpaceHeaterComponent> ent, ref PowerChangedEvent args)
    {
        UpdateAppearance(ent);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnToggle(Entity<SpaceHeaterComponent> ent, ref SpaceHeaterToggleMessage args)
    {
        ent.Comp.Active = _receiver.TogglePower(ent, user: args.Actor);
        UpdateAppearance(ent);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnTemperatureChanged(Entity<SpaceHeaterComponent> ent, ref SpaceHeaterChangeTemperatureMessage args)
    {
        if (!_thermoQuery.TryComp(ent, out var thermoMachine))
            return;

        thermoMachine.TargetTemperature = float.Clamp(thermoMachine.TargetTemperature + args.Temperature,
                                                      ent.Comp.MinTemperature,
                                                      ent.Comp.MaxTemperature);

        UpdateAppearance(ent);
        Dirty(ent, thermoMachine);

        if (_ui.TryGetOpenUi(ent.Owner, SpaceHeaterUiKey.Key, out var bui))
            bui.Update();
    }

    [SubscribeLocalEvent]
    private void OnModeChanged(Entity<SpaceHeaterComponent> ent, ref SpaceHeaterChangeModeMessage args)
    {
        if (!_thermoQuery.TryComp(ent, out var thermoMachine))
            return;

        ent.Comp.Mode = args.Mode;
        Dirty(ent);

        if (ent.Comp.Mode == SpaceHeaterMode.Heat)
            thermoMachine.Cp = ent.Comp.HeatingCp;
        else if (ent.Comp.Mode == SpaceHeaterMode.Cool)
            thermoMachine.Cp = ent.Comp.CoolingCp;
        Dirty(ent, thermoMachine);

        if (_ui.TryGetOpenUi(ent.Owner, SpaceHeaterUiKey.Key, out var bui))
            bui.Update();
    }

    [SubscribeLocalEvent]
    private void OnPowerLevelChanged(Entity<SpaceHeaterComponent> ent, ref SpaceHeaterChangePowerLevelMessage args)
    {
        if (!_thermoQuery.TryComp(ent, out var thermoMachine))
            return;

        ent.Comp.PowerLevel = args.PowerLevel;
        Dirty(ent);

        switch (ent.Comp.PowerLevel)
        {
            case SpaceHeaterPowerLevel.Low:
                thermoMachine.HeatCapacity = ent.Comp.PowerConsumption / 2;
                break;

            case SpaceHeaterPowerLevel.Medium:
                thermoMachine.HeatCapacity = ent.Comp.PowerConsumption;
                break;

            case SpaceHeaterPowerLevel.High:
                thermoMachine.HeatCapacity = ent.Comp.PowerConsumption * 2;
                break;
        }

        Dirty(ent, thermoMachine);

        if (_ui.TryGetOpenUi(ent.Owner, SpaceHeaterUiKey.Key, out var bui))
            bui.Update();
    }

    private void UpdateAppearance(EntityUid uid)
    {
        if (!_receiver.IsPowered(uid) || !_thermoQuery.TryComp(uid, out var thermoMachine))
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.Off);
        }
        else if (thermoMachine.LastEnergyDelta > 0)
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.Heating);
        }
        else if (thermoMachine.LastEnergyDelta < 0)
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.Cooling);
        }
        else
        {
            _appearance.SetData(uid, SpaceHeaterVisuals.State, SpaceHeaterState.StandBy);
        }
    }
}
