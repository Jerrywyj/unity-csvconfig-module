using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "生成/PersonObj")]
public class PersonObj : ScriptableObject {
    [System.Serializable]
    public class Item
    {
        public string Name;
        public string Sex;
    }
    public List<Item> list = new List<Item>();
}

