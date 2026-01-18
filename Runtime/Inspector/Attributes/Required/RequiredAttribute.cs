using System;

namespace VahTyah
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredAttribute : Attribute
    {
        public string Message { get; }
        public bool IsError { get; }

        public RequiredAttribute(string message = null, bool isError = false)
        {
            Message = message;
            IsError = isError;
        }
    }
}
