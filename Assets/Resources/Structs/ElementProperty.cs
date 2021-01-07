using System;

namespace Resources.Structs
{
    [Serializable]
    public struct ElementProperties
    {
        public ElementProperty[] propertiesArray;
    }

    [Serializable]
    public struct ElementProperty
    {
        public string name;
        public string displayName;
        public string unitName;
        public string value;
    }
}