using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Shared.Access.Components;

/// <summary>
/// Provides temporary access
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TemporaryAccessComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Expired;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan AccessExpireTime;

    [DataField, AutoNetworkedField]
    public TimeSpan ExpireTime;

    [DataField, AutoNetworkedField]
    public TimeSpan PrintCooldownTime = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan PrintCooldownTimer;
}
