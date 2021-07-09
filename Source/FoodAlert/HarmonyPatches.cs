using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    [StaticConstructorOnStartup]
    internal class HarmonyPatches
    {
        private static float CachedNutrition;
        private static float CachedNeed;
        private static int CachedHumans;
        public static readonly bool IsSosLoaded;

        static HarmonyPatches()
        {
            var harmony = new Harmony("mehni.rimworld.FoodAlert.main");
            IsSosLoaded = ModLister.GetActiveModWithIdentifier("kentington.saveourship2") != null;
            harmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FoodCounter_NearDatePostfix)));
        }

        private static float GetEdibleStuff(Map map)
        {
            var num = 0f;
            var selectedPreferability = LoadedModManager.GetMod<FoodAlertMod>().GetSettings<FoodAlertSettings>()
                .foodPreferability;
            var selectedPreferabilityEnum =
                (FoodPreferability) Enum.Parse(typeof(FoodPreferability), selectedPreferability);
            foreach (var keyValuePair in map.resourceCounter.AllCountedAmounts)
            {
                if (keyValuePair.Value <= 0)
                {
                    continue;
                }

                if (!keyValuePair.Key.IsNutritionGivingIngestible)
                {
                    continue;
                }

                if (!keyValuePair.Key.ingestible.HumanEdible)
                {
                    continue;
                }

                if (selectedPreferabilityEnum > keyValuePair.Key.ingestible.preferability)
                {
                    continue;
                }

                num += keyValuePair.Key.GetStatValueAbstract(StatDefOf.Nutrition) * keyValuePair.Value;
            }

            return num;
        }

        private static void FoodCounter_NearDatePostfix(ref float curBaseY)
        {
            var map = Find.CurrentMap;

            if (map == null || !map.IsPlayerHome && !IsSosLoaded)
            {
                return;
            }

            //if (Find.TickManager.TicksGame < 15000)
            //    return;
            var updateFoodInfo = Find.TickManager.TicksGame % 400 == 0;

            if (updateFoodInfo)
            {
                CachedNutrition = GetEdibleStuff(map);
                CachedNeed = 0f;
                var pawns = map.mapPawns.FreeColonistsAndPrisoners;
                foreach (var pawn in pawns)
                {
                    if (pawn?.needs?.food == null)
                    {
                        continue;
                    }

                    var pawnNeed = pawn.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f;
                    //Log.Message($"{pawn.NameShortColored} needs {pawnNeed} food per day.");
                    CachedNeed += pawnNeed;
                }

                CachedHumans = map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
            }

            //if (totalHumanEdibleNutrition < 4f * map.mapPawns.FreeColonistsSpawnedCount)
            //    return;


            string addendumForFlavour = "\n    " + "SettingDescription".Translate() + ": " +
                                        LoadedModManager.GetMod<FoodAlertMod>().GetSettings<FoodAlertSettings>()
                                            .foodPreferability;
            if (CachedNeed == 0f)
            {
                addendumForFlavour = "\n\nTotal food-need is 0, that shouldnt happen.";
                CachedNeed = 0.0001f;
            }

            var totalDaysOfFood = Mathf.FloorToInt(CachedNutrition / CachedNeed);
            string daysWorthOfHumanFood = $"{totalDaysOfFood}" + "FoodAlert_DaysOfFood".Translate();

            switch (totalDaysOfFood)
            {
                case { } n when n >= 100:
                    addendumForFlavour += "FoodAlert_Ridiculous".Translate();
                    break;

                case { } n when n >= 60:
                    addendumForFlavour += "FoodAlert_Solid".Translate();
                    break;

                case { } n when n >= 30:
                    addendumForFlavour += "FoodAlert_Bunch".Translate();
                    break;

                case { } n when n >= 4:
                    addendumForFlavour += "FoodAlert_Decent".Translate();
                    break;
                case { } n when n >= 1:
                    addendumForFlavour += "FoodAlert_Poor".Translate();
                    break;
                default:
                    return;
            }

            var rightMargin = 7f;
            var zlRect = new Rect(UI.screenWidth - Alert.Width, curBaseY - 24f, Alert.Width, 24f);
            Text.Font = GameFont.Small;

            if (Mouse.IsOver(zlRect))
            {
                Widgets.DrawHighlight(zlRect);
            }

            var foodText = "SomeFoodDescNew";
            GUI.BeginGroup(zlRect);
            var startColor = GUI.color;
            if (totalDaysOfFood <= 3)
            {
                GUI.color = Color.yellow;
                if (totalDaysOfFood <= 1)
                {
                    GUI.color = Color.red;
                }

                foodText = "LowFoodDesc";
            }

            Text.Anchor = TextAnchor.UpperRight;
            var rect = zlRect.AtZero();
            rect.xMax -= rightMargin;

            Widgets.Label(rect, daysWorthOfHumanFood);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = startColor;
            GUI.EndGroup();

            TooltipHandler.TipRegion(zlRect, new TipSignal(
                () => string.Format(foodText.Translate(),
                    CachedNutrition.ToString("F0"),
                    CachedHumans.ToStringCached(),
                    CachedNeed.ToString("F0"),
                    totalDaysOfFood.ToStringCached() + addendumForFlavour),
                76515));

            curBaseY -= zlRect.height;
        }
    }
}