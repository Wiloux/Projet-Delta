using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Florian.ActionSequencer.Editor {
    [CustomEditor(typeof(ActionPlayAnimator))]
    public class ActionPlayAnimatorEditor : ActionGameObjectEditor {
        public override void OnInspector() {
            base.OnInspector();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Animator");
            EditorGUILayout.Space(SPACE);

            SerializedProperty animProp = serializedObject.FindProperty("anim");
            EditorGUILayout.PropertyField(animProp);

            SerializedProperty animVarProp = serializedObject.FindProperty("animVar");
            EditorGUILayout.PropertyField(animVarProp);

            SerializedProperty varStateProp = serializedObject.FindProperty("varState");
            EditorGUILayout.PropertyField(varStateProp);

            EditorGUILayout.EndVertical();
        }

        protected override string ChangeName() {
            string name = base.ChangeName();
            name += " animator " + serializedObject.FindProperty("animVar").stringValue;
            return name;
        }
    }
}