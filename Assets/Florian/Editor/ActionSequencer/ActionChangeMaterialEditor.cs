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
            GUILayout.Label("Material");
            EditorGUILayout.Space(SPACE);

            SerializedProperty materialProp = serializedObject.FindProperty("material");
            EditorGUILayout.PropertyField(materialProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            SerializedProperty materialProp = serializedObject.FindProperty("material");
            name += " change material to ";
            if (materialProp.objectReferenceValue != null) { name += materialProp.objectReferenceValue.name; }
            return name;
        }
    }
}