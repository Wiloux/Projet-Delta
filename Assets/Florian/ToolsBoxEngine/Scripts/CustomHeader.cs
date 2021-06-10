using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolsBoxEngine {
    public class CustomHeader : MonoBehaviour {
        public float headerLength = 20f;

        private void OnValidate() {
            Debug.Log("Validated");
        }
    }
}