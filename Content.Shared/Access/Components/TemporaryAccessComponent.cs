using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;


namespace Content.Shared.Access.Components;

/// <summary>
/// Provides temporary access
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TemporaryAccessSystem))]
public sealed partial class TemporaryAccessComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Expired;

    [DataField, AutoNetworkedField]
    public TimeSpan AccessExpireTime;

    [DataField, AutoNetworkedField]
    public TimeSpan ExpireTime;
}
