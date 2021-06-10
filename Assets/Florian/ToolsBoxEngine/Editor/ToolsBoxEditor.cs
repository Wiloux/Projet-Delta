using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace ToolsBoxEngine {
    public class ToolsBoxEditor {
    }

    [Serializable]
    public static class WindowEditor {
        public static int headerLength = 20;
        [SerializeField] private static List<GameObject> headers;

        static WindowEditor() {
            EditorApplication.hierarchyChanged += OnUpdateHeaders;
        }

        [MenuItem("GameObject/ToolsBox/Header", false, 0)]
        public static void CreateHeader() {
            if (headers == null) {
                headers = new List<GameObject>();
            }

            string baseName = "";
            for (int i = 0; i < headerLength; i++) {
                baseName += "-";
            }

            GameObject lastHeader = new GameObject(baseName);
            headers.Add(lastHeader);
        }

        static void OnUpdateHeaders() {
            if (headers == null || headers.Count == 0) { return; }

            for (int i = 0; i < headers.Count; i++) {
                if (headers[i] == null)
                    headers.RemoveAt(i);

                string name = headers[i].name;
                name = Regex.Replace(name, @"- ", string.Empty);
                if (name.Length < headerLength) {
                    int delta = headerLength - name.Length;
                    //if (delta % 2 != 0) { delta--; }

                    string wantedName = "";
                    for (int j = 0; j < delta / 2; j++) {
                        wantedName += "-";
                    }

                    wantedName += " " + name + " ";

                    for (int j = 0; j < delta / 2; j++) {
                        wantedName += "-";
                    }

                    headers[i].name = wantedName;
                }
            }
        }
    }
}
