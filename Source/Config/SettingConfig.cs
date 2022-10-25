using Verse;

namespace FoodAlert.Config;

/// <summary>
/// mod设置
/// </summary>
class SettingConfig : ModSettings
{
    /// <summary>
    /// 优化更新频率
    /// </summary>
    public bool Dynamicupdate = true;

    /// <summary>
    /// 食物等级
    /// </summary>
    public string FoodPreferability = "RawBad";

    /// <summary>
    /// 更新频率
    /// </summary>
    public float Updatefrequency = 400;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref FoodPreferability, "FoodPreferability", "RawBad", true);
        Scribe_Values.Look(ref Dynamicupdate, "Dynamicupdate", true);
        Scribe_Values.Look(ref Updatefrequency, "Updatefrequency", 400);
    }
}