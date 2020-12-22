using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Resources.Structs
{
    [Serializable]
    public struct Experience
    {
        public string name;
        public int id;
        public string url;
        public string description;

        public GameObject[] allElements;
        public Step[] actions;

        public int GetFirstUnCompleteStep() => actions.ToList().IndexOf(actions.ToList().First(s => s.isCompleted == false));
    }
}