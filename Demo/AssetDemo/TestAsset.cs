using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="myAsset.asset",menuName = "生成/测试数据资源",order = 0)]
public class TestAsset : ScriptableObject {

    [System.Serializable]
    public class InnerClass
    {
        [System.Serializable]
        public class InnerClassInnerClass
        {
            public string message;
            public string data;
        }

        public string key;
        public string info;

        public List<InnerClassInnerClass> testList2 = new List<InnerClassInnerClass>();
    }

  

    public int id;
    public string dataName;
    public List<InnerClass> testList1 = new List<InnerClass>();
}
