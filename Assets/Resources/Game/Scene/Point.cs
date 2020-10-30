using UnityEngine;

    public class Point : MonoBehaviour
    {
        public Element boundElem;

        public void ElemToCenter()
        {
            if (boundElem) boundElem.transform.position = this.transform.position; // хз что отнимать
            else Debug.LogError("BoundElem is null! (Point.cs ElemToCenter())");
        }
    }
