using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Shared.Access.Components;

/// <summary>
/// Provides temporary access
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class TemporaryAccessComponent : Component
{
    /// <summary>
    /// Is the temporary access expired? If so, the access is disabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Expired = false;

    /// <summary>
    /// How long the access will last for, from the time of creation.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan AccessExpireTime = TimeSpan.FromMinutes(5);


    /// <summary>
    /// The time at which the temporary access will expire.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan ExpireTime = TimeSpan.Zero;
}
