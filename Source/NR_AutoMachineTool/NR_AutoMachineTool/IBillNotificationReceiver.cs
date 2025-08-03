using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public interface IBillNotificationReceiver
{
    void OnComplete(Bill_Production bill, List<Thing> ingredients);
}