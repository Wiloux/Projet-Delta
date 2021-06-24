using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Florian;

public class MoveCactus : MonoBehaviour
{
    private bool doTween = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !doTween)
        {
            doTween = true;
            transform.DOPunchRotation(other.transform.right * other.GetComponent<MovementController>().Speed / 3, 3, 5).OnComplete(()=> { doTween = false; });
        }
    }
}
