using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {
    public Text content;
    public Text saveContent;
    void Start()
    {
        SettingTable table = CsvManager.LoadConfig<SettingTable>(CsvManager.settingTable);
        content.text = table.GetAt(1).Description;
        CsvManager.SaveConfig("gentable.csv", table);
        saveContent.text = table.UnLoad();
    }
}
