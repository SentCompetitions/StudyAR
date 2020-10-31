using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Resources.Game
{
    [Serializable]
    public struct Experience
    {
        public GameObject[] allElements;
        public Step[] steps;

        public int GetFirstUnCompleteStep() => steps.ToList().IndexOf(steps.ToList().First(s => s.isCompleted == false));
    }

    [Serializable]
    public struct Step
    {
        public string displayName;
        public string eventName;
        public bool isCompleted;
    }
}