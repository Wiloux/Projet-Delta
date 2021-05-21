using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Florian
{
    public class MountThrowing : MonoBehaviour
    {
        [SerializeField] private float _throwForce;
        [SerializeField] private Vector3 _throwBoxDimension;
        public float _cooldown;
        public float _timer;

        public void MountThrow()
        {
            //throw forward

            Collider[] colliders = Physics.OverlapBox(transform.forward + transform.localPosition, _throwBoxDimension);

            foreach (Collider pushedObject in colliders)
            {
                if (pushedObject.CompareTag("Player") && pushedObject.name != gameObject.name)
                {
                    //stun pushedobject
                    //anim rebond
                    //player speed = 0
                    _timer = _cooldown;
                }
                else if (pushedObject.CompareTag("Obstacle") || pushedObject.CompareTag("Wall"))
                {
                    //player speed = 0 + stun
                }
                else
                {
                    //player lose a part of his speed
                }

            }
        }
    }
}
