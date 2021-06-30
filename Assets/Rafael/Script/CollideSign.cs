using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Florian;

public class CollideSign : MonoBehaviour {
    private bool ended = false;
    [SerializeField] private float speedUp = 5f;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Rotate();

            MovementController mvtCtl = other.GetComponent<MovementController>();
            if (mvtCtl == null) { return; }

            if (!mvtCtl.Decelerating)
                mvtCtl.physics.AddVelocity(Vector3.forward * speedUp);
        }
    }

    public void Rotate() {
        if (!ended) {
            ended = true;
            transform.DORotate(transform.localEulerAngles + new Vector3(0f, 1440f, 0f), .5f, RotateMode.FastBeyond360).OnComplete(() => { ended = false; });
        }
    }
}
