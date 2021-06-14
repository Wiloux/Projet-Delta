using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionCheckpointReached))]
    public class ActionCheckpointReachedEditor : ActionGameObjectEditor {
        public override void OnInspector() {
            base.OnInspector();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Checkpoint");
            EditorGUILayout.Space(SPACE);

            SerializedProperty raceManagerProp = serializedObject.FindProperty("raceManager");
            EditorGUILayout.PropertyField(raceManagerProp);

            SerializedProperty checkpointIndexProp = serializedObject.FindProperty("checkpointIndex");
            EditorGUILayout.PropertyField(checkpointIndexProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            int index = serializedObject.FindProperty("checkpointIndex").intValue;
            name += " checkpoint " + index.ToString();
            return name;
        }
    }
}