using System;
using FoodAlert.Config;
using FoodAlert.Setting;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert.Core;

/// <summary>
/// 启动类
/// </summary>
[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    /// <summary>
    /// 当前食物数量
    /// </summary>
    private static float _cachedNutrition;

    private static float CachedNeed;
    private static int CachedHumans;
    private static int CachedDaysWorthOfFood;

    /// <summary>
    /// 优化更新频率的下一次更新时间
    /// </summary>
    private static int _nextUpdateTick = 0;

    /// <summary>
    /// 是否允许更新数据
    /// </summary>
    private static bool _vanillaActive;

    /// <summary>
    /// 判断Sos2的加载状态（未实装）
    /// </summary>
    public static readonly bool isSosLoaded;

    /// <summary>
    /// 启动Harmony
    /// </summary>
    static HarmonyPatches()
    {
        Harmony harmony = new Harmony("mehni.rimworld.FoodAlert.main");
        harmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate)), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(FoodCounter_NearDatePostfix)));
    }

    /// <summary>
    /// 根据食物等级在可用存储区内统计食物数量
    /// </summary>
    /// <param name="map">当前地图</param>
    /// <returns></returns>
    private static float GetEdibleStuff(Map map)
    {
        float num = 0f;
        String selectedPreferability = FoodAlertMod.Settings.foodPreferability;
        // 获取FoodPreferability枚举对象
        FoodPreferability selectedPreferabilityEnum =
            (FoodPreferability)Enum.Parse(typeof(FoodPreferability), selectedPreferability);
        // 在可用存储区内统计食物数量
        foreach (var keyValuePair in map.resourceCounter.AllCountedAmounts)
        {
            // 物品数量小于或等于0
            if (keyValuePair.Value <= 0)
            {
                continue;
            }

            // 物品没有营养
            if (!keyValuePair.Key.IsNutritionGivingIngestible)
            {
                continue;
            }

            // 人类不能食用
            if (!keyValuePair.Key.ingestible.HumanEdible)
            {
                continue;
            }

            // 不符合食物等级
            if (selectedPreferabilityEnum > keyValuePair.Key.ingestible.preferability)
            {
                continue;
            }

            // 通过物品的营养价值乘以物品数量算出总营养价值
            num += keyValuePair.Key.GetStatValueAbstract(StatDefOf.Nutrition) * keyValuePair.Value;
        }

        return num;
    }

    /// <summary>
    /// 是否允许更新数据
    /// </summary>
    /// <returns></returns>
    private static bool ShouldUpdate()
    {
        // 不使用优化更新频率
        if (!FoodAlertMod.Settings.dynamicupdate)
        {
            // 游戏时间到达指定更新时间
            return Find.TickManager.TicksGame % FoodAlertMod.Settings.updatefrequency == 0;
        }

        // 优化更新频率等于0 或 游戏时间大于或等于优化更新频率指定的下一次更新时间
        return _nextUpdateTick == 0 || Find.TickManager.TicksGame >= _nextUpdateTick;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="curBaseY"></param>
    private static void FoodCounter_NearDatePostfix(ref float curBaseY)
    {
        // 允许更新数据
        if (ShouldUpdate())
        {
            _vanillaActive = false;
            // 获取当前的地图
            var map = Find.CurrentMap;
            if (map == null || // 现在的地图不是null
                !map.IsPlayerHome && !isSosLoaded || // 地图不是居住区且sos2没有加载
                map.IsPlayerHome && map.mapPawns.AnyColonistSpawned &&
                map.resourceCounter.TotalHumanEdibleNutrition <
                4f * map.mapPawns.FreeColonistsSpawnedCount // 在玩家的居住区内且是殖民者地图且是人类可用的食物小于殖民者4天的消耗
               ) // Vanilla low food alert condition
            {
                // 允许优化更新频率
                if (FoodAlertMod.Settings.dynamicupdate)
                {
                    // 以最高预设tick进行更新
                    _nextUpdateTick = Find.TickManager.TicksGame + 400;
                }

                _vanillaActive = true;
                return;
            }

            // 获取当前食物数量
            _cachedNutrition = GetEdibleStuff(map);
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

            var daysWorthActual = _cachedNutrition / CachedNeed;
            if (FoodAlertMod.Settings.dynamicupdate)
            {
                _nextUpdateTick = Find.TickManager.TicksGame +
                                  (int)Math.Round(Math.Min(Math.Max(daysWorthActual * 400, 100), 10000));
                //Log.Message(
                //    $"Setting next update to {NextUpdateTick} ({NextUpdateTick - Find.TickManager.TicksGame} ticks)");
            }

            CachedDaysWorthOfFood = Mathf.FloorToInt(daysWorthActual);
        }

        if (_vanillaActive)
        {
            return;
        }

        var selectedPreferability = LoadedModManager.GetMod<FoodAlertMod>().GetSettings<FoodAlertSettings>()
            .foodPreferability;
        var selectedPreferabilityEnum =
            (FoodPreferability)Enum.Parse(typeof(FoodPreferability), selectedPreferability);

        string addendumForFlavour = "\n    " + "SettingDescription".Translate() + ": " +
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

                /* there's food but since there's no vanilla alert active, probably we are counting food with an higher preferability
                 * in any case, let's dispaly at least a poor food alert 
                 */
                addendumForFlavour += "FoodAlert_Poor".Translate();

                if (selectedPreferabilityEnum > FoodPreferability.DesperateOnly)
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
                _cachedNutrition.ToString("F0"),
                CachedHumans.ToStringCached(),
                CachedNeed.ToString("F0"),
                CachedDaysWorthOfFood.ToStringCached() + addendumForFlavour),
            76515));

        curBaseY -= zlRect.height;
    }
}