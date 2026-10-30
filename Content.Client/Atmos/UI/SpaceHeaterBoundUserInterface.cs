using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

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
        _window.ToggleStatusButton.OnToggled += _ => ToggleStatus();
        _window.IncreaseTempRange.OnPressed += _ => ChangeTemperature(_window.TemperatureChangeDelta);
        _window.DecreaseTempRange.OnPressed += _ => ChangeTemperature(-_window.TemperatureChangeDelta);
        _window.ModeSelector.OnItemSelected += args => ChangeMode((SpaceHeaterMode)args.Id);
        _window.PowerLevelSelector.OnItemSelected += args => ChangePowerLevel((SpaceHeaterPowerLevel)args.Id);

        Update();
    }

    private void ToggleStatus()
    {
        SendPredictedMessage(new SpaceHeaterToggleMessage());
        Update();
    }

    private void ChangeTemperature(float delta)
    {
        SendPredictedMessage(new SpaceHeaterChangeTemperatureMessage(delta));
        Update();
    }

    private void ChangeMode(SpaceHeaterMode mode)
    {
        SendPredictedMessage(new SpaceHeaterChangeModeMessage(mode));
        Update();
    }

    private void ChangePowerLevel(SpaceHeaterPowerLevel level)
    {
        SendPredictedMessage(new SpaceHeaterChangePowerLevelMessage(level));
        Update();
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

        _window.SetTemperature(gasThermoMachine.TargetTemperature, spaceHeaterComp.MinTemperature, spaceHeaterComp.MaxTemperature);
    }

    protected override void Dispose(bool disposing)
    {
        Close();
        if (_window == null)
            return;
        _window.Close();
    }
}
