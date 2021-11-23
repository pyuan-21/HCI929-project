using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class Utility : ScriptableObject
    {
        //[MenuItem("Tools/MyTool/Do It in C#")]
        //static void DoIt()
        //{
        //    EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
        //}

        public static GameObject FindGameObject(GameObject parent, string name)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(parent);
            GameObject currentObj;
            while (queue.Count > 0)
            {
                currentObj = queue.Dequeue();
                if (currentObj.name == name)
                {
                    return currentObj;
                }
                else if (currentObj.transform.childCount > 0)
                {
                    for (int i = 0; i < currentObj.transform.childCount; i++)
                    {
                        queue.Enqueue(currentObj.transform.GetChild(i).gameObject);
                    }
                }
            }
            return null;
        }
    }
}