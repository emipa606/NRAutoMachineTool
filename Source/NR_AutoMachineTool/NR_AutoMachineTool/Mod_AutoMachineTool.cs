using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Mod_AutoMachineTool : Mod
{
    public readonly Option<HopmMod> Hopm;

    public Mod_AutoMachineTool(ModContentPack content)
        : base(content)
    {
        Setting = GetSettings<ModSetting_AutoMachineTool>();
        var option = (from a in LoadedModManager.RunningMods
                .Where(m => m.Name.StartsWith("Harvest Organs Post Mortem -"))
                .SelectMany(m => m.assemblies.loadedAssemblies)
            where a.GetType("Autopsy.Mod") != null
            select a).FirstOption();
        var hugsAsm = (from a in LoadedModManager.RunningMods.Where(m => m.Name == "HugsLib")
                .SelectMany(m => m.assemblies.loadedAssemblies)
            where a.GetType("HugsLib.Settings.SettingHandle") != null
            select a).FirstOption();
        Hopm = option.SelectMany(ho => hugsAsm.SelectMany(hu => HopmMod.CreateHopmMod(ho, hu)));
    }

    public ModSetting_AutoMachineTool Setting { get; }

    public override string SettingsCategory()
    {
        return "NR_AutoMachineTool.SettingName".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Setting.DoSetting(inRect);
    }

    public class HopmMod
    {
        private static readonly Dictionary<string, FieldInfo> fields = new();

        private static readonly Dictionary<string, PropertyInfo> fieldValueProps = new();

        private Type autopsyRecipeDefsType;

        private bool initializedAtRuntime;
        private Type modType;

        private RecipeDef Recipe_AutopsyAdvanced;

        private RecipeDef Recipe_AutopsyAnimal;

        private RecipeDef Recipe_AutopsyBasic;

        private RecipeDef Recipe_AutopsyGlitterworld;

        private Type recipeInfoType;

        private Type settingHandlerType;

        private MethodInfo traverseBody;

        private HopmMod()
        {
        }

        public static Option<HopmMod> CreateHopmMod(Assembly hopmAsm, Assembly hugsAsm)
        {
            try
            {
                var hopmMod = new HopmMod
                {
                    modType = hopmAsm.GetType("Autopsy.Mod"),
                    settingHandlerType = hugsAsm.GetType("HugsLib.Settings.SettingHandle`1"),
                    recipeInfoType = hopmAsm.GetType("Autopsy.RecipeInfo")
                };
                hopmMod.traverseBody = hopmAsm.GetType("Autopsy.NewMedicalRecipesUtility").GetMethod(
                    "TraverseBody", [
                        hopmMod.recipeInfoType,
                        typeof(Corpse),
                        typeof(float)
                    ]);
                hopmMod.autopsyRecipeDefsType = hopmAsm.GetType("Autopsy.Util.AutopsyRecipeDefs");
                return Ops.Just(hopmMod);
            }
            catch (Exception ex)
            {
                Log.Error("HOPMのメタデータ取得エラー. " + ex);
                return Ops.Nothing<HopmMod>();
            }
        }

        private void InitializeAtRuntime()
        {
            if (!initializedAtRuntime)
            {
                Recipe_AutopsyBasic = (RecipeDef)autopsyRecipeDefsType
                    .GetField("AutopsyBasic", BindingFlags.Static | BindingFlags.Public)
                    ?.GetValue(null);
                Recipe_AutopsyAdvanced = (RecipeDef)autopsyRecipeDefsType
                    .GetField("AutopsyAdvanced", BindingFlags.Static | BindingFlags.Public)
                    ?.GetValue(null);
                Recipe_AutopsyGlitterworld = (RecipeDef)autopsyRecipeDefsType
                    .GetField("AutopsyGlitterworld", BindingFlags.Static | BindingFlags.Public)
                    ?.GetValue(null);
                Recipe_AutopsyAnimal = (RecipeDef)autopsyRecipeDefsType
                    .GetField("AutopsyAnimal", BindingFlags.Static | BindingFlags.Public)
                    ?.GetValue(null);
                if (Recipe_AutopsyBasic == null)
                {
                    throw new FieldAccessException("Autopsy.Util.AutopsyRecipeDefs.AutopsyBasic へのアクセスに失敗.");
                }

                if (Recipe_AutopsyAdvanced == null)
                {
                    throw new FieldAccessException("Autopsy.Util.AutopsyRecipeDefs.AutopsyAdvanced へのアクセスに失敗.");
                }

                if (Recipe_AutopsyGlitterworld == null)
                {
                    throw new FieldAccessException("Autopsy.Util.AutopsyRecipeDefs.AutopsyGlitterworld へのアクセスに失敗.");
                }

                if (Recipe_AutopsyAnimal == null)
                {
                    throw new FieldAccessException("Autopsy.Util.AutopsyRecipeDefs.AutopsyAnimal へのアクセスに失敗.");
                }
            }

            initializedAtRuntime = true;
        }

        private object GetValue(string fieldName)
        {
            if (!fields.ContainsKey(fieldName))
            {
                fields[fieldName] = modType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
                fieldValueProps[fieldName] = settingHandlerType
                    .MakeGenericType(fields[fieldName].FieldType.GetGenericArguments()).GetProperty("Value");
            }

            var value = fields[fieldName].GetValue(null);
            return fieldValueProps[fieldName].GetValue(value, []);
        }

        public void Postfix_MakeRecipeProducts(ref IEnumerable<Thing> __result, RecipeDef recipeDef, float skillChance,
            List<Thing> ingredients)
        {
            string text = null;
            InitializeAtRuntime();
            try
            {
                if (recipeDef.Equals(Recipe_AutopsyBasic))
                {
                    text = "Basic";
                }
                else if (recipeDef.Equals(Recipe_AutopsyAdvanced))
                {
                    text = "Advanced";
                }
                else if (recipeDef.Equals(Recipe_AutopsyGlitterworld))
                {
                    text = "Glitter";
                }
                else if (recipeDef.Equals(Recipe_AutopsyAnimal))
                {
                    text = "Animal";
                }

                if (text == null)
                {
                    return;
                }

                var obj = text == "Animal" ? 0f : GetValue(text + "AutopsyOrganMaxChance");
                var num = text != "Animal" ? (int)GetValue(text + "AutopsyCorpseAge") * 2500 : 0;
                var obj2 = text == "Animal" ? 0f : GetValue(text + "AutopsyFrozenDecay");
                var obj3 = Activator.CreateInstance(recipeInfoType, obj, num, GetValue(text + "AutopsyBionicMaxChance"),
                    GetValue(text + "AutopsyMaxNumberOfOrgans"), obj2);
                skillChance *= (float)GetValue(text + "AutopsyMedicalSkillScaling");
                var list = __result as List<Thing> ?? __result.ToList();
                foreach (var item in ingredients.OfType<Corpse>())
                {
                    list.AddRange(
                        (IEnumerable<Thing>)traverseBody.Invoke(null, [obj3, item, skillChance]));
                }

                __result = list;
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("HOPMの実行時エラー. " + ex, 1660882676);
            }
        }
    }
}