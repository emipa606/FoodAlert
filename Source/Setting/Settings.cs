using System;
using FoodAlert.Config;
using UnityEngine;
using Verse;

namespace FoodAlert.Setting;

/// <summary>
/// mod配置
/// </summary>
internal class Settings : Mod
{
    /// <summary>
    /// mod设置
    /// </summary>
    public static SettingConfig Config;

    /// <summary>
    /// 当前版本
    /// </summary>
    private static string _currentVersion;

    /// <summary>
    /// 食物等级
    /// </summary>
    private static readonly string[] Preference =
        { "DesperateOnly", "RawBad", "RawTasty", "MealAwful", "MealSimple", "MealFine", "MealLavish" };

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="content"></param>
    public Settings(ModContentPack content) : base(content)
    {
        Config = GetSettings<SettingConfig>();
        // _currentVersion =ModLister.GetActiveModWithIdentifier("Mlie.FoodAlert");
    }

    /// <summary>
    /// mod标识符
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Food Alert";
    }

    /// <summary>
    /// 设置标签内容
    /// </summary>
    /// <param name="inRect"></param>
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        foreach (var preference in Preference)
        {
            if (listingStandard.RadioButton(preference, Config.FoodPreferability == preference))
            {
                Config.FoodPreferability = preference;
            }
        }

        listingStandard.Label("SettingExplanation".Translate());
        listingStandard.GapLine();
        listingStandard.Label("FA.updatetype.label".Translate());
        listingStandard.CheckboxLabeled("FA.typedynamic.label".Translate(), ref Config.Dynamicupdate,
            "FA.typedynamic.description".Translate());
        if (!Config.Dynamicupdate)
        {
            // Settings.Updatefrequency = listingStandard.SliderLabeled(
            //     "FA.typestatic.slider".Translate(Math.Round((decimal)Settings.Updatefrequency / 2500, 2)),
            //     Settings.Updatefrequency, 100, 10000);
        }

        if (_currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("FA.modversion".Translate(_currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();

        Config.Write();
    }
}