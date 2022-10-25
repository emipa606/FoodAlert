using System;
using System.Xml;

namespace FoodAlert.Tools;

/// <summary>
/// XML相关api
/// </summary>
public class XmlApi
{
    public static string GetXml(String xmlPaths, String node)
    {
        //XmlDocument读取xml文件
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPaths);
        //根据节点顺序逐步读取
        //读取第一个节点
        String value = xmlDoc.SelectSingleNode(node)?.InnerText;
        return value;
    }
}