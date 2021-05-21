using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform lookAt;       // The object our camera will be following, defaulted on the player

    [Header("Reference")]
    [SerializeField] private float boundX = 0.15f;    // Amount of space we can walk in x before the camera follows
    [SerializeField] private float boundY = 0.05f;    // Amount of space we can walk in y before the camera follows

    // LateUpdate, as we have to make sure the camera moves AFTER the player
    private void LateUpdate()
    {
        Vector3 delta = Vector3.zero;

        // This is to check if we're inside the bounds on the X axis
        float deltaX = lookAt.position.x - transform.position.x;
        if (deltaX > boundX || deltaX < -boundX)
        {
            if (transform.position.x < lookAt.position.x)
            {
                delta.x = deltaX - boundX;
            }
            else
            {
                delta.x = deltaX + boundX;
            }
        }

        // This is to check if we're inside the bounds on the Y axis
        float deltaY = lookAt.position.y - transform.position.y;
        if (deltaY > boundY || deltaY < -boundY)
        {
            if (transform.position.y < lookAt.position.y)
            {
                delta.y = deltaY - boundY;
            }
            else
            {
                delta.y = deltaY + boundY;
            }
        }

        transform.position += new Vector3(delta.x, delta.y, 0);
    }
}
