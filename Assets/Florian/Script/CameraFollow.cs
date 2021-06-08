using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian {
    public class CameraFollow : MonoBehaviour
    {
        public GameObject player;

        private void LateUpdate()
        {
            transform.position = player.transform.position;
            transform.rotation = player.transform.rotation;
        }

    }
}