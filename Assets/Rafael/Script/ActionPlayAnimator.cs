using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian.ActionSequencer
{
    public class ActionPlayAnimator : ActionGameObject
    {
        public Animator anim;
        public string animVar;
        public bool varState = true;

        protected override void OnStart() { }

        protected override void OnExecute()
        {
            if(anim.GetBool(animVar) != varState)
            {
                Debug.Log("Anim Activated");
                anim.SetBool(animVar, varState);
            }
        }

        public override bool IsActionEnded()
        {
            return true;
        }

        #region Setters

        public void SetAction(WaitType waitType, TargetType targetType, RaceManager race, int checkpointIndex)
        {
            base.SetAction(waitType, 0f, targetType);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(WaitType waitType, float timeToWait, TargetType targetType, RaceManager race, int checkpointIndex)
        {
            base.SetAction(waitType, timeToWait, targetType);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(WaitType waitType, TargetType targetType, GameObject targetEntity, RaceManager race, int checkpointIndex)
        {
            base.SetAction(waitType, targetType, targetEntity);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(WaitType waitType, float timeToWait, TargetType targetType, GameObject targetEntity, RaceManager race, int checkpointIndex)
        {
            base.SetAction(waitType, timeToWait, targetType, targetEntity);
            SetAction(race, checkpointIndex);
        }

        public void SetAction(RaceManager race, int checkpointIndex)
        {
            //this.raceManager = race;
            //this.checkpointIndex = checkpointIndex;
        }

        #endregion
    }
}