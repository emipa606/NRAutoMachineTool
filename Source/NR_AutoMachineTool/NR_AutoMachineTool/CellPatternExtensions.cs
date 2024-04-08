using NR_AutoMachineTool.Utilities;
using UnityEngine;

namespace NR_AutoMachineTool;

public static class CellPatternExtensions
{
    public static Color ToColor(this CellPattern pat)
    {
        return pat switch
        {
            CellPattern.BlurprintMin => Color.white,
            CellPattern.BlurprintMax => Color.gray.A(0.6f),
            CellPattern.Instance => Color.white,
            CellPattern.OtherInstance => Color.white.A(0.35f),
            CellPattern.OutputCell => Color.blue,
            CellPattern.OutputZone => Color.blue.A(0.5f),
            CellPattern.InputCell => Color.magenta,
            CellPattern.InputZone => Color.magenta.A(0.5f),
            _ => Color.white
        };
    }
}