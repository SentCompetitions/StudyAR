using System.Collections;
using System.Collections.Generic;
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

