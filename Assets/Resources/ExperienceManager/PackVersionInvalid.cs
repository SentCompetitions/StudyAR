using System;

namespace Resources.ExperienceManager
{
    public class PackVersionInvalid: Exception
    {
        public PackVersionInvalid() : base() { }
        public PackVersionInvalid(string message) : base(message) { }
    }
}