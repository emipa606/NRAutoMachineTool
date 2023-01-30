using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public abstract class Building_BaseLimitation<T> : Building_BaseMachine<T>, IProductLimitation where T : Thing
{
    private bool productLimitation;

    private int productLimitCount = 100;

    private ILoadReferenceable slotGroupParent;

    private string slotGroupParentLabel;

    public int ProductLimitCount
    {
        get => productLimitCount;
        set => productLimitCount = value;
    }

    public bool ProductLimitation
    {
        get => productLimitation;
        set => productLimitation = value;
    }

    public Option<SlotGroup> TargetSlotGroup { get; set; } = Ops.Nothing<SlotGroup>();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref productLimitCount, "productLimitCount", 100);
        Scribe_Values.Look(ref productLimitation, "productLimitation");
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            slotGroupParentLabel = TargetSlotGroup.Select(s => s.parent.SlotYielderLabel()).GetOrDefault(null);
            slotGroupParent = (from s in TargetSlotGroup
                select s.parent
                into p
                select p as ILoadReferenceable).GetOrDefault(null);
        }

        Scribe_References.Look(ref slotGroupParent, "slotGroupParent");
        Scribe_Values.Look(ref slotGroupParentLabel, "slotGroupParentLabel");
    }

    public override void PostMapInit()
    {
        TargetSlotGroup = (from g in Map.haulDestinationManager.AllGroups
            where g.parent.SlotYielderLabel() == slotGroupParentLabel
            where Ops.Option(slotGroupParent).Fold(true)(p => p == g.parent)
            select g).FirstOption();
        base.PostMapInit();
    }

    public bool IsLimit(ThingDef def)
    {
        if (!ProductLimitation)
        {
            return false;
        }

        TargetSlotGroup = TargetSlotGroup.Where(s => Map.haulDestinationManager.AllGroups.Any(a => a == s));
        return TargetSlotGroup.Fold(() => Map.resourceCounter.GetCount(def) >= ProductLimitCount)(s =>
            (from t in s.HeldThings
                where t.def == def
                select t.stackCount).Sum() >= ProductLimitCount || !s.Settings.filter.Allows(def) ||
            !s.CellsList.Any(c => c.GetFirstItem(Map) == null || c.GetFirstItem(Map).def == def));
    }

    public bool IsLimit(Thing thing)
    {
        if (!ProductLimitation)
        {
            return false;
        }

        TargetSlotGroup = TargetSlotGroup.Where(s => Map.haulDestinationManager.AllGroups.Any(a => a == s));
        return TargetSlotGroup.Fold(() => Map.resourceCounter.GetCount(thing.def) >= ProductLimitCount)(s =>
            (from t in s.HeldThings
                where t.def == thing.def
                select t.stackCount).Sum() >= ProductLimitCount ||
            !s.CellsList.Any(c => c.IsValidStorageFor(Map, thing)));
    }
}