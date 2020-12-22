using System;

namespace Resources.Structs
{
    [Serializable]
    public struct Pack
    {
        public string name;
        public int id;
        public string url;
        public string description;
        public string subject;
        public string subjectDisplay;
        public bool isOfficial;
        public bool isPublished;
        public string editors;
        public Experience[] experiences;
    }
}