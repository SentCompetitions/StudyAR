using System;
using UnityEngine;

namespace Resources.Structs
{
    [Serializable]
    public struct Step
    {
        public string description;
        public string action;
        public bool isCompleted;
        public int id;
        public string image;

        [NonSerializedAttribute] public Texture2D imageTexture;
        [NonSerializedAttribute] public byte[] imageBytes;
    }
}