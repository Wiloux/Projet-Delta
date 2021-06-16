using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionAllPlayerEnter))]
    public class ActionAllPlayerEnterEditor : ActionGameObjectEditor {
        public override void OnInspector() {
            base.OnInspector();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Animator");
            EditorGUILayout.Space(SPACE);

            SerializedProperty raceManagerProp = serializedObject.FindProperty("raceManager");
            EditorGUILayout.PropertyField(raceManagerProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            name += " passed";
            return name;
        }
    }
}