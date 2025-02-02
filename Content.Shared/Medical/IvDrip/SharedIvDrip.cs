using Robust.Shared.Serialization;


namespace Content.Shared.Medical.IvDrip
{
    public sealed class SharedIvDrip
    {
        public static string BagSlotId = "bagSlot";
    }

    [Serializable, NetSerializable]
    public enum IvDripVisualState : byte
    {
        BagAttached,
        Color,
        FillFraction
    }
}
