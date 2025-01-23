using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Medical
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
