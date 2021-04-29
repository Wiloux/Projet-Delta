using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionGameObject))]
    public abstract class ActionGameObjectEditor : ActionEditor {
        public override void OnInspector() {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Target Game object");
            EditorGUILayout.Space(SPACE);

            SerializedProperty targetTypeProp = serializedObject.FindProperty("targetType");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Target")) {
                targetTypeProp.enumValueIndex = 0;
            } else if (GUILayout.Button("Trigger")) {
                targetTypeProp.enumValueIndex = 1;
            }
            GUILayout.EndHorizontal();

            switch (targetTypeProp.enumValueIndex) {
                case 0:
                    SerializedProperty targEntityProperty = serializedObject.FindProperty("targetEntity");
                    EditorGUILayout.PropertyField(targEntityProperty);
                    break;
                case 1:
                    EditorGUILayout.LabelField("Using the GameObjec that trigger the ActionSequencer");
                    break;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(SPACE);
        }

        protected override string ChangeName() {
            SerializedProperty targetTypeProp = serializedObject.FindProperty("targetType");
            string name = "";
            switch(targetTypeProp.enumValueIndex) {
                case 0:
                    SerializedProperty targetEntityProp = serializedObject.FindProperty("targetEntity");
                    if (targetEntityProp.objectReferenceValue != null) {
                        name += targetEntityProp.objectReferenceValue.name;
                    } else {
                        name += "null";
                    }
                    break;
                case 1:
                    name += "TRIGGERING OBJECT";
                    break;
            }
            return name;
        }
    }
}