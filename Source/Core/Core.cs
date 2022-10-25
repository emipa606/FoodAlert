using System;
using System.Collections.Generic;
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
internal class Core
{
    /// <summary>
    /// 当前食物数量
    /// </summary>
    private static float _cachedNutrition;

    /// <summary>
    /// 殖民者和囚犯每天消耗的食物
    /// </summary>
    private static float _cachedNeed;

    /// <summary>
    /// 殖民者获取食物
    /// </summary>
    private static float _cachedHumans;

    /// <summary>
    /// 食物可供食用的天数
    /// </summary>
    private static float _cachedDaysWorthOfFood;

    /// <summary>
    /// 优化更新频率的下一次更新时间
    /// </summary>
    private static int _nextUpdateTick;

    /// <summary>
    /// 是否允许更新数据
    /// </summary>
    private static bool _vanillaActive = true;

    /// <summary>
    /// 判断Sos2的加载状态（未实装）
    /// </summary>
    public static readonly bool isSosLoaded;

    /// <summary>
    /// 启动Harmony
    /// </summary>
    static Core()
    {
        Harmony harmony = new Harmony("mehni.rimworld.FoodAlert.main");
        harmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate)), null,
            new HarmonyMethod(typeof(Core), nameof(UpdateData)));
    }

    /// <summary>
    /// 根据食物等级在可用存储区内统计食物数量
    /// </summary>
    /// <param name="map">当前地图</param>
    /// <returns>食物数量</returns>
    private static float GetEdibleStuff(Map map)
    {
        float num = 0f;
        String selectedPreferability = FoodAlertMod.Settings.FoodPreferability;
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
        if (!FoodAlertMod.Settings.Dynamicupdate)
        {
            // 游戏时间到达指定更新时间
            return Find.TickManager.TicksGame % FoodAlertMod.Settings.Updatefrequency == 0;
        }

        // 优化更新频率等于0 或 游戏时间大于或等于优化更新频率指定的下一次更新时间
        return _nextUpdateTick == 0 || Find.TickManager.TicksGame >= _nextUpdateTick;
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="curBaseY"></param>
    private static void UpdateData(ref float curBaseY)
    {
        // 不允许更新数据
        if (!ShouldUpdate())
        {
            return;
        }

        // 正在更新中
        if (!_vanillaActive)
        {
            Tools.Debug.Log("UpdateData Return");
            return;
        }

        Tools.Debug.Log("UpdateData _vanillaActive:" + _vanillaActive);

        _vanillaActive = false;
        // 获取当前的地图
        var map = Find.CurrentMap;
        // 获取当前食物数量
        _cachedNutrition = GetEdibleStuff(map);
        _cachedNeed = 0f;
        // 获取当前地图的殖民者和囚犯
        List<Pawn> pawns = map.mapPawns.FreeColonistsAndPrisoners;
        // 统计殖民者和囚犯每天消耗的食物
        foreach (var pawn in pawns)
        {
            if (pawn?.needs?.food == null)
            {
                continue;
            }

            // 按照殖民者都能吃饱的情况下计算每天需要消耗的食物
            var pawnNeed = pawn.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f;
            Tools.Debug.Log(string.Format("{0}每天消耗食物{1}，目前饥饿程度{2}", pawn.Name, pawnNeed,
                pawn.needs.food.CurCategory));
            _cachedNeed += pawnNeed;
        }

        // 获取殖民者和囚犯每天获取的食物
        _cachedHumans = map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
        // 为没有食物消耗的单位添加默认消耗
        if (_cachedNeed == 0f)
        {
            _cachedNeed = 0.0001f;
        }

        // 计算食物可供食用的天数
        _cachedDaysWorthOfFood = _cachedNutrition / _cachedNeed;


        if (FoodAlertMod.Settings.Dynamicupdate)
        {
            // 根据食物优化更新频率
            _nextUpdateTick = Find.TickManager.TicksGame +
                              (int)Math.Round(Math.Min(_cachedDaysWorthOfFood * 400, 10000));
        }

        UpdateTab(ref curBaseY);
        Tools.Debug.Log("UpdateData _vanillaActive:" + _vanillaActive);
    }

    /// <summary>
    /// 更新Tab
    /// </summary>
    /// <param name="curBaseY"></param>
    private static void UpdateTab(ref float curBaseY)
    {
        Tools.Debug.Log("UpdateTab curBaseY:" + curBaseY);
        String selectedPreferability = FoodAlertMod.Settings.FoodPreferability;
        FoodPreferability selectedPreferabilityEnum =
            (FoodPreferability)Enum.Parse(typeof(FoodPreferability), selectedPreferability);

        string addendumForFlavour = "\n    " + "SettingDescription".Translate() + ": " +
                                    selectedPreferability;
        string daysWorthOfHumanFood = $"{_cachedDaysWorthOfFood}" + "FoodAlert_DaysOfFood".Translate();

        switch (_cachedDaysWorthOfFood)
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
        if (_cachedDaysWorthOfFood <= 3)
        {
            GUI.color = Color.yellow;
            if (_cachedDaysWorthOfFood <= 1)
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
                _cachedNutrition.ToString("F1"),
                _cachedHumans.ToString("F1"),
                _cachedNeed.ToString("F1"),
                _cachedDaysWorthOfFood.ToString("F1") + addendumForFlavour),
            76515));

        curBaseY -= zlRect.height;
        _vanillaActive = true;
        Tools.Debug.Log("UpdateTab End");
    }
}