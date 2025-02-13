using System.Collections.Generic;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class Bill_ProductionNotifyComplete : Bill_Production
{
    public Bill_ProductionNotifyComplete(RecipeDef recipe) : base(recipe)
    {
    }

    public Bill_ProductionNotifyComplete()
    {
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        base.Notify_IterationCompleted(billDoer, ingredients);
        Ops.Option(billStack?.billGiver as IBillNotificationReceiver).ForEach(delegate(IBillNotificationReceiver r)
        {
            r.OnComplete(this, ingredients);
        });
    }
}