using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionSetActiveColliders))]
    public class ActionSetActiveCollidersEditor : ActionEditor {
        public override void OnInspector() {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Colliders");
            EditorGUILayout.Space(SPACE);

            SerializedProperty colliderObjectProp = serializedObject.FindProperty("colliderObject");
            EditorGUILayout.PropertyField(colliderObjectProp);

            SerializedProperty activeProp = serializedObject.FindProperty("active");
            EditorGUILayout.PropertyField(activeProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = "";
            name += serializedObject.FindProperty("colliderObject").name + " colliders " + serializedObject.FindProperty("active").boolValue;
            return name;
        }
    }
}