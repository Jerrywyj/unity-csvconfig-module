using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;

/// <summary>
/// 读取sqlite中的数据并加载到属性
/// </summary>
public class CsvManager : AssistManager<CsvManager>
{
    public static string FilesPath { get { return Application.streamingAssetsPath + "/CSV/"; } }
    public const string settingTable = "SettingTable.csv";
    public static Dictionary<string, CsvTable> loadedTables = new Dictionary<string, CsvTable>();

    #region Core
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="fileName">配置文件名, CSV文件</param>
    /// <param name="index">表示从第index行开始读取文件, 从0开始</param>
    public static T LoadConfig<T>(string fileName) where T : CsvTable
    {
        if (!loadedTables.ContainsKey(fileName))
        {
            string csvText = LoadTextAsset(fileName);
            CsvTable table = Activator.CreateInstance<T>();
            table.Load(csvText);
            loadedTables.Add(fileName, table);
            return (T)table;
            //try
            //{
            //    CsvTable table = Activator.CreateInstance<T>();
            //    table.Load(csvText);
            //    loadedTables.Add(fileName, table);
            //    return (T)table;
            //}
            //catch (System.Exception ex)
            //{
            //    Debug.LogError("转换失败" + ex);
            //    return null;
            //}
        }
        else
        {
            return (T)loadedTables[fileName];
        }
    }

    /// <summary>
    /// 将数据保存到配制表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="index"></param>
    /// <param name="objs"></param>
    public static void SaveConfig(string tableName, CsvTable table)
    {
        string csvData = table.UnLoad();
        SaveTextAsset(tableName, csvData);
    }
    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static string LoadTextAsset(string fileName)
    {
        if (File.Exists(FilesPath + fileName))
        {
            string text = File.ReadAllText(FilesPath + fileName);
            return text;
        }
        else
        {
            Debug.LogError(fileName + "不存在");
        }
        return null;
    }

    /// <summary>
    /// 保存字符串
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static void SaveTextAsset(string fileName, string text)
    {
        try
        {
            File.WriteAllText(FilesPath + fileName, text);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    #endregion
}
