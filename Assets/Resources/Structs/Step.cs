using System;
using UnityEngine.Serialization;

namespace Resources.Structs
{
    [Serializable]
    public struct Step
    {
        public string description;
        public string action;
        public bool isCompleted;
    }
}