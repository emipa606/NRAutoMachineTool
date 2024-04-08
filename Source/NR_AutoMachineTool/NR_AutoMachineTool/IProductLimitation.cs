using NR_AutoMachineTool.Utilities;
using RimWorld;

namespace NR_AutoMachineTool;

internal interface IProductLimitation
{
    int ProductLimitCount { get; set; }

    bool ProductLimitation { get; set; }

    Option<SlotGroup> TargetSlotGroup { get; set; }
}