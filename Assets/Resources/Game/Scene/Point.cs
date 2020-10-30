using UnityEngine;

    public class Point : MonoBehaviour
    {
        public Element boundElem;

        public void ElemToCenter()
        {
            if (boundElem) boundElem.transform.position = transform.position - boundElem.transform.GetChild(0).localPosition / 100;
            else Debug.LogError("BoundElem is null! (Point.cs ElemToCenter())");
        }
    }
