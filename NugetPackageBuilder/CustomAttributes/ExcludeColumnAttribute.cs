using System;

namespace ShivOhm.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExcludeColumnAttribute : Attribute
    {
        public bool AllowAdd { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowRead { get; set; }
    }
}
