using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(InstructionUI)), CanEditMultipleObjects]
public class InstructionUIEditor : Editor {
         
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
