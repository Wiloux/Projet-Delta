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
    }
}