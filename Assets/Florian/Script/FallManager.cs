using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

public class FallManager : MonoBehaviour
{

    public static FallManager instance;
    public List<Transform> checkpoints = new List<Transform>();

    private void Awake()
    {
        instance = this;
        checkpoints.Clear();
        for (int i = 0; i < transform.childCount; i++) {
            checkpoints.Add(transform.GetChild(i));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPlayerBestCheckPoint(other.gameObject, other.gameObject.transform.position);
            //Need to edit player script;
        }
    }

    public void CheckPlayerBestCheckPoint(GameObject fallenPlayer, Vector3 leftGroundPos, bool respawnOrientationBackward = false)
    {
        float bestDistance = Mathf.Infinity;
        Transform bestCheckPoint = null;
        foreach (Transform checkpoint in checkpoints)
        {
            if (bestDistance >= Vector3.Distance(checkpoint.transform.position, leftGroundPos))
            {
                bestCheckPoint = checkpoint;
                bestDistance = Vector3.Distance(checkpoint.transform.position, leftGroundPos);
            }
        }

        fallenPlayer.transform.position = bestCheckPoint.position;
        //fallenPlayer.transform.LookAt(bestCheckPoint.transform.TransformDirection(transform.forward));
        //fallenPlayer.transform.forward = new Vector3(bestCheckPoint.transform.TransformDirection(transform.forward).x, bestCheckPoint.transform.TransformDirection(transform.forward).y, bestCheckPoint.transform.TransformDirection(transform.forward).z);

        Vector3 forward = bestCheckPoint.forward;
        if (respawnOrientationBackward)
            forward = -bestCheckPoint.forward;

        fallenPlayer.transform.forward = forward;
        //Vector3 rotation = fallenPlayer.transform.localEulerAngles;
        //fallenPlayer.transform.localEulerAngles = rotation.Override(rotation.y * respawnOrientaitonBackward, Axis.Y);
        //fallenPlayer.transform.rotation.eulerAngles = new Vector3(fallenPlayer.transform.rotation.x, bestCheckPoint.transform.rotation.y, fallenPlayer.transform.rotation.z);
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
