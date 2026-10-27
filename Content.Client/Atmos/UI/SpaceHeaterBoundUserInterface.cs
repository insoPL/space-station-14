using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Atmos.UI;

/// <summary>
/// Initializes a <see cref="SpaceHeaterWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class SpaceHeaterBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SpaceHeaterWindow? _window;

    public SpaceHeaterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SpaceHeaterWindow>();

        _window.ToggleStatusButton.OnToggled += _ => OnToggleStatusButtonPressed();
        _window.IncreaseTempRange.OnPressed += _ => OnTemperatureRangeChanged(_window.TemperatureChangeDelta);
        _window.DecreaseTempRange.OnPressed += _ => OnTemperatureRangeChanged(-_window.TemperatureChangeDelta);
        _window.ModeSelector.OnItemSelected += OnModeChanged;

        _window.PowerLevelSelector.OnItemSelected += OnPowerLevelChange;

        Update();
    }

    private void OnToggleStatusButtonPressed()
    {
        SendPredictedMessage(new SpaceHeaterToggleMessage());
    }

    private void OnTemperatureRangeChanged(float changeAmount)
    {
        SendPredictedMessage(new SpaceHeaterChangeTemperatureMessage(changeAmount));
    }

    private void OnModeChanged(OptionButton.ItemSelectedEventArgs args)
    {
        _window?.ModeSelector.SelectId(args.Id);
        SendPredictedMessage(new SpaceHeaterChangeModeMessage((SpaceHeaterMode)args.Id));
    }

    private void OnPowerLevelChange(RadioOptionItemSelectedEventArgs<int> args)
    {
        _window?.PowerLevelSelector.Select(args.Id);
        SendPredictedMessage(new SpaceHeaterChangePowerLevelMessage((SpaceHeaterPowerLevel)args.Id));
    }

    public override void Update()
    {
        base.Update();

        if (_window == null
            || !EntMan.TryGetComponent(Owner, out GasThermoMachineComponent? gasThermoMachine)
            || !EntMan.TryGetComponent(Owner, out SpaceHeaterComponent? spaceHeaterComp))
            return;

        _window.ToggleStatusButton.Pressed = spaceHeaterComp.Active;
        _window.ModeSelector.SelectId((int)spaceHeaterComp.Mode);
        _window.PowerLevelSelector.Select((int)spaceHeaterComp.PowerLevel);

        _window.SetTemperature(gasThermoMachine.TargetTemperature, gasThermoMachine.MinTemperature, gasThermoMachine.MaxTemperature);
    }

    protected override void Dispose(bool disposing)
    {
        Close();
        if (_window == null)
            return;
        _window.Close();
    }
}
