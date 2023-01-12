using System;
using UnityEditor;
using UnityEngine;

namespace Utility.Property
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ConditionalHideInInspectorAttribute : PropertyAttribute
    {
   
        public string ComparedProperty { get; private set; }
        public object ComparedPropertyValue { get; private set; }
        public bool IsNegative { get; private set; }

        public ConditionalHideInInspectorAttribute(string comparedProperty, object comparedPropertyValue, bool isNegative = false)
        {
            ComparedProperty = comparedProperty;
            ComparedPropertyValue = comparedPropertyValue;
            IsNegative = isNegative;
        }
    
        public ConditionalHideInInspectorAttribute(string useExclamationMark)
        {
            ComparedProperty = useExclamationMark;
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
            var conditionalAttribute = (ConditionalHideInInspectorAttribute)attribute;


            // Debug.Log(property.propertyPath);
            // Debug.Log(Attribute.comparedProperty);
            // Debug.Log(Attribute.comparedPropertyValue);
            // Debug.Log(property.propertyPath.Contains("."));
            string path = property.propertyPath.Contains(".") ? global::System.IO.Path.ChangeExtension(property.propertyPath, conditionalAttribute.ComparedProperty) : conditionalAttribute.ComparedProperty;

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

                path = global::System.IO.Path.ChangeExtension(property.propertyPath.Substring(0, LastIndex), conditionalAttribute.ComparedProperty);

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
                    return conditionalAttribute.IsNegative ? !ComparedField.boolValue : ComparedField.boolValue;
                case "Enum":
                {
                    // Debug.Log(ComparedField.intValue);
                    // Debug.Log((int)Attribute.comparedPropertyValue);
                    if (conditionalAttribute.IsNegative ? !ComparedField.intValue.Equals((int)conditionalAttribute.ComparedPropertyValue) : ComparedField.intValue.Equals((int)conditionalAttribute.ComparedPropertyValue))
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
}