using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

[StaticConstructorOnStartup]
internal static class TexButton
{
    public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste");
}