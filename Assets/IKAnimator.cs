using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKAnimator : MonoBehaviour {
    public Transform anchor;
    public LayerMask layerMask;
    [Range(0, 1f)] public float DistanceToGround;

    private Animator anim;

    private Vector3[] hitpoints;

    private void Start() {
        anim = GetComponent<Animator>();
        hitpoints = new Vector3[2];
    }

    private void OnAnimatorIK(int layerIndex) {
        if (anim != null) {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

            RaycastHit hit;
            Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
            Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up + Vector3.down, Color.red);
            if (Physics.Raycast(ray, out hit, 1f, layerMask)) {
                Debug.Log(hit.collider.gameObject.name);
                Vector3 footposition = hit.point;
                hitpoints[0] = hit.point;
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, footposition);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
            //Vector3 footposition = anchor.position;
            //anim.SetIKPosition(AvatarIKGoal.LeftFoot, footposition);

            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            //footposition = anchor.position;
            //anim.SetIKPosition(AvatarIKGoal.RightFoot, footposition);
            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
            Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up + Vector3.down, Color.red);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
                Vector3 footposition = hit.point;
                hitpoints[1] = hit.point;
                anim.SetIKPosition(AvatarIKGoal.RightFoot, footposition);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        for (int i = 0; i < hitpoints.Length; i++) {
            Gizmos.DrawSphere(hitpoints[i], 0.1f);
        }
    }
}
