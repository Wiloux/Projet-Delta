using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionSetRespawnRotationFactor))]
    public class ActionSetRespawnRotationFactorEditor : ActionGameObjectEditor {
        public override void OnInspector() {
            base.OnInspector();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Respawn orientation");
            EditorGUILayout.Space(SPACE);

            SerializedProperty raceManagerProp = serializedObject.FindProperty("factor");
            EditorGUILayout.PropertyField(raceManagerProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            name += " orientation factor " + serializedObject.FindProperty("factor").floatValue;
            return name;
        }
    }
}