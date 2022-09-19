using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ConditionalHideInInspectorAttribute : PropertyAttribute
{
   
    public string comparedProperty { get; private set; }
    public object comparedPropertyValue { get; private set; }

    public ConditionalHideInInspectorAttribute(string comparedProperty, object comparedPropertyValue)
    {
        this.comparedProperty = comparedProperty;
        this.comparedPropertyValue = comparedPropertyValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ConditionalHideInInspectorAttribute))]
public class Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // var Attribute = (ConditionalHideInInspectorAttribute)attribute;
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
        var Attribute = (ConditionalHideInInspectorAttribute)attribute;

        // Debug.Log(property.propertyPath);
        // Debug.Log(Attribute.comparedProperty);
        // Debug.Log(Attribute.comparedPropertyValue);
        // Debug.Log(property.propertyPath.Contains("."));
        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, Attribute.comparedProperty) : Attribute.comparedProperty;

        var ComparedField = property.serializedObject.FindProperty(path);
        // Debug.Log(ComparedField);
        if (ComparedField == null)
        {
            int LastIndex = property.propertyPath.LastIndexOf(".");
            // Debug.Log(Attribute.comparedEnum);
            if (LastIndex == -1)
            {
                return true;
            }

            path = System.IO.Path.ChangeExtension(property.propertyPath.Substring(0, LastIndex), Attribute.comparedProperty);

            ComparedField = property.serializedObject.FindProperty(path);
            // Debug.Log(ComparedField);
            if (ComparedField == null)
            {
                return true;
            }
        }

        switch (ComparedField.type)
        {
            case "bool":
                return ComparedField.boolValue.Equals(Attribute.comparedPropertyValue);
            case "Enum":
                {
                    // Debug.Log(ComparedField.intValue);
                    // Debug.Log((int)Attribute.comparedPropertyValue);
                    if (ComparedField.intValue.Equals((int)Attribute.comparedPropertyValue))
                    {
                        return true;
                    }
                    //if((ComparedField.intValue & (int)Attribute.ComparedValue) != 0)
                    //{
                    //    Debug.Log(ComparedField.intValue + "   " + (int)Attribute.ComparedValue);
                    //}
                    //return (ComparedField.intValue & (int)Attribute.ComparedValue) != 0;
                    return false;
                }
        }

        return true;
    }
}
#endif