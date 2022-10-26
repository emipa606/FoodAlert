using System;
using FoodAlert.Config;
using FoodAlert.Tools;
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
    {
        "DesperateOnly", "DesperateOnlyForHumanlikes", "RawBad", "RawTasty", "MealAwful", "MealSimple", "MealFine",
        "MealLavish"
    };

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="content"></param>
    public Settings(ModContentPack content) : base(content)
    {
        Config = GetSettings<SettingConfig>();
        _currentVersion = XmlApi.GetXml(".\\Mods\\2017538067\\About\\Manifest.xml",
            "Manifest/version");
    }

    /// <summary>
    /// 设置中显示的mod名称
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Food Alert 粮食储备";
    }

    /// <summary>
    /// 设置标签内容
    /// </summary>
    /// <param name="inRect"></param>
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        // 食物偏好等级
        listingStandard.Label("SettingDescription".Translate());
        // 按照食物等级创建UI
        foreach (var preference in Preference)
        {
            if (listingStandard.RadioButton(preference.Translate(), Config.FoodPreferability == preference, 0f,
                    (preference + "Tip").Translate()))
            {
                Config.FoodPreferability = preference;
            }
        }

        listingStandard.GapLine();
        // 更新频率
        listingStandard.Label("FA.updatetype.label".Translate());
        // 优化更新频率
        listingStandard.CheckboxLabeled("FA.typedynamic.label".Translate(), ref Config.Dynamicupdate,
            "FA.typedynamic.description".Translate());
        if (!Config.Dynamicupdate)
        {
            // 手动设置更新频率
            listingStandard.Label($"{"FA.updatetype.label".Translate()}: {Config.Updatefrequency.ToString("F0")} Tick",
                -1f, "FA.updatetype.label.tip".Translate());
            Config.Updatefrequency = listingStandard.Slider(Config.Updatefrequency, 100f, 10000f);
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