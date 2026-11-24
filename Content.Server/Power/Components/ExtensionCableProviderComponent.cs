using Content.Server.Power.EntitySystems;

namespace Content.Server.Power.Components;

[RegisterComponent]
[Access(typeof(ExtensionCableSystem))]
public sealed partial class ExtensionCableProviderComponent : Component
{
    /// <summary>
    /// The max distance this can connect to <see cref="ExtensionCableReceiverComponent"/>s from.
    /// </summary>
    [DataField]
    public int TransferRange { get; set; } = 3;

    /// <summary>
    /// List of power recivers connected to this cable.
    /// </summary>
    [ViewVariables]
    public List<EntityUid> LinkedReceivers { get; } = new();

    /// <summary>
    ///     If <see cref="ExtensionCableReceiverComponent"/>s should consider connecting to this.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Connectable { get; set; } = true;
}
