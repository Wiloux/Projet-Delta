using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

namespace Florian
{

    public class Gliding : MonoBehaviour
    {

        private MovementController _playerMovementController;
        public bool isGliding;
        private float timer;
        public float timerDur;
        public float divideAmount;

        void Start()
        {
            timer = timerDur;
            _playerMovementController = GetComponent<MovementController>();
        }

        // Update is called once per frame
        void Update()
        {

            if (isGliding)
            {
                if (timer >= 0)
                {
                    _playerMovementController.physics.NegateVelocity(Axis.Y);
                    _playerMovementController.physics.AddVelocity(new Vector3(0f, -(_playerMovementController.physics.gravityCurve.amplitude * divideAmount), 0f));
                    timer -= Time.deltaTime;
                }
            }
            else
            {
                if (!_playerMovementController.Airborn)
                {
                    timer = timerDur;
                }
            }
        }

        //IEnumerator resetAbility()
        //{

         //   yield return new WaitForSeconds(coolDown);
         //   timer = timerDur;
        //}
    }
}
