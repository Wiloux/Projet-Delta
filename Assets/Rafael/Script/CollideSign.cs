using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CollideSign : MonoBehaviour {
    private bool ended = false;

    //private void Start() {
    //    Rotate();
    //}

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Rotate();
        }
    }

    public void Rotate() {
        if (!ended) {
            ended = true;
            transform.DORotate(transform.localEulerAngles + new Vector3(0f, 1440f, 0f), .5f, RotateMode.FastBeyond360).OnComplete(() => { ended = false; });
        }
    }
}
