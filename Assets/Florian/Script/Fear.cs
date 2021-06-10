using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Florian;
using ToolsBoxEngine;

public class Fear : MonoBehaviour {
    private MovementController mvtController;

    //public float sphereSize;
    public AmplitudeCurve sphereGrowthCurve = null;
    public float slowAmount;
    public float castingTime;
    public bool castable = true;
    public float coolDownDuration;
    private Material sphereMat;
    private MeshRenderer sphereRend;
    private ParticleSystem ps;
    private bool disapear;

    public float alphaTimer;

    void Start() {
        mvtController = GetComponent<MovementController>();
        sphereRend = GameObject.Find(mvtController.playerName + "/PlayerRoot/Vfx/RageBall").GetComponent<MeshRenderer>();
        ps = GameObject.Find(mvtController.playerName + "/PlayerRoot/Vfx/Rage").GetComponent<ParticleSystem>();
        sphereRend.gameObject.SetActive(false);
        sphereMat = sphereRend.material;
    }

    public void Activate() {
        StartCoroutine(Cast());
    }

    public void AbilityUpdate() {
        if (!sphereRend.transform.gameObject.activeSelf)
            return;

        sphereGrowthCurve.timer += Time.deltaTime;

        if (sphereGrowthCurve.timer / sphereGrowthCurve.duration > 0.5f) {
            disapear = true;
        }

        Collide(sphereGrowthCurve.GetRatio() * sphereGrowthCurve.amplitude * 0.5f);

        if (sphereGrowthCurve.timer >= sphereGrowthCurve.duration) {
            StartCoroutine(Cooldown(coolDownDuration));
            sphereGrowthCurve.timer = 0;
        } else {
            sphereRend.gameObject.transform.localScale = new Vector3(
                sphereGrowthCurve.GetRatio() * sphereGrowthCurve.amplitude,
                sphereGrowthCurve.GetRatio() * sphereGrowthCurve.amplitude,
                sphereGrowthCurve.GetRatio() * sphereGrowthCurve.amplitude
            );
        }

        if (disapear) {
            alphaTimer += -0.14f * Time.deltaTime;
            sphereMat.SetFloat("OverallAlpha", alphaTimer);
            if (sphereMat.GetFloat("OverallAlpha") <= 0) {
                disapear = false;
                sphereRend.gameObject.SetActive(false);
            }
        }
    }

    private void Collide(float size) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, size);
        foreach (Collider hitCollider in hitColliders) {
            MovementController hittedMvtController = hitCollider.GetComponent<MovementController>();
            if (hittedMvtController != null && hittedMvtController.playerName != mvtController.playerName) {
                hittedMvtController.physics.Slow(slowAmount);
            }
        }
    }

    private IEnumerator Cast() {
        ps.Play();
        mvtController.riderAnim.SetTrigger("fear");
        sphereMat.SetFloat("OverallAlpha", 0.08f);
        alphaTimer = 0.08f;
        castable = false;
        yield return new WaitForSeconds(castingTime);
        sphereGrowthCurve.timer = 0;
        sphereRend.gameObject.SetActive(true);
    }

    private IEnumerator Cooldown(float time) {
        yield return new WaitForSeconds(time);
        castable = true;
    }

    //private void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(sphereRend.gameObject.transform.position, sphereGrowthCurve.amplitude * sphereGrowthCurve.GetRatio() * 0.5f);
    //}
}
