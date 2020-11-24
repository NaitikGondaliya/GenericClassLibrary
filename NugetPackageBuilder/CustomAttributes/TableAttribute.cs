using System;

namespace ShivOhm.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
    }
}
