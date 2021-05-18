using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Florian;
using ToolsBoxEngine;

public class Fear : MonoBehaviour
{
    private MovementController mvtController;

    public float sphereSize;
    public float slowAmount;
    private float coolDown;
    public float coolDownDuration;
    // Start is called before the first frame update
    void Start()
    {
        mvtController = GetComponent<MovementController>();
        sphereSize = 50;
    }

    // Update is called once per frame
    void Update()
    {
        if (mvtController.player.GetButtonDown("Action") && coolDown <= 0)
        {
            coolDown = coolDownDuration;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereSize);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.GetComponent<MovementController>() && hitCollider.transform.name != transform.name)
                {
                    hitCollider.GetComponent<MovementController>().physics.Slow(slowAmount * Vector3.forward);
                }
            }
        }
        else if (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereSize);
    }
}
