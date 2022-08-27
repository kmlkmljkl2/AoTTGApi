using System;

namespace AottgBotLib.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HiddenAttribute : Attribute
    {
    }
}