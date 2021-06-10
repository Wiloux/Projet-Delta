using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ToolsBoxEngine {
    //[CustomEditor(typeof(CustomHeader))]
    public class CustomHeaderEditor : Editor {
        private void OnValidate() {
            Debug.Log("Validated");
        }
    }
}