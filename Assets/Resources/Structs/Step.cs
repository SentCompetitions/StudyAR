using System;
using System.Collections.Generic;
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
        //public Dictionary<string, SchemaElement> schemaElements;

        [NonSerializedAttribute] public Texture2D imageTexture;
        [NonSerializedAttribute] public byte[] imageBytes;
    }
}