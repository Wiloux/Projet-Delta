using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallManager : MonoBehaviour
{
    public List<GameObject> checkpoints = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPlayerBestCheckPoint(other.gameObject, other.gameObject.transform.position);
            //Need to edit player script;
        }
    }

    public void CheckPlayerBestCheckPoint(GameObject fallenPlayer, Vector3 leftGroundPos)
    {
        float bestDistance = 99999999f;
        Transform bestCheckPoint = null;
        foreach (GameObject checkpoint in checkpoints)
        {
            if (bestDistance >= Vector3.Distance(checkpoint.transform.position, leftGroundPos))
            {
                bestCheckPoint.position = checkpoint.transform.position;
                bestDistance = Vector3.Distance(checkpoint.transform.position, leftGroundPos);
            }
        }

        fallenPlayer.transform.position = bestCheckPoint.position;
    }


    private void OnDrawGizmos()
    {
        //if (checkpoints.Count != 0)
        //{
        //    foreach (Transform checkpoint in checkpoints)
        //    {
        //        Gizmos.DrawWireCube(checkpoint.position, Vector2.one);
        //    }
        //}
    }
}
