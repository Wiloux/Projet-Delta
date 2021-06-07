using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    public Transform[] exits;
    
    //Gizmos
    private BoxCollider col;
    private Transform child;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
        child = GetComponentInChildren<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && exits.Length > 0)
        {
            int index = Random.Range(0, exits.Length);
            Debug.Log(exits.Length);

            other.transform.position = exits[index].position;
            other.transform.rotation = exits[index].rotation;
            Debug.Log("Player : " + other.transform.position + " ; Exit : " + exits[index].position);
            
        }
    }


    private void OnDrawGizmos()
    {

        if(col && child)
        {

            //Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.DrawCube(transform.position, col.size);

            Gizmos.DrawSphere(child.position, 1);
        }

    }
}
