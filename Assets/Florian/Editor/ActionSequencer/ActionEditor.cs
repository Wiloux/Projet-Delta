using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Florian;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(Action))]
    public abstract class ActionEditor : UnityEditor.Editor {
        protected const float SPACE = 2f;

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Action system");
            EditorGUILayout.Space(SPACE);

            SerializedProperty waitTypeProperty = serializedObject.FindProperty("waitType");
            EditorGUILayout.PropertyField(waitTypeProperty);

            string waitTypeString = waitTypeProperty.enumNames[waitTypeProperty.enumValueIndex];
            if (waitTypeString.Equals(Action.WaitType.TIME.ToString()) || waitTypeString.Equals(Action.WaitType.BOTH.ToString())) {
                SerializedProperty timeToWaitProperty = serializedObject.FindProperty("timeToWait");
                EditorGUILayout.PropertyField(timeToWaitProperty);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(SPACE);

            OnInspector();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) {
                OnValidate();
            }
        }

        public abstract void OnInspector();

        protected virtual void OnValidate() {
            serializedObject.Update();
            serializedObject.targetObject.name = ChangeName();
            serializedObject.ApplyModifiedProperties();
        }

        protected abstract string ChangeName();
    }
}