using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(InteractionObjectControl))]
[CanEditMultipleObjects]
public class InteractionObjectControlEditor : Editor
{
    private InteractionObjectControl _control = null;

    private GUIContent _onMiniGameEvent = new GUIContent("미니게임 실행 전 발생 이벤트");
    private GUIContent _offMiniGameEvent = new GUIContent("미니게임 실행 후 발생 이벤트");
    private GUIContent _eventBefore = new GUIContent("인터렉션 전 발생 이벤트");
    private GUIContent _eventAfter = new GUIContent("인터렉션 후 발생 이벤트");
    
    private void OnEnable()
    {
        _control = (InteractionObjectControl) target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        _control.isInteractable = EditorGUILayout.Toggle("인터렉션 가능 여부", _control.isInteractable);

        if (_control.isInteractable == false)
            return;

        _control.interactionClass = (InteractionClass) EditorGUILayout.ObjectField("인터렉션 클래스",
            _control.interactionClass, typeof(InteractionClass), true);

        _control.interactionType = (InteractionType) EditorGUILayout.EnumPopup("인터렉션 타입", _control.interactionType);
        _control.hasMiniGame = EditorGUILayout.Toggle("미니게임 여부", _control.hasMiniGame);
        
        if (_control.hasMiniGame)
        {
            _control.miniGame =
                (MiniGameClass) EditorGUILayout.ObjectField("미니게임 오브젝트", _control.miniGame, typeof(MiniGameClass),
                    true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("onMiniGameEvent"),_onMiniGameEvent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offMiniGameEvent"),_offMiniGameEvent);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("EventCallBeforeInteraction"),_eventBefore);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EventCallAfterInteraction"),_eventAfter);

        serializedObject.ApplyModifiedProperties();
    }
}
