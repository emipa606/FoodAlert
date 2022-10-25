using System;

namespace FoodAlert.Tools;

/// <summary>
/// 工具类
/// </summary>
public class Debug
{
    /// <summary>
    /// mod名称
    /// </summary>
    private static string ModName => "Food Alert";

    /// <summary>
    /// 序列化Log输出
    /// </summary>
    public static void Log(string log)
    {
        Verse.Log.Message(string.Format("[{0}] {1} {2}", ModName, DateTime.Now, log));
    }
}