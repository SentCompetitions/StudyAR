using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstructionUI : EventTrigger
{
    public Animator animator;
    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        base.OnPointerClick(eventData);
        animator.SetBool("Instruction", !animator.GetBool("Instruction"));
    }
}

[CustomEditor(typeof(InstructionUI)), CanEditMultipleObjects]
public class InstructionUIEditor : Editor {
         
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
