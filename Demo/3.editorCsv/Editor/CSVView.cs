using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
/// <summary>
/// 用于编辑csv文件
/// </summary>
public class CSVView : EditorWindow
{
    private string[][] arr = null;
    private string filePath;
    private const string Pref_lastFile = "csv_view_last_file_path";
    private Vector2 scrollPos;

    [MenuItem("Window/CsvConfig/CsvView")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(CSVView));
        window.titleContent = new GUIContent("csv 文件编辑");
        window.minSize = new Vector2(400, 300);
        window.position = new Rect(Screen.width * .5f - 200, Screen.height * .5f - 150, 400, 300);
    }
    void OnEnable()
    {
        LoadFilePathFromPref();
        LoadArrayDataFromFile();
    }
    void OnGUI()
    {
        DrawFileSelect();
        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
        {
            scrollPos = scroll.scrollPosition;
            if (arr != null)
            {
                DrawArrayInfo();
            }
            else
            {
                DrawNoArrayInfo();
            }
        }
        DrawToolButtons();
    }

    private void DrawToolButtons()
    {
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
            {
                AddNewLine();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存",EditorStyles.miniButtonRight,GUILayout.Width(60)))
            {
                SaveArrayDataToFile();
            }
        }
       
    }

    private void AddNewLine()
    {
        if (arr != null)
        {
        }
    }

    private void DrawNoArrayInfo()
    {
        EditorGUILayout.HelpBox("请选择需要查看，或编辑的csv文件", MessageType.Info);
    }

    void DrawFileSelect()
    {
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginChangeCheck();
            filePath = EditorGUILayout.TextField(filePath);
            if (EditorGUI.EndChangeCheck())
            {
                LoadArrayDataFromFile();
                CatchFilePath();
            }
            if (GUILayout.Button("选择",GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFilePanel("打开csv", GetBastFolder(), "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    filePath = path;
                    LoadArrayDataFromFile();
                    CatchFilePath();
                }
            }
        }
       
    }
    string GetBastFolder()
    {
        if (!string.IsNullOrEmpty( filePath))
        {
            return System.IO.Path.GetDirectoryName(filePath);
        }
        return Application.dataPath;
    }
    void DrawArrayInfo()
    {
        for (int i = 0; i < arr.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < arr[i].Length; j++)
            {
                arr[i][j] =  EditorGUILayout.TextField(arr[i][j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        
       
    }

    void LoadFilePathFromPref()
    {
        if (PlayerPrefs.HasKey(Pref_lastFile))
        {
            filePath = PlayerPrefs.GetString(Pref_lastFile);
        }
    }
    void LoadArrayDataFromFile()
    {
        if (System.IO.File.Exists(filePath))
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding("gb2312")))
                {
                    arr = ParserCSV.Parse(sr.ReadToEnd());
                }
            }
        }
    }
    void SaveArrayDataToFile()
    {
        if(arr != null)
        {
            string data = UParserCSV.UParser(arr);
            System.IO.File.WriteAllText(filePath, data, System.Text.Encoding.GetEncoding("gb2312"));
        }
    }

    void CatchFilePath()
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            PlayerPrefs.SetString(Pref_lastFile, filePath);
            PlayerPrefs.Save();
        }
    }
}
