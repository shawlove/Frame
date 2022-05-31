using System;
using System.Diagnostics;

namespace GameFrame.Flux
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ActionTypeAttribute : Attribute
    {
        public readonly string title;
        public readonly string desc;

        public ActionTypeAttribute(string title,string desc)
        {
            this.title = title;
            this.desc  = desc;
        }
    }
}