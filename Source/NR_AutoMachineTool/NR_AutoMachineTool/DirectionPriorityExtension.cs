using Verse;

namespace NR_AutoMachineTool;

public static class DirectionPriorityExtension
{
    public static string ToText(this DirectionPriority pri)
    {
        return ("NR_AutoMachineTool_Conveyor.DirectionPriority." + pri).Translate();
    }
}