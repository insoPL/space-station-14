namespace Content.Server.Implants.Components;

/// <summary>
/// Allows connecting your brain to clone so you can be reborn after death.
/// </summary>
[RegisterComponent]
public sealed partial class MindBackupComponent : Component
{
    /// <summary>
    /// unique ID of Implant
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public String ID = "123ABC";



}
