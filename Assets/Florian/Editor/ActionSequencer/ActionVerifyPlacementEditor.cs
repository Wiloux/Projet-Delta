using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionVerifyPlacement))]
    public class ActionVerifyPlacementEditor : ActionGameObjectEditor {
        public override void OnInspector() {
            base.OnInspector();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Placement");
            EditorGUILayout.Space(SPACE);

            SerializedProperty raceManagerProp = serializedObject.FindProperty("raceManager");
            EditorGUILayout.PropertyField(raceManagerProp);

            EditorGUILayout.Space(SPACE);

            SerializedProperty markTypeProp = serializedObject.FindProperty("markType");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Relative")) {
                markTypeProp.enumValueIndex = 0;
            } else if (GUILayout.Button("Absolute")) {
                markTypeProp.enumValueIndex = 1;
            }
            GUILayout.EndHorizontal();

            switch (markTypeProp.enumValueIndex) {
                case 0:
                    SerializedProperty placementTypeProp = serializedObject.FindProperty("placementType");
                    EditorGUILayout.PropertyField(placementTypeProp);
                    break;
                case 1:
                    SerializedProperty placeProp = serializedObject.FindProperty("place");
                    EditorGUILayout.PropertyField(placeProp);
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            SerializedProperty markTypeProp = serializedObject.FindProperty("markType");
            switch (markTypeProp.enumValueIndex) {
                case 0:
                    SerializedProperty placementTypeProp = serializedObject.FindProperty("placementType");
                    int index = placementTypeProp.enumValueIndex;
                    name += " is " + placementTypeProp.enumNames[index] + " place";
                    break;
                case 1:
                    name += " is " + serializedObject.FindProperty("place").intValue + " place";
                    break;
        }
            return name;
        }
    }
}