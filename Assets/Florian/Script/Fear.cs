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
    public AmplitudeCurve sphereGrowthCurve = null;
    public float sphereGrowthSpd;
    public float slowAmount;
    public float castingTime;
    private float coolDown;
    public float coolDownDuration;
    private Material sphereMat;
    private MeshRenderer sphereRend;
    private ParticleSystem ps;
    private bool disapear;

    public float _t;
    // Start is called before the first frame update
    void Start()
    {
        sphereRend = GameObject.Find("Vfx/RageBall").GetComponent<MeshRenderer>();
        ps = GameObject.Find("Vfx/Rage").GetComponent<ParticleSystem>();
        mvtController = GetComponent<MovementController>();
        sphereRend.gameObject.SetActive(false);
        sphereMat = sphereRend.material;

        sphereGrowthCurve.amplitude = sphereSize;
        //  sphereMat = Resources.Load("Assets/Resources/Mat/RageBall" + GetComponent<MovementController>().playerName + "", typeof(Material)) as Material;
        // Debug.Log("Assets/Resources/Mat/RageBall" + GetComponent<MovementController>().playerName + "");
    }

    // Update is called once per frame
    void Update()
    {
        if (mvtController.player.GetButtonDown("Action") && coolDown <= 0)
        {
            StartCoroutine(Cast());
        }
        else if (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
        }

        if (!sphereRend.transform.gameObject.activeSelf)
            return;


        sphereGrowthCurve.timer += Time.deltaTime;

        if (sphereGrowthCurve.timer / sphereGrowthCurve.duration > 0.5f)
        {
            disapear = true;
        }

        if (sphereGrowthCurve.timer >= sphereGrowthCurve.duration)
        {
            sphereGrowthCurve.timer = 0;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereSize);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.GetComponent<MovementController>() && hitCollider.transform.name != transform.name)
                {
                    hitCollider.GetComponent<MovementController>().physics.Slow(slowAmount * Vector3.forward);
                }
            }
        }
        else
        {
            sphereRend.gameObject.transform.localScale = new Vector3(sphereGrowthCurve.GetRatio() * sphereSize, sphereGrowthCurve.GetRatio() * sphereSize, sphereGrowthCurve.GetRatio() * sphereSize);
        }

        if (disapear)
        {
            _t += -0.14f * Time.deltaTime;
            sphereMat.SetFloat("OverallAlpha", _t);
            if (sphereMat.GetFloat("OverallAlpha") <= 0)
            {
                disapear = false;
                sphereRend.gameObject.SetActive(false);
            }
        }

    }

    private IEnumerator Cast()
    {
        ps.Play();
        sphereMat.SetFloat("OverallAlpha", 0.08f);
        _t = 0.08f;
        coolDown = coolDownDuration;
        yield return new WaitForSeconds(castingTime);
        sphereGrowthCurve.timer = 0;
        sphereRend.gameObject.SetActive(true);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereSize);
    }
}
