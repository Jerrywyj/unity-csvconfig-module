using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;

/// <summary>
/// 读取sqlite中的数据并加载到属性
/// </summary>
public class CsvManager:MonoBehaviour
{
    private static CsvManager instance = default(CsvManager);
    private static object lockHelper = new object();
    public static bool mManualReset = false;

    protected CsvManager() { }
    public static CsvManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockHelper)
                {
                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(CsvManager).ToString());
                        instance = go.AddComponent<CsvManager>();
                    }
                }
            }
            return instance;
        }
    }

    public static string FilesPath { get { return Application.streamingAssetsPath + "/CSV/"; } }
    public static Dictionary<string, CsvTable> loadedTables = new Dictionary<string, CsvTable>();

    #region Core
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="fileName">配置文件名, CSV文件</param>
    public static T LoadConfig<T>(string fileName) where T : CsvTable
    {
        if (!loadedTables.ContainsKey(fileName))
        {
            string csvText = LoadTextAsset(fileName);
            CsvTable table = Activator.CreateInstance<T>();
            table.Load(csvText);
            loadedTables.Add(fileName, table);
            return (T)table;
        }
        else
        {
            return (T)loadedTables[fileName];
        }
    }

    /// <summary>
    /// 将数据保存到配制表
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="table"></param>
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
