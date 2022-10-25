using System;
using System.Collections.Generic;
using FoodAlert.Data;
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
            new HarmonyMethod(typeof(Core), nameof(ShouldUpdate)));
    }

    /// <summary>
    /// 根据食物等级在可用存储区内统计食物数量
    /// </summary>
    /// <param name="map">当前地图</param>
    /// <returns>食物数量</returns>
    private static float GetEdibleStuff(Map map)
    {
        float num = 0f;
        String selectedPreferability = ModData.Config.FoodPreferability;
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
    private static void ShouldUpdate(ref float curBaseY)
    {
        // 不使用优化更新频率
        if (!ModData.Config.Dynamicupdate)
        {
            // 游戏时间到达指定更新时间
            if (Find.TickManager.TicksGame % ModData.Config.Updatefrequency == 0)
            {
                UpdateData(ref curBaseY);
            }
        }

        // 优化更新频率等于0 或 游戏时间大于或等于优化更新频率指定的下一次更新时间
        if (_nextUpdateTick == 0 || Find.TickManager.TicksGame >= _nextUpdateTick)
        {
            UpdateData(ref curBaseY);
        }

        // TODO: 我不知道为什么 不在这里加载UI 在其他地方就会不显示UI...
        UpdateTab(ref curBaseY);
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="curBaseY"></param>
    private static void UpdateData(ref float curBaseY)
    {
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


        if (ModData.Config.Dynamicupdate)
        {
            // 根据食物优化更新频率
            _nextUpdateTick = Find.TickManager.TicksGame +
                              (int)Math.Round(Math.Min(_cachedDaysWorthOfFood * 400, 10000));
        }

        Tools.Debug.Log(
            $"当前食物{_cachedNutrition.ToString("F1")} 食物获取{_cachedHumans.ToString("F1")} 每日消耗{_cachedNeed.ToString("F1")} 可用天数{_cachedDaysWorthOfFood.ToString("F1")}");
    }

    /// <summary>
    /// 更新Tab
    /// </summary>
    /// <param name="curBaseY"></param>
    private static void UpdateTab(ref float curBaseY)
    {
        // 食物等级
        String selectedPreferability = ModData.Config.FoodPreferability;
        // 食物等级枚举
        FoodPreferability selectedPreferabilityEnum =
            (FoodPreferability)Enum.Parse(typeof(FoodPreferability), selectedPreferability);
        // 食物偏好相关
        string addendumForFlavour = "\n" + "SettingDescription".Translate() + ": " +
                                    selectedPreferability;
        // 每日食物消耗
        string daysWorthOfHumanFood = $"{_cachedDaysWorthOfFood.ToString("F1")}" + "FoodAlert_DaysOfFood".Translate();
        // 根据食物可用天数判断
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

        float rightMargin = 7f;
        Rect zlRect = new Rect(UI.screenWidth - Alert.Width, curBaseY - 24f, Alert.Width, 22f);
        Text.Font = GameFont.Small;

        // 鼠标移入时画出强调色
        if (Mouse.IsOver(zlRect))
        {
            Widgets.DrawHighlight(zlRect);
        }

        String foodText = "SomeFoodDescNew";

        // 在此处创建GUI
        GUI.BeginGroup(zlRect);

        // 可供食用天数小于等于3
        if (_cachedDaysWorthOfFood <= 3)
        {
            GUI.color = Color.yellow;
            // 可供食用天数小于等于1
            if (_cachedDaysWorthOfFood <= 1)
            {
                GUI.color = Color.red;
            }

            foodText = "LowFoodDescNew";
        }

        // 文本锚点在右上角
        Text.Anchor = TextAnchor.UpperRight;
        Rect rect = zlRect.AtZero();
        // 横坐标减少
        rect.xMax -= rightMargin;

        // 创建label
        Widgets.Label(rect, daysWorthOfHumanFood);
        // 文本锚点在左上角
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        // GUI创建结束
        GUI.EndGroup();

        // 创建提示
        TooltipHandler.TipRegion(zlRect, new TipSignal(
            () => string.Format(foodText.Translate(),
                _cachedNutrition.ToString("F1"),
                _cachedHumans.ToString("F1"),
                _cachedNeed.ToString("F1"),
                _cachedDaysWorthOfFood.ToString("F1") + addendumForFlavour),
            76515));

        curBaseY -= zlRect.height;
    }
}