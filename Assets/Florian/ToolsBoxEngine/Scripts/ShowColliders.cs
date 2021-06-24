using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowColliders : MonoBehaviour {
    public Color gizmosColor = Color.red;
    public bool alwaysDraw = false;
    private BoxCollider[] boxColliders;

    [ExecuteInEditMode]
    private void Awake() {
        boxColliders = GetComponents<BoxCollider>();
    }

    private void OnDrawGizmosSelected() {
        if (alwaysDraw) { return; }

        Draw();
    }

    private void OnDrawGizmos() {
        if (!alwaysDraw) { return; }

        Draw();
    }

    private void Draw() {
        if (boxColliders == null) { Awake(); }

        Matrix4x4 baseMatrix = Gizmos.matrix;

        Color color = gizmosColor;
        Gizmos.color = color;
        Matrix4x4 matrix = Gizmos.matrix;
        matrix.SetTRS(transform.position, transform.localRotation, Vector3.one);
        Gizmos.matrix = matrix;
        for (int i = 0; i < boxColliders.Length; i++) {
            Gizmos.DrawCube(Vector3.zero + boxColliders[i].center, boxColliders[i].size);
        }

        Gizmos.matrix = baseMatrix;
    }
}
