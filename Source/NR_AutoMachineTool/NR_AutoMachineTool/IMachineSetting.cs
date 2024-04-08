using Verse;

namespace NR_AutoMachineTool;

public interface IMachineSetting : IExposable
{
    void DrawModSetting(Listing list);

    float GetHeight();
}