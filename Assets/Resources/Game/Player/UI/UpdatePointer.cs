using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePointer : MonoBehaviour
{
    public void ChangePointer()
    {
        Player player = transform.parent.gameObject.GetComponent<Player>();
        player.pointer.sprite = player.NewPointer;
    }
}
