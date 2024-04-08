using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

[StaticConstructorOnStartup]
public static class RS
{
    public static readonly Texture2D PregnantIcon;

    public static readonly Texture2D BondIcon;

    public static readonly Texture2D MaleIcon;

    public static readonly Texture2D FemaleIcon;

    public static readonly Texture2D SlaughterIcon;

    public static readonly Texture2D TrainedIcon;

    public static readonly Texture2D YoungIcon;

    public static readonly Texture2D AdultIcon;

    public static readonly Texture2D OutputDirectionIcon;

    public static readonly Texture2D ForbidIcon;

    public static readonly Texture2D PlayIcon;

    public static readonly Texture2D DeleteX;

    static RS()
    {
        PregnantIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant");
        BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond");
        MaleIcon = Gender.Male.GetIcon();
        FemaleIcon = Gender.Female.GetIcon();
        SlaughterIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Slaughter");
        TrainedIcon = ContentFinder<Texture2D>.Get("UI/Icons/Trainables/Obedience");
        YoungIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Young");
        AdultIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult");
        OutputDirectionIcon = ContentFinder<Texture2D>.Get("NR_AutoMachineTool/UI/OutputDirection");
        ForbidIcon = ContentFinder<Texture2D>.Get("NR_AutoMachineTool/UI/Forbid");
        PlayIcon = ContentFinder<Texture2D>.Get("NR_AutoMachineTool/UI/Play");
        DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");
    }
}