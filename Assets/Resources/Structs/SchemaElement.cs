using System.Collections.Generic;

namespace Resources.Structs
{
    public struct SchemaElement
    {
        public Dictionary<string, List<Dictionary<string, string>>> crossings;
        public ElementProperty[] properties;
        public string assetName;
    }
}