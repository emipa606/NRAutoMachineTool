using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class Bill_ProductionNotifyComplete(RecipeDef recipe) : Bill_Production(recipe)
{
    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        base.Notify_IterationCompleted(billDoer, ingredients);
        Ops.Option(billStack?.billGiver as IBillNotificationReceiver).ForEach(delegate(IBillNotificationReceiver r)
        {
            r.OnComplete(this, ingredients);
        });
    }
}