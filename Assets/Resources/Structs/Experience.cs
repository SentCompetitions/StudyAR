using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
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
        public ElementProperties[] elementProperties;
        public Step[] actions;

        public Pack Pack
        {
            get
            {
                Experience experience = this;
                return GameManager.instance.packs.ToList().Where(
                    p => p.experiences.ToList().Contains(experience)
                ).ToArray()[0];
            }
        }

        public int GetFirstUnCompleteStep() => actions.ToList().IndexOf(actions.ToList().First(s => s.isCompleted == false));
    }
}