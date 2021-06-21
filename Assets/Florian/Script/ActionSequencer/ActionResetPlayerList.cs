using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer
{
    public class ActionResetPlayerList : ActionGameObject
    {
        protected override void OnStart() { }

        protected override void OnExecute()
        {
            MovementController movementController = entity.GetComponent<MovementController>();
            if (movementController == null) { return; }

            movementController.ResetPlayerTriggerList();
        }

        public override bool IsActionEnded()
        {
            return true;
        }

        public override void ResetAction()
        {
            base.ResetAction();
        }
    }
}

