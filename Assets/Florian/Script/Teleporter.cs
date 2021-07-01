using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

namespace Florian {
    public class Teleporter : MonoBehaviour {
        [SerializeField] private float teleportTime = 1f;

        public List<Transform> exits = new List<Transform>();

        public List<string> passedPlayers = new List<string>();

        public bool inverseRespawn = false;

        public bool original;

        public Transform tunnelBottom;
        public Transform tunnelBegin;
        public Transform cameraOnExit;

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) { return; }

            if (original) {
                StartCoroutine(FadeAndTp(other.gameObject, teleportTime));
            } else {
                for (int i = 0; i < passedPlayers.Count; i++) {
                    string n = passedPlayers[i];
                    if (other.transform.name == n) {
                        StartCoroutine(FadeAndTp(other.gameObject, teleportTime));
                    }
                }
            }
            //StartCoroutine(TeleporterFade(other.gameObject, 2f, false));
            //StartCoroutine(WaitFor(TeleporterFade(other.gameObject, 2f, false), Teleport, other.gameObject));
        }

        private IEnumerator FadeAndTp(GameObject player, float time) {
            MovementController mvtCtl = player.GetComponent<MovementController>();
            mvtCtl.GoToDestination(tunnelBottom.position, 20f);
            mvtCtl.Ghost(true);
            mvtCtl.cameraController.followPlayer = false;

            yield return StartCoroutine(TeleporterFade(player, time / 2f, false)); // Fadein

            mvtCtl.StopDestinationRoutine();
            Transform tpTrans = Teleport(player);
            if (tpTrans == null) { Debug.LogWarning("CA VA PA MARCHE"); }
            Teleporter tp = tpTrans.parent.GetComponent<Teleporter>();
            Debug.Log(mvtCtl.cameraController.transform.position);
            mvtCtl.cameraController.transform.position = tp.cameraOnExit.position;
            mvtCtl.cameraController.transform.rotation = tp.cameraOnExit.rotation;
            Debug.Log(mvtCtl.cameraController.transform.position);
            mvtCtl.GoToDestination(tp.tunnelBegin.position, 20f);

            yield return StartCoroutine(TeleporterFade(player, time / 2f, true)); // Fadeout

            mvtCtl.cameraController.followPlayer = true;
            mvtCtl.lockMovements = false;
            mvtCtl.Ghost(false);
        }

        private Transform Teleport(GameObject other) {
            if (exits.Count > 0 && original) {
                int index = Random.Range(0, exits.Count);
                foreach (Transform exit in exits) {
                    if (index != exits.IndexOf(exit)) {
                        exit.parent.GetComponent<Teleporter>().passedPlayers.Add(other.transform.name);
                        break;
                    }
                }

                other.transform.position = exits[index].position;
                other.transform.rotation = exits[index].rotation;

                if (exits[index].parent.GetComponent<Teleporter>().inverseRespawn) {
                    other.GetComponent<Movement>().respawnOrientationFactor = -1;
                } else {
                    other.GetComponent<Movement>().respawnOrientationFactor = 1;
                }

                return exits[index];
            } else if (!original && exits.Count > 0) {
                for (int i = 0; i < passedPlayers.Count; i++) {
                    string n = passedPlayers[i];
                    if (other.transform.name == n) {
                        int index = Random.Range(0, exits.Count);
                        if (exits[index].parent.GetComponent<Teleporter>().exits.Count != 0) {
                            foreach (Transform exit in exits) {
                                if (index != exits.IndexOf(exit)) {
                                    exit.parent.GetComponent<Teleporter>().passedPlayers.Add(other.transform.name);
                                    break;
                                }
                            }
                        }


                        passedPlayers.Remove(n);
                        other.transform.position = exits[index].position;
                        other.transform.rotation = exits[index].rotation;

                        if (exits[index].parent.GetComponent<Teleporter>().inverseRespawn) {
                            other.GetComponent<Movement>().respawnOrientationFactor = -1;
                        } else {
                            other.GetComponent<Movement>().respawnOrientationFactor = 1;
                        }

                        return exits[index];
                    }
                }
            }

            return null;
        }

        IEnumerator TeleporterFade(GameObject player, float time, bool inverse) {
            int framesNumber = Mathf.CeilToInt(60f * time);
            for (int i = 0; i <= framesNumber; i++) {
                float radius = Mathf.Lerp(25f, 0f, (i * i) / ((float)framesNumber * (float)framesNumber));
                if (inverse)
                    radius = 25f - radius;
                player.GetComponent<VFXManager>()._fadeImg.material.SetFloat("_Radius", radius);
                yield return new WaitForSeconds(1f / 60f);
            }
        }

        private IEnumerator WaitFor<T>(IEnumerator coroutine1, Tools.BasicDelegate<T> function, T arg) {
            yield return StartCoroutine(coroutine1);
            Debug.Log($"Started {coroutine1}");
            function(arg);
        }
    }
}