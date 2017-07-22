using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CSVView : EditorWindow 
{
	TextAsset csv = null;
	string[][] arr = null;

	[MenuItem(CsvConfigUtility.Menu_CsvView)]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(CSVView));
	}

	void OnGUI()
	{
		TextAsset newCsv = EditorGUILayout.ObjectField("CSV", csv, typeof(TextAsset), false) as TextAsset;
		if(newCsv != csv)
		{
			csv = newCsv;
            var path = AssetDatabase.GetAssetPath(newCsv);
            var text = System.IO.File.ReadAllText(path, System.Text.Encoding.GetEncoding("gb2312"));
            arr = ParserCSV.Parse(text);
		}
		if(GUILayout.Button("Refresh") && csv != null)
			arr = ParserCSV.Parse(csv.text);

		if(csv == null)
			return;

		if(arr == null)
			arr = ParserCSV.Parse(csv.text);

		for(int i = 0 ; i < arr.Length ; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for(int j = 0 ; j < arr[i].Length ; j++)
			{
				EditorGUILayout.TextField(arr[i][j]);
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}
