using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ConditionalHideInInspectorAttribute : PropertyAttribute
{
   
    public string ComparedPropertyName { get; private set; }
    public object ComparedValue { get; private set; }

    public ConditionalHideInInspectorAttribute(string InComparedPropertyName, object InComparedValue)
    {
        ComparedPropertyName = InComparedPropertyName;
        ComparedValue = InComparedValue;
    }
}

[CustomPropertyDrawer(typeof(ConditionalHideInInspectorAttribute))]
public class Drawer : PropertyDrawer
{
    ConditionalHideInInspectorAttribute Attribute;
    SerializedProperty ComparedField;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (CanDraw(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return CanDraw(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0.0f;
    }
    private bool CanDraw(SerializedProperty property)
    {
        Attribute = attribute as ConditionalHideInInspectorAttribute;

        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, Attribute.ComparedPropertyName) : Attribute.ComparedPropertyName;

        ComparedField = property.serializedObject.FindProperty(path);

        if (ComparedField == null)
        {
            int LastIndex = property.propertyPath.LastIndexOf(".");

            if (LastIndex == -1)
            {
                return true;
            }

            path = System.IO.Path.ChangeExtension(property.propertyPath.Substring(0, LastIndex), Attribute.ComparedPropertyName);

            ComparedField = property.serializedObject.FindProperty(path);

            if (ComparedField == null)
            {
                return true;
            }
        }

        switch (ComparedField.type)
        {
            case "bool":
                return ComparedField.boolValue.Equals(Attribute.ComparedValue);
            case "Enum":
                {
                    if (ComparedField.intValue.Equals((int)Attribute.ComparedValue))
                    {
                        return true;
                    }

                    return (ComparedField.intValue & (int)Attribute.ComparedValue) != 0;
                }
        }

        return true;
    }
}