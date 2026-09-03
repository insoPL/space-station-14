using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class BadgePrintUiMessageEvent : CartridgeMessageEvent
{
    public readonly SelectedBadgeTimer Timer;
    public readonly SelectedDepartment Dept;

    public BadgePrintUiMessageEvent(SelectedBadgeTimer timer, SelectedDepartment dept)
    {
        Timer = timer;
        Dept = dept;
    }
}

[Serializable, NetSerializable]
public enum SelectedBadgeTimer
{
    Print5,
    Print10,
    Print15,
    Print25
}

[Serializable, NetSerializable]
public enum SelectedDepartment
{
    All,
    Command,
    Security,
    Medical,
    Engineering,
    Research,
    Cargo,
    Service
}
