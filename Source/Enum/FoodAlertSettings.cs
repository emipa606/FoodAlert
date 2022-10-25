using Verse;

namespace FoodAlert.Config;

/// <summary>
/// mod设置
/// </summary>
internal class FoodAlertSettings : ModSettings
{
    /// <summary>
    /// 优化更新频率
    /// </summary>
    public bool dynamicupdate = true;

    /// <summary>
    /// 食物等级
    /// </summary>
    public string foodPreferability = "RawBad";

    /// <summary>
    /// 更新频率
    /// </summary>
    public float updatefrequency = 400;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref foodPreferability, "foodPreferability", "RawBad", true);
        Scribe_Values.Look(ref dynamicupdate, "dynamicupdate", true);
        Scribe_Values.Look(ref updatefrequency, "updatefrequency", 400);
    }
}