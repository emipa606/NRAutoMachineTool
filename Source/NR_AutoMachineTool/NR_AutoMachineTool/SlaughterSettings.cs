using Verse;

namespace NR_AutoMachineTool;

public class SlaughterSettings : IExposable
{
    public ThingDef def;

    public bool doSlaughter;

    public bool hasBonds;

    public int keepFemaleAdultCount;

    public int keepFemaleYoungCount;

    public int keepMaleAdultCount;

    public int keepMaleYoungCount;

    public bool pregnancy;

    public bool trained;

    public SlaughterSettings()
    {
        doSlaughter = false;
        hasBonds = false;
        pregnancy = false;
        trained = false;
        keepMaleAdultCount = 10;
        keepMaleYoungCount = 10;
        keepFemaleAdultCount = 10;
        keepFemaleYoungCount = 10;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref doSlaughter, "doSlaughter");
        Scribe_Values.Look(ref hasBonds, "hasBonds");
        Scribe_Values.Look(ref pregnancy, "pregnancy");
        Scribe_Values.Look(ref trained, "trained");
        Scribe_Values.Look(ref keepMaleAdultCount, "keepMaleAdultCount", 10);
        Scribe_Values.Look(ref keepMaleYoungCount, "keepMaleYoungCount", 10);
        Scribe_Values.Look(ref keepFemaleAdultCount, "keepFemaleAdultCount", 10);
        Scribe_Values.Look(ref keepFemaleYoungCount, "keepFemaleYoungCount", 10);
        Scribe_Defs.Look(ref def, "def");
    }

    public int KeepCount(Gender gender, bool adult)
    {
        if (gender == Gender.Male)
        {
            return adult ? keepMaleAdultCount : keepMaleYoungCount;
        }

        return adult ? keepFemaleAdultCount : keepFemaleYoungCount;
    }
}