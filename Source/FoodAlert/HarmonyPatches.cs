using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    private static float CachedNutrition;
    private static float CachedNeed;
    private static int CachedHumans;
    private static int CachedDaysWorthOfFood;
    private static int NextUpdateTick;
    private static bool VanillaActive;
    public static readonly bool IsSosLoaded;

    static HarmonyPatches()
    {
        var harmony = new Harmony("mehni.rimworld.FoodAlert.main");
        IsSosLoaded = ModLister.GetActiveModWithIdentifier("kentington.saveourship2") != null;
        harmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate)), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(FoodCounter_NearDatePostfix)));
    }

    private static bool ShouldUpdate()
    {
        if (!FoodAlertMod.settings.dynamicupdate)
        {
            return Find.TickManager.TicksGame % FoodAlertMod.settings.updatefrequency == 0;
        }

        return NextUpdateTick == 0 || Find.TickManager.TicksGame >= NextUpdateTick;
    }

    private static void FoodCounter_NearDatePostfix(ref float curBaseY)
    {
        if (ShouldUpdate())
        {
            VanillaActive = false;
            var map = Find.CurrentMap;
            if (map == null ||
                !map.IsPlayerHome && !IsSosLoaded ||
                map.IsPlayerHome && map.mapPawns.AnyColonistSpawned &&
                map.resourceCounter.TotalHumanEdibleNutrition <
                4f * map.mapPawns.FreeColonistsSpawnedCount) // Vanilla low food alert condition
            {
                if (FoodAlertMod.settings.dynamicupdate)
                {
                    NextUpdateTick = Find.TickManager.TicksGame + 400;
                }

                VanillaActive = true;
                return;
            }

            CachedNutrition = NutritionCounter.GetEdibleStuff(map);
            CachedNeed = 0f;
            var pawns = map.mapPawns.FreeColonistsAndPrisoners;
            foreach (var pawn in pawns)
            {
                if (pawn?.needs?.food == null)
                {
                    continue;
                }

                var pawnNeed = pawn.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f;
                CachedNeed += pawnNeed;
            }

            CachedHumans = map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
            if (CachedNeed == 0f)
            {
                CachedNeed = 0.0001f;
            }

            var daysWorthActual = CachedNutrition / CachedNeed;
            if (FoodAlertMod.settings.dynamicupdate)
            {
                NextUpdateTick = Find.TickManager.TicksGame +
                                 (int)Math.Round(Math.Min(Math.Max(daysWorthActual * 400, 100), 10000));
            }

            CachedDaysWorthOfFood = Mathf.FloorToInt(daysWorthActual);
        }

        if (VanillaActive)
        {
            return;
        }

        var selectedPreferability = LoadedModManager.GetMod<FoodAlertMod>().GetSettings<FoodAlertSettings>()
            .foodPreferability;

        var addendumForFlavour = "\n    " + "SettingDescription".Translate() + ": " +
                                 selectedPreferability;
        string daysWorthOfHumanFood = $"{CachedDaysWorthOfFood}" + "FoodAlert_DaysOfFood".Translate();

        switch (CachedDaysWorthOfFood)
        {
            case >= 100:
                addendumForFlavour += "FoodAlert_Ridiculous".Translate();
                break;

            case >= 60:
                addendumForFlavour += "FoodAlert_Solid".Translate();
                break;

            case >= 30:
                addendumForFlavour += "FoodAlert_Bunch".Translate();
                break;

            case >= 4:
                addendumForFlavour += "FoodAlert_Decent".Translate();
                break;

            case >= 0:

                /* there's food but since there's no vanilla alert active, probably we are counting food with a higher preferability
                 * in any case, let's dispaly at least a poor food alert
                 */
                addendumForFlavour += "FoodAlert_Poor".Translate();

                if (selectedPreferability > FoodPreferability.DesperateOnly)
                {
                    // and a warning that more food may be available
                    addendumForFlavour += "LowFoodAddendum".Translate();
                }

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
        if (CachedDaysWorthOfFood <= 3)
        {
            GUI.color = Color.yellow;
            if (CachedDaysWorthOfFood <= 1)
            {
                GUI.color = Color.red;
            }

            foodText = "LowFoodDescNew";
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
                CachedDaysWorthOfFood.ToStringCached() + addendumForFlavour),
            76515));

        curBaseY -= zlRect.height;
    }
}