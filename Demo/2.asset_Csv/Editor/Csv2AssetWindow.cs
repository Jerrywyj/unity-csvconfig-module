using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
public class Csv2AssetWindow : EditorWindow
{
    [MenuItem("Window/Csv2Asset")]
    static void CreateCsv2AssetWindow()
    {
        EditorWindow window = GetWindow<Csv2AssetWindow>();
        window.titleContent = new GUIContent("Csv2AssetWindow");
        window.position = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 500, 300);
        window.autoRepaintOnSceneChange = true;
    }
    private FieldInfo fielditem;
    private string _path;
    private List<FieldInfo> selectingFiled;
    private bool[] selectTog;
    private string[][] arr = null;
    Vector2 pos;
    ScriptableObject obj;
    SerializedProperty script;
    private void OnEnable()
    {
        var windowObj = new SerializedObject(this);
        script = windowObj.FindProperty("m_Script");
    }

    void ResetEnum(ScriptableObject arg)
    {
        if (arg != null)
        {
            selectingFiled = new List<FieldInfo>();
            RetriveArray(arg.GetType(), selectingFiled);
            selectTog = new bool[selectingFiled.Count];

            for (int i = 0; i < selectTog.Length; i++)
            {
                selectTog[i] = i == 0;
            }
        }
        else
        {
            selectingFiled = null;
            selectTog = null;
        }
    }

    private void RetriveArray(Type type, List<FieldInfo> listFields)
    {
        FieldInfo[] fields = type.GetFields();

        if (fields == null)
        {
            return;
        }

        for (int i = 0; i < fields.Length; i++)
        {
            Type innerType = fields[i].FieldType;
            if (innerType.IsClass && innerType != typeof(string))
            {
                if (innerType.GetMethod("Find") != null)
                {
                    listFields.Add(fields[i]);

                    Type listInnerType = innerType.GetMethod("Find").ReturnType;
                    if (true)
                    {
                        RetriveArray(listInnerType, listFields);
                    }
                }
                else if (true)
                {
                    RetriveArray(innerType, listFields);
                }
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.PropertyField(script);
        DocAndAsset();
        FunctionButtons();
        TryDrawKeys();
        TryDrawDataList();
    }

    private void TryDrawKeys()
    {
        //using (var hor = new EditorGUILayout.HorizontalScope())
        //{
        //    if (keys != null)
        //    {
        //        for (int i = 0; i < keys.Length; i++)
        //        {
        //            EditorGUILayout.LabelField(keys[i]);

        //        }
        //    }
        //}
    }

    void DocAndAsset()
    {
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("资源：", GUILayout.Width(100));
            var obj0 = (ScriptableObject)EditorGUILayout.ObjectField("", obj, typeof(ScriptableObject), false);

            if (obj0 != obj)
            {
                obj = obj0;
                ResetEnum(obj);
            }
            if (selectingFiled != null)
            {
                for (int i = 0; i < selectingFiled.Count; i++)
                {
                    var newOn = EditorGUILayout.Toggle(selectTog[i]);
                    if (selectTog[i] != newOn && newOn)
                    {
                        fielditem = selectingFiled[i];
                        selectTog[i] = newOn;
                    }

                    EditorGUILayout.LabelField(selectingFiled[i].Name, GUILayout.Width(60));
                }

                for (int i = 0; i < selectTog.Length; i++)
                {
                    selectTog[i] = fielditem == selectingFiled[i];
                }
            }

            if (GUILayout.Button("o", GUILayout.Width(EditorGUIUtility.singleLineHeight)))
            {
                ResetEnum(obj);
            }

        }

        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("文档：", GUILayout.Width(100));
            if (GUILayout.Button(_path ?? "选择文件（csv）"))
            {
                string path = EditorUtility.OpenFilePanel("选择csv文件", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    _path = path;
                }
            }
        }
    }
    void FunctionButtons()
    {
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("CSV<--"))
            {

            }
            if (GUILayout.Button("加载CSV"))
            {
                if (!string.IsNullOrEmpty(_path))
                {
                    string text = System.IO.File.ReadAllText(_path);
                    if (!string.IsNullOrEmpty(text))
                    {
                        arr = ParserCSV.Parse(text);
                    }
                }
            }
            if (GUILayout.Button("加载Asset"))
            {

            }
            if (GUILayout.Button("-->Asset"))
            {
                Type listType = fielditem.FieldType;

                Type type = listType.GetMethod("Find").ReturnType;
                FieldInfo[] fields = type.GetFields();

                for (int i = 0; i < fields.Length; i++)
                {
                    Debug.Log(fields[i]);
                }
                //fielditem.FieldType.GetMethod("Clear").Invoke(fielditem.GetValue(obj),null);
                fielditem.SetValue(obj,Activator.CreateInstance(fielditem.FieldType));
                for (int i = 0; i < arr.Length; i++)
                {
                    var instance1 = Activator.CreateInstance(type);
                    for (int j = 0; j < arr[i].Length; j++)
                    {
                        fields[j].SetValue(instance1, arr[i][j]);
                    }
                    fielditem.FieldType.GetMethod("Add").Invoke(fielditem.GetValue(obj), new object[] { instance1 });
                }
                EditorUtility.SetDirty(obj);
            }
        }

    }
    void TryDrawDataList()
    {
        using (var scro = new EditorGUILayout.ScrollViewScope(pos))
        {
            pos = scro.scrollPosition;
            int x = -1;
            int y = -1;
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    using (var hor = new EditorGUILayout.HorizontalScope())
                    {
                        for (int j = 0; j < arr[i].Length; j++)
                        {
                            arr[i][j] = EditorGUILayout.TextField(arr[i][j]);
                        }
                        if (GUILayout.Button("-"))
                        {
                            x = i;
                        }
                    }
                }
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    if (arr != null && arr.Length > 0)
                    {
                        for (int i = 0; i < arr[0].Length; i++)
                        {
                            if (GUILayout.Button("-"))
                            {
                                y = i;
                            }
                        }
                        if (GUILayout.Button("o", GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                bool rawEnpty = true;
                                for (int j = 0; j < arr[i].Length; j++)
                                {
                                    arr[i][j] = EditorGUILayout.TextField(arr[i][j]);
                                    bool empty = string.IsNullOrEmpty(arr[i][j]);
                                    rawEnpty &= empty;
                                }
                                if (rawEnpty)
                                {
                                    x = i;
                                }
                            }
                        }
                    }

                }
            }
            if (x != -1)
            {
                var arr0 = new string[arr.Length - 1][];
                int xindex = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (x != i)
                    {
                        int yindex = 0;
                        arr0[xindex] = new string[arr[i].Length];
                        for (int j = 0; j < arr[i].Length; j++)
                        {
                            arr0[xindex][yindex] = arr[i][j];
                            yindex++;
                        }
                        xindex++;
                    }
                }
                arr = arr0;
            }
            if (y != -1)
            {
                var arr0 = new string[arr.Length][];
                int xindex = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    int yindex = 0;
                    arr0[xindex] = new string[arr[i].Length - 1];
                    for (int j = 0; j < arr[i].Length; j++)
                    {
                        if (y != j)
                        {
                            arr0[xindex][yindex] = arr[i][j];
                            yindex++;
                        }
                    }
                    xindex++;
                }
                arr = arr0;
            }
        }
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("添加一条"))
            {
                int i = 0;
                int length = 0;
                string[][] arr0 = null;
                if (arr != null)
                {
                    arr0 = new string[arr.Length + 1][];
                    for (; i < arr.Length; i++)
                    {
                        length = arr[i].Length;
                        arr0[i] = new string[length];
                        for (int j = 0; j < arr[i].Length; j++)
                        {
                            arr0[i][j] = arr[i][j];
                        }
                    }
                }
                else
                {
                    arr0 = new string[1][];
                    arr0[0] = new string[1];
                }

                arr0[i] = new string[length];
                arr = arr0;
            }
            if (GUILayout.Button("添加一列"))
            {
                string[][] arr0 = null;
                if (arr != null)
                {
                    arr0 = new string[arr.Length][];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr0[i] = new string[arr[i].Length + 1];
                        for (int j = 0; j < arr[i].Length; j++)
                        {
                            arr0[i][j] = arr[i][j];
                        }
                    }
                }
                else
                {
                    arr0 = new string[1][];
                    arr0[0] = new string[1];
                }

                arr = arr0;
            }
        }
    }
}
