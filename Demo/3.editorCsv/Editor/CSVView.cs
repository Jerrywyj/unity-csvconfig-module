using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using System;
/// <summary>
/// 用于编辑csv文件
/// </summary>
public class CSVView : EditorWindow
{
    private enum Type
    {
        String,
        Int,
        Byte,
        Bool,
        Float,
    }

    private List<List<string>> arr = new List<List<string>>();
    private string filePath;
    private const string Pref_lastFile = "csv_view_last_file_path";
    private const string Pref_lastCsFolder = "csv_view_last_cs_folder";
    private Vector2 scrollPos;
    private ReorderableList reorderList;
    private List<Type> types;

    [MenuItem("Window/CsvView")]
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
        InitReorderList();
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
    private void InitReorderList()
    {
        reorderList = new ReorderableList(arr, typeof(string[]));
        reorderList.headerHeight = EditorGUIUtility.singleLineHeight;
        reorderList.drawHeaderCallback = DrawTableHeader;
        reorderList.drawElementCallback = DrawTableElement;
        reorderList.onAddCallback = AddNewLine;
    }

    private void DrawTableElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var root = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        if (arr.Count > 0)
        {
            var width = rect.width / arr[index].Count;
            var headRect = new Rect(root.x, root.y, width, root.height);
            for (int i = 0; i < arr[index].Count; i++)
            {
                var contentRect = GetPaddingRect(headRect, 1f);
                arr[index][i] = EditorGUI.TextField(contentRect, arr[index][i]);
                headRect.x += width;
            }

        }
    }

    private void DrawTableHeader(Rect rect)
    {
        var root = new Rect(rect.x + 12, rect.y, rect.width - 12, EditorGUIUtility.singleLineHeight * 1.1f);
        if (types != null)
        {
            var width = root.width / types.Count;
            var headRect = new Rect(root.x, root.y, width - 20, root.height);
            var deleteRect = new Rect(root.x + width - 20, root.y + 2f, 20, root.height - 4f);
            for (int i = 0; i < types.Count; i++)
            {
                var contentRect = GetPaddingRect(headRect, 2f);
                types[i] = (Type)EditorGUI.EnumPopup(contentRect, types[i]);
                if (GUI.Button(deleteRect, "-"))
                {
                    DeleteArray(i);
                }
                headRect.x += width;
                deleteRect.x += width;
            }
        }
    }
    private void AppendArray()
    {
        if (arr.Count == 0)
        {
            arr.Add(new List<string>());
        }

        foreach (var item in arr)
        {
            item.Add("");
        }

        types.Add(Type.String);
    }

    private void DeleteArray(int li)
    {
        if (arr != null && arr.Count > 0 && arr[0].Count > li)
        {
            foreach (var item in arr)
            {
                item.RemoveAt(li);
            }
        }
        if (types != null && types.Count > li)
        {
            types.RemoveAt(li);
        }
    }

    private Rect GetPaddingRect(Rect orgin, float span)
    {
        var contentRect = new Rect(orgin.x + span, orgin.y + span, orgin.width - span * 2f, orgin.height - span * 2f);
        return contentRect;
    }

    private void DrawToolButtons()
    {
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(20)))
            {
                AppendArray();
            }
            if (GUILayout.Button("Table", EditorStyles.miniButtonRight, GUILayout.Width(60)))
            {
                CreateScriptFromTable();
            }

        }

    }

    private void CreateScriptFromTable()
    {
        var folder = Application.streamingAssetsPath;
        if (PlayerPrefs.HasKey(Pref_lastCsFolder))
        {
            folder = PlayerPrefs.GetString(Pref_lastCsFolder);
        }
        var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        var path = EditorUtility.SaveFilePanel("保存C#文件", folder, fileName, "cs");
        if (!string.IsNullOrEmpty(path))
        {
            fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var script = TableCodeGen.Generate(ListToArray(arr), types.ConvertAll<string>(x => x.ToString().ToLower()).ToArray(), fileName);
            System.IO.File.WriteAllText(path, script);
            AssetDatabase.Refresh();
        }
    }

    private void AddNewLine(ReorderableList list)
    {
        if (arr != null && arr.Count > 1)
        {
            var newLine = new string[arr[0].Count];
            arr[arr.Count - 1].CopyTo(newLine, 0);
            arr.Add(new List<string>(newLine));
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
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFilePanel("打开csv", GetBastFolder(), "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    filePath = path;
                    LoadArrayDataFromFile();
                    CatchFilePath();
                }
            }
            if (string.IsNullOrEmpty(filePath) && GUILayout.Button("创建", GUILayout.Width(60)))
            {
                CreateNewCsvTable();
            }
            if (!string.IsNullOrEmpty(filePath) && GUILayout.Button("保存", GUILayout.Width(60)))
            {
                SaveArrayDataToFile();
            }
        }

    }

    private void CreateNewCsvTable()
    {
        var dir = Application.dataPath;
        if (!string.IsNullOrEmpty(filePath))
        {
            dir = System.IO.Path.GetDirectoryName(filePath);
        }

        var newfilePath = EditorUtility.SaveFilePanel("创建csv文档", dir, "new_table", "csv");
        if (!string.IsNullOrEmpty(newfilePath))
        {
            filePath = newfilePath;
            if (!File.Exists(filePath))
            {
                System.IO.File.Create(filePath);
            }
            EditorApplication.delayCall += () =>
            {
                LoadArrayDataFromFile();
            };
            PlayerPrefs.SetString(Pref_lastFile, filePath);
            PlayerPrefs.Save();
        }
    }

    string GetBastFolder()
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            return System.IO.Path.GetDirectoryName(filePath);
        }
        return Application.dataPath;
    }
    void DrawArrayInfo()
    {
        reorderList.DoLayoutList();
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
                    arr.Clear();
                    var newarray = ParserCSV.Parse(sr.ReadToEnd());
                    foreach (var item in newarray)
                    {
                        arr.Add(new List<string>(item));
                    }

                    UpdateTypes();
                }
            }
        }
    }
    void UpdateTypes()
    {
        if (arr != null && arr.Count > 0)
        {
            int length = arr[0].Count;
            if (types == null)
            {
                types = new List<Type>();
                for (int i = 0; i < length; i++)
                {
                    types.Add(Type.String);
                }
            }
            else if (types.Count > length)
            {
                for (int i = 0; i < types.Count - length; i++)
                {
                    types.RemoveAt(types.Count - 1);
                }
            }
            else if (types.Count < length)
            {
                for (int i = 0; i < length - types.Count; i++)
                {
                    types.Add(Type.String);
                }
            }
        }
    }
    void SaveArrayDataToFile()
    {
        if (arr != null)
        {
            string data = UParserCSV.UParser(ListToArray(arr));
            System.IO.File.WriteAllText(filePath, data, System.Text.Encoding.GetEncoding("gb2312"));
        }
    }

    static string[][] ListToArray(List<List<string>> arr)
    {
        var newarray = arr.ConvertAll(x => x.ToArray()).ToArray();
        return newarray;
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
