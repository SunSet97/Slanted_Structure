// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

#if TMPRO_AVAILABLE

using TMPro.EditorUtilities;
using UnityEditor;

namespace Naninovel
{
    [CustomEditor(typeof(UI.RevealableTMProText), true)]
    [CanEditMultipleObjects]
    public class RevealableTMProTextEditor
        #if UNITY_2020_1_OR_NEWER
        : TMP_EditorPanelUI
        #else
        : TMP_UiEditorPanel
        #endif
    {
        private SerializedProperty revealFadeWidth;
        private SerializedProperty slideClipRect;
        private SerializedProperty italicSlantAngle;
        private SerializedProperty rubyVerticalOffset;
        private SerializedProperty rubySizeScale;
        private SerializedProperty fixArabicText;
        private SerializedProperty fixArabicFarsi;
        private SerializedProperty fixArabicTextTags;
        private SerializedProperty fixArabicPreserveNumbers;

        protected override void OnEnable ()
        {
            base.OnEnable();

            revealFadeWidth = serializedObject.FindProperty("revealFadeWidth");
            slideClipRect = serializedObject.FindProperty("slideClipRect");
            italicSlantAngle = serializedObject.FindProperty("italicSlantAngle");
            rubyVerticalOffset = serializedObject.FindProperty("rubyVerticalOffset");
            rubySizeScale = serializedObject.FindProperty("rubySizeScale");
            fixArabicText = serializedObject.FindProperty("fixArabicText");
            fixArabicFarsi = serializedObject.FindProperty("fixArabicFarsi");
            fixArabicTextTags = serializedObject.FindProperty("fixArabicTextTags");
            fixArabicPreserveNumbers = serializedObject.FindProperty("fixArabicPreserveNumbers");
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.LabelField("Revealing", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.PropertyField(revealFadeWidth);
                EditorGUILayout.PropertyField(slideClipRect);
                EditorGUILayout.PropertyField(italicSlantAngle);
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.LabelField("Ruby Text", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.PropertyField(rubyVerticalOffset);
                EditorGUILayout.PropertyField(rubySizeScale);
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.LabelField("Arabic Text Support", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.PropertyField(fixArabicText);
                EditorGUI.BeginDisabledGroup(!fixArabicText.boolValue);
                EditorGUILayout.PropertyField(fixArabicFarsi);
                EditorGUILayout.PropertyField(fixArabicTextTags);
                EditorGUILayout.PropertyField(fixArabicPreserveNumbers);
                EditorGUI.EndDisabledGroup();
            }
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }
    } 
}

#endif