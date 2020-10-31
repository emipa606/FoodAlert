using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("mehni.rimworld.FoodAlert.main");

            harmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FoodCounter_NearDatePostfix)), null);
        }

        private static float GetEdibleStuff(Map map)
        {
            float num = 0f;
            var selectedPreferability = LoadedModManager.GetMod<FoodAlertMod>().GetSettings<FoodAlertSettings>().foodPreferability;
            FoodPreferability selectedPreferabilityEnum = (FoodPreferability)Enum.Parse(typeof(FoodPreferability), selectedPreferability);
            foreach (KeyValuePair<ThingDef, int> keyValuePair in map.resourceCounter.AllCountedAmounts)
            {
                if (keyValuePair.Key.IsNutritionGivingIngestible && keyValuePair.Key.ingestible.HumanEdible)
                {
                    if (selectedPreferabilityEnum > keyValuePair.Key.ingestible.preferability)
                        continue;
                    num += keyValuePair.Key.GetStatValueAbstract(StatDefOf.Nutrition, null) * (float)keyValuePair.Value;
                }
            }
            return num;
        }

        private static void FoodCounter_NearDatePostfix(ref float curBaseY)
        {
            Map map = Find.CurrentMap;

            if (map == null || !map.IsPlayerHome)
                return;

            if (Find.TickManager.TicksGame < 15000)
                return;

            float totalHumanEdibleNutrition = GetEdibleStuff(map);

            if (totalHumanEdibleNutrition < 4f * map.mapPawns.FreeColonistsSpawnedCount)
                return;

            int humansGettingFood = map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
            float totalFoodNeedPerDay = 0f;
            var pawns = map.mapPawns.FreeColonistsAndPrisoners;
            foreach (Pawn pawn in pawns)
            {
                var pawnNeed = pawn.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, false) * 60000f;
                //Log.Message($"{pawn.NameShortColored} needs {pawnNeed} food per day.");
                totalFoodNeedPerDay += pawnNeed;
            }
            string addendumForFlavour = "\n    " + "SettingDescription".Translate() + ": " + LoadedModManager.GetMod<FoodAlertMod>().GetSettings<FoodAlertSettings>().foodPreferability;
            if(totalFoodNeedPerDay == 0f)
            {
                addendumForFlavour = "\n\nTotal food-need is 0, that shouldnt happen.";
                totalFoodNeedPerDay = 0.0001f;
            }
            int totalDaysOfFood = Mathf.FloorToInt(totalHumanEdibleNutrition / totalFoodNeedPerDay);
            string daysWorthOfHumanFood = $"{totalDaysOfFood}" + "FoodAlert_DaysOfFood".Translate();

            switch (totalDaysOfFood)
            {
                case int n when n >= 100:
                    addendumForFlavour += "FoodAlert_Ridiculous".Translate();
                    break;

                case int n when n >= 60:
                    addendumForFlavour += "FoodAlert_Solid".Translate();
                    break;

                case int n when n >= 30:
                    addendumForFlavour += "FoodAlert_Bunch".Translate();
                    break;

                case int n when n >= 10:
                    addendumForFlavour += "FoodAlert_Decent".Translate();
                    break;

                default:
                    addendumForFlavour += "FoodAlert_Decent".Translate();
                    break;
            }

            float rightMargin = 7f;
            Rect zlRect = new Rect(UI.screenWidth - Alert.Width, curBaseY - 24f, Alert.Width, 24f);
            Text.Font = GameFont.Small;

            if (Mouse.IsOver(zlRect))
                Widgets.DrawHighlight(zlRect);

            GUI.BeginGroup(zlRect);
            Text.Anchor = TextAnchor.UpperRight;
            Rect rect = zlRect.AtZero();
            rect.xMax -= rightMargin;

            Widgets.Label(rect, daysWorthOfHumanFood);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();

            TooltipHandler.TipRegion(zlRect, new TipSignal(
                                                   () => string.Format("SomeFoodDescNew".Translate(),
                                                                       totalHumanEdibleNutrition.ToString("F0"),
                                                                       humansGettingFood.ToStringCached(),
                                                                       totalFoodNeedPerDay.ToString("F0"),
                                                                       totalDaysOfFood.ToStringCached() + addendumForFlavour),
                                                   76515));

            curBaseY -= zlRect.height;
        }
    }
}
