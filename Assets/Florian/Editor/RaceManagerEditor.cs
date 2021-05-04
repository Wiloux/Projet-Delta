using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Florian;

namespace Florian.Editor {
    [CustomEditor(typeof(RaceManager))]
    public class RaceManagerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Spawn checkpoints")) {
                RaceManager script = (RaceManager)target;
                script?.SpawnCheckpoints();
            }
        }

        private void OnSceneGUI() {
            serializedObject.Update();

            SerializedProperty pointsProp = serializedObject.FindProperty("checkpoints");
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < pointsProp.arraySize; i++) {
                points.Add(pointsProp.GetArrayElementAtIndex(i).vector3Value);
            }

            Handles.color = Color.red;
            for (int i = 0; i < points.Count; i++) {
                Handles.DrawDottedLine(points[i], points[(i + 1) % points.Count], 2f);
                points[i] = Handles.PositionHandle(points[i], Quaternion.identity);
                pointsProp.GetArrayElementAtIndex(i).vector3Value = points[i];
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}