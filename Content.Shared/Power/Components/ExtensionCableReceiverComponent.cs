using Robust.Shared.GameStates;

namespace Content.Shared.Power.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
//[Access(typeof(ExtensionCableSystem))]
public sealed partial class ExtensionCableReceiverComponent : Component
{
    /// <summary>
    /// <see cref="ExtensionCableProviderComponent"/> that is supplying power to this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Provider;

    /// <summary>
    /// Is power this entity in the state to receive power.
    /// </summary>
    [DataField]
    public bool Connectable = false;

    /// <summary>
    /// The max distance from a <see cref="ExtensionCableProviderComponent"/> that this can receive power from.
    /// </summary>
    [DataField]
    public int ReceptionRange = 3;

    /// <summary>
    /// The max distance from a <see cref="ExtensionCableProviderComponent"/> that this can receive power from.
    /// </summary>
    [DataField]
    public EntityUid? ExtensionCordEntity;
}
