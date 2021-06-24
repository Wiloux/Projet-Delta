using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionChangeMaterial))]
    public class ActionChangeMaterialEditor : ActionGameObjectEditor {
        public override void OnInspector() {
            base.OnInspector();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Color To");
            EditorGUILayout.Space(SPACE);

            SerializedProperty materialProp = serializedObject.FindProperty("colorTo");
            EditorGUILayout.PropertyField(materialProp);

            SerializedProperty transitionTimeProp = serializedObject.FindProperty("transitionTime");
            EditorGUILayout.PropertyField(transitionTimeProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            SerializedProperty colorProp = serializedObject.FindProperty("colorTo");
            name += " change color to ";
            if (colorProp.objectReferenceValue != null) { name += colorProp.colorValue.ToString(); }
            return name;
        }
    }
}