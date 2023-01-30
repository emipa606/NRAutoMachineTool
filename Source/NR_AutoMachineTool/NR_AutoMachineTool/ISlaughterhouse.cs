using System.Collections.Generic;
using Verse;

namespace NR_AutoMachineTool;

internal interface ISlaughterhouse
{
    Dictionary<ThingDef, SlaughterSettings> Settings { get; }
}