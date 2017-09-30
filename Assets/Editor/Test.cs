using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Test : MonoBehaviour {

    [MenuItem("Test/SelectionType")]
    static void GetSelectionType()
    {
        if (Selection.activeObject == null)
            return;
        Debug.Log(Selection.activeObject.GetType());
    }
}
