using System;

namespace VahTyah
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ReadOnlyAttribute : Attribute
    {
    }
}
