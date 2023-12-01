using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class ModSetting_AutoMachineTool : ModSettings
{
    public static readonly Func<BasicMachineSetting> BeltConveyorDefault = () => new BasicMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 10,
        maxSupplyPowerForSpeed = 100
    };

    public static readonly Func<BasicMachineSetting> PullerDefault = () => new BasicMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 200,
        maxSupplyPowerForSpeed = 10000
    };

    public static readonly Func<RangeMachineSetting> GathererDefault = () => new RangeMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 500,
        maxSupplyPowerForSpeed = 20000,
        minSupplyPowerForRange = 0,
        maxSupplyPowerForRange = 2000
    };

    public static readonly Func<RangeMachineSetting> SlaughterDefault = () => new RangeMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 500,
        maxSupplyPowerForSpeed = 20000,
        minSupplyPowerForRange = 0,
        maxSupplyPowerForRange = 2000
    };

    public static readonly Func<BasicMachineSetting> MinerDefault = () => new BasicMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 10000,
        maxSupplyPowerForSpeed = 1000000
    };

    public static readonly Func<RangeMachineSetting> CleanerDefault = () => new RangeMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 500,
        maxSupplyPowerForSpeed = 20000,
        minSupplyPowerForRange = 0,
        maxSupplyPowerForRange = 3000
    };

    public static readonly Func<RangeMachineSetting> RepairerDefault = () => new RangeMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 1000,
        maxSupplyPowerForSpeed = 20000,
        minSupplyPowerForRange = 0,
        maxSupplyPowerForRange = 10000
    };

    public static readonly Func<RangeMachineSetting> StunnerDefault = () => new RangeMachineSetting
    {
        speedFactor = 1f,
        minSupplyPowerForSpeed = 1000,
        maxSupplyPowerForSpeed = 50000,
        minSupplyPowerForRange = 0,
        maxSupplyPowerForRange = 10000
    };

    public static readonly Func<SimpleRangeMachineSetting> ShieldDefault = () => new SimpleRangeMachineSetting
    {
        minSupplyPowerForRange = 0,
        maxSupplyPowerForRange = 10000
    };

    private List<RangeSkillMachineSetting> autoMachineToolSetting = CreateAutoMachineToolDefault();
    public BasicMachineSetting beltConveyorSetting = BeltConveyorDefault();

    public RangeMachineSetting cleanerSetting = CleanerDefault();

    public RangeMachineSetting gathererSetting = GathererDefault();

    private List<RangeMachineSetting> harvesterSetting = CreateHarvesterDefault();

    public BasicMachineSetting minerSetting = MinerDefault();

    private List<RangeSkillMachineSetting> planterSetting = CreatePlanterDefault();

    public BasicMachineSetting pullerSetting = PullerDefault();

    public RangeMachineSetting repairerSetting = RepairerDefault();

    private Vector2 scrollPosition;

    public SimpleRangeMachineSetting shieldSetting = ShieldDefault();

    public RangeMachineSetting slaughterSetting = SlaughterDefault();

    public RangeMachineSetting stunnerSetting = StunnerDefault();

    public event EventHandler DataExposed;

    private static List<RangeSkillMachineSetting> CreateAutoMachineToolDefault()
    {
        return
        [
            new RangeSkillMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 500,
                minSupplyPowerForSpeed = 100,
                maxSupplyPowerForSpeed = 1000,
                skillLevel = 5,
                speedFactor = 1f
            },

            new RangeSkillMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 500,
                minSupplyPowerForSpeed = 500,
                maxSupplyPowerForSpeed = 5000,
                skillLevel = 10,
                speedFactor = 1.5f
            },

            new RangeSkillMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 1000,
                minSupplyPowerForSpeed = 1000,
                maxSupplyPowerForSpeed = 100000,
                skillLevel = 20,
                speedFactor = 2f
            }
        ];
    }

    private static List<RangeSkillMachineSetting> CreatePlanterDefault()
    {
        return
        [
            new RangeSkillMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 1000,
                minSupplyPowerForSpeed = 300,
                maxSupplyPowerForSpeed = 1000,
                skillLevel = 5,
                speedFactor = 1f
            },

            new RangeSkillMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 2000,
                minSupplyPowerForSpeed = 500,
                maxSupplyPowerForSpeed = 5000,
                skillLevel = 10,
                speedFactor = 1.5f
            },

            new RangeSkillMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 5000,
                minSupplyPowerForSpeed = 1000,
                maxSupplyPowerForSpeed = 10000,
                skillLevel = 20,
                speedFactor = 2f
            }
        ];
    }

    private static List<RangeMachineSetting> CreateHarvesterDefault()
    {
        return
        [
            new RangeMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 1000,
                minSupplyPowerForSpeed = 300,
                maxSupplyPowerForSpeed = 1000,
                speedFactor = 1f
            },

            new RangeMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 2000,
                minSupplyPowerForSpeed = 500,
                maxSupplyPowerForSpeed = 5000,
                speedFactor = 1.5f
            },

            new RangeMachineSetting
            {
                minSupplyPowerForRange = 0,
                maxSupplyPowerForRange = 5000,
                minSupplyPowerForSpeed = 1000,
                maxSupplyPowerForSpeed = 10000,
                speedFactor = 2f
            }
        ];
    }

    public void RestoreDefault()
    {
        autoMachineToolSetting = CreateAutoMachineToolDefault();
        planterSetting = CreatePlanterDefault();
        harvesterSetting = CreateHarvesterDefault();
        beltConveyorSetting = BeltConveyorDefault();
        pullerSetting = PullerDefault();
        gathererSetting = GathererDefault();
        slaughterSetting = SlaughterDefault();
        minerSetting = MinerDefault();
        cleanerSetting = CleanerDefault();
        repairerSetting = RepairerDefault();
        stunnerSetting = StunnerDefault();
        shieldSetting = ShieldDefault();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref autoMachineToolSetting, "autoMachineToolSetting");
        Scribe_Collections.Look(ref planterSetting, "planterSetting");
        Scribe_Collections.Look(ref harvesterSetting, "harvesterSetting");
        Scribe_Deep.Look(ref beltConveyorSetting, "beltConveyorSetting");
        Scribe_Deep.Look(ref pullerSetting, "pullerSetting");
        Scribe_Deep.Look(ref gathererSetting, "gathererSetting");
        Scribe_Deep.Look(ref slaughterSetting, "slaughterSetting");
        Scribe_Deep.Look(ref minerSetting, "minerSetting");
        Scribe_Deep.Look(ref cleanerSetting, "cleanerSetting");
        Scribe_Deep.Look(ref repairerSetting, "repairerSetting");
        Scribe_Deep.Look(ref stunnerSetting, "stunnerSetting");
        Scribe_Deep.Look(ref shieldSetting, "shieldSetting");
        autoMachineToolSetting ??= CreateAutoMachineToolDefault();
        planterSetting ??= CreatePlanterDefault();
        harvesterSetting ??= CreateHarvesterDefault();
        beltConveyorSetting ??= BeltConveyorDefault();
        pullerSetting ??= PullerDefault();
        gathererSetting ??= GathererDefault();
        slaughterSetting ??= SlaughterDefault();
        minerSetting ??= MinerDefault();
        cleanerSetting ??= CleanerDefault();
        repairerSetting ??= RepairerDefault();
        stunnerSetting ??= StunnerDefault();
        shieldSetting ??= ShieldDefault();
        Ops.Option(DataExposed).ForEach(delegate(EventHandler e) { e(this, EventArgs.Empty); });
    }

    public RangeSkillMachineSetting AutoMachineToolTier(int tier)
    {
        autoMachineToolSetting ??= CreateAutoMachineToolDefault();
        return autoMachineToolSetting[tier - 1];
    }

    public RangeSkillMachineSetting PlanterTier(int tier)
    {
        planterSetting ??= CreatePlanterDefault();
        return planterSetting[tier - 1];
    }

    public RangeMachineSetting HarvesterTier(int tier)
    {
        harvesterSetting ??= CreateHarvesterDefault();
        return harvesterSetting[tier - 1];
    }

    public void DoSetting(Rect inRect)
    {
        var array = new[]
        {
            new
            {
                Name = "NR_AutoMachineTool.AutoMachineTool",
                Setting = autoMachineToolSetting.Cast<IMachineSetting>()
            },
            new
            {
                Name = "NR_AutoMachineTool.Planter",
                Setting = planterSetting.Cast<IMachineSetting>()
            },
            new
            {
                Name = "NR_AutoMachineTool.Harvester",
                Setting = harvesterSetting.Cast<IMachineSetting>()
            }
        };
        var array2 = new[]
        {
            new
            {
                Name = "Building_NR_AutoMachineTool_BeltConveyor",
                Setting = (IMachineSetting)beltConveyorSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Puller",
                Setting = (IMachineSetting)pullerSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_AnimalResourceGatherer",
                Setting = (IMachineSetting)gathererSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Slaughterhouse",
                Setting = (IMachineSetting)slaughterSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Miner",
                Setting = (IMachineSetting)minerSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Cleaner",
                Setting = (IMachineSetting)cleanerSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Repairer",
                Setting = (IMachineSetting)repairerSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Stunner",
                Setting = (IMachineSetting)stunnerSetting
            },
            new
            {
                Name = "Building_NR_AutoMachineTool_Shield",
                Setting = (IMachineSetting)shieldSetting
            }
        };
        var width = inRect.width - 30f;
        var height =
            array.Select(a =>
                Text.CalcHeight(a.Name.Translate(), width) + 12f +
                a.Setting.Select(s => s.GetHeight() + 42f + 12f).Sum()).Sum() + array2.Select(a =>
                Text.CalcHeight(ThingDef.Named(a.Name).label, width) + a.Setting.GetHeight() + 12f).Sum() + 50f;
        var rect = new Rect(inRect.x, inRect.y, width, height);
        Widgets.BeginScrollView(inRect, ref scrollPosition, rect);
        var list = new Listing_Standard();
        list.Begin(rect);
        array.ForEach(a =>
        {
            var i = 0;
            DrawMachineName(a.Name.Translate(), list);
            a.Setting.ForEach(delegate(IMachineSetting s) { DrawTier(list, s, ++i); });
            list.GapLine();
        });
        array2.ForEach(a =>
        {
            DrawMachineName(ThingDef.Named(a.Name).label, list);
            DrawSetting(list, a.Setting);
            list.GapLine();
        });
        if (Widgets.ButtonText(list.GetRect(30f).RightHalf().RightHalf(),
                "NR_AutoMachineTool.SettingReset".Translate()))
        {
            RestoreDefault();
        }

        list.End();
        Widgets.EndScrollView();
    }

    private void DrawTier(Listing_Standard list, IMachineSetting s, int tier)
    {
        var rect = list.GetRect(s.GetHeight() + 42f);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect.RightPartPixels(rect.width - 50f));
        Text.Font = GameFont.Medium;
        Widgets.Label(listing_Standard.GetRect(30f), "Tier " + tier);
        listing_Standard.GapLine();
        Text.Font = GameFont.Small;
        s.DrawModSetting(listing_Standard);
        listing_Standard.End();
    }

    private void DrawSetting(Listing_Standard list, IMachineSetting s)
    {
        var rect = list.GetRect(s.GetHeight());
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect.RightPartPixels(rect.width - 50f));
        s.DrawModSetting(listing_Standard);
        listing_Standard.End();
    }

    private void DrawMachineName(string name, Listing_Standard list)
    {
        var font = Text.Font;
        Text.Font = GameFont.Medium;
        list.Label(name);
        Text.Font = font;
    }
}