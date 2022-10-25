using FoodAlert.Config;
using FoodAlert.Setting;
using Verse;

namespace FoodAlert.Data;

/// <summary>
/// 存放游戏内所有的数据文件
/// </summary>
class ModData
{
    /// <summary>
    /// Config类
    /// </summary>
    public static SettingConfig Config;

    /// <summary>
    /// 初始化
    /// </summary>
    public ModData()
    {
        Config = Settings.Config;
    }
}