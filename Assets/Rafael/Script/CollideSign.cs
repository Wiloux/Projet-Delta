using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CollideSign : MonoBehaviour
{
    private bool ended = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !ended)
        {
            ended = true;
            transform.DORotate(transform.rotation * new Vector3(0, 620, 0), .5f, RotateMode.FastBeyond360).OnComplete(() => { ended = false; });
        }
    }
}
