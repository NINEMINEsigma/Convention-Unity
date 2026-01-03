using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Convention.WindowsUI.Variant.PropertiesWindow;

namespace Convention.WindowsUI.Variant
{
    public enum InspectorDrawType
    {
        // Auto
        Auto = -1,
        // String
        Text = 0,
        // Bool
        Toggle = 1 << 1,
        // Sripte
        Image = 1 << 2,
        // Transform
        Transform = 1 << 3,
        // Container
        List = 1 << 4 + 1, Dictionary = 1 << 5 + 1, Array = 1 << 6 + 1,
        // Object
        Reference = 1 << 7, Structure = 1 << 8,
        // Method
        Button = 1 << 9,
        // Enum
        Enum = 1 << 10
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class InspectorDrawAttribute : Attribute
    {
        public readonly InspectorDrawType drawType;
        public readonly bool isUpdateAble = true;
        public readonly bool isChangeAble = true;
        public readonly string name = null;
        // Get Real Inspector Name: Field
        public readonly string nameGenerater = null;
        // Get Real Enum Names: Method
        public readonly string enumGenerater = null;

        public InspectorDrawAttribute()
        {
            this.drawType = InspectorDrawType.Auto;
        }
        public InspectorDrawAttribute(InspectorDrawType drawType = InspectorDrawType.Auto, bool isUpdateAble = true,
                                      bool isChangeAble = true, string name = null, string nameGenerater = null, string enumGenerater = null)
        {
            this.drawType = drawType;
            this.isUpdateAble = isUpdateAble;
            this.isChangeAble = isChangeAble;
            this.name = name;
            this.nameGenerater = nameGenerater;
            this.enumGenerater = enumGenerater;
        }
    }

    public interface IOnlyFocusThisOnInspector
    {

    }
}
