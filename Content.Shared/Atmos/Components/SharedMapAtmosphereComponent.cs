using Content.Shared.Atmos.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Components;

[NetworkedComponent]
public abstract partial class SharedMapAtmosphereComponent : Component
{
    [ViewVariables] public SharedGasTileOverlaySystem.SharedFireData FireOverlay;
    [ViewVariables] public SharedGasTileOverlaySystem.SharedVisibleGasData VisibleGasOverlay;
    [ViewVariables] public SharedGasTileOverlaySystem.SharedGasTemperatureData GasTemperatureOverlay;
}

[Serializable, NetSerializable]
public sealed class MapAtmosphereComponentState : ComponentState
{
    public SharedGasTileOverlaySystem.SharedFireData FireOverlay;
    public SharedGasTileOverlaySystem.SharedVisibleGasData VisibleGasOverlay;
    public SharedGasTileOverlaySystem.SharedGasTemperatureData GasTemperatureOverlay;

    public MapAtmosphereComponentState(SharedGasTileOverlaySystem.SharedFireData fireOverlay, SharedGasTileOverlaySystem.SharedVisibleGasData visibleGasOverlay, SharedGasTileOverlaySystem.SharedGasTemperatureData gasTemperatureOverlay)
    {
        FireOverlay = fireOverlay;
        VisibleGasOverlay = visibleGasOverlay;
        GasTemperatureOverlay = gasTemperatureOverlay;
    }
}
