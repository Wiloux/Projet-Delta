using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian {
    public class Teleporter : MonoBehaviour {

        public List<Transform> exits = new List<Transform>();

        public List<string> playerNames = new List<string>();

        public bool inverseRespawn = false;

        public bool original;

        private void OnTriggerStay(Collider other) {
            if (other.CompareTag("Player") && exits.Count > 0 && original) {

                int index = Random.Range(0, exits.Count);
                foreach (Transform exit in exits) {
                    if (index != exits.IndexOf(exit)) {
                        exit.parent.GetComponent<Teleporter>().playerNames.Add(other.transform.name);
                        break;
                    }
                }

                if (other.GetComponent<VFXManager>()._fadeImg.material.GetFloat("_Radius") <= 5f)
                {
                    other.transform.position = exits[index].position;
                    other.transform.rotation = exits[index].rotation;
                }

                if (exits[index].parent.GetComponent<Teleporter>().inverseRespawn) {
                    other.GetComponent<Movement>().respawnOrientationFactor = -1;
                } else {
                    other.GetComponent<Movement>().respawnOrientationFactor = 1;
                }
            } else if (!original && other.CompareTag("Player") && exits.Count > 0) {
                for (int i = 0; i < playerNames.Count; i++) {
                    string n = playerNames[i];
                    if (other.transform.name == n) {
                        int index = Random.Range(0, exits.Count);
                        if (exits[index].parent.GetComponent<Teleporter>().exits.Count != 0) {
                            foreach (Transform exit in exits) {
                                if (index != exits.IndexOf(exit)) {
                                    exit.parent.GetComponent<Teleporter>().playerNames.Add(other.transform.name);
                                    break;
                                }
                            }
                        }


                        if (other.GetComponent<VFXManager>()._fadeImg.material.GetFloat("_Radius") <= 5f)
                        {
                            playerNames.Remove(n);
                            other.transform.position = exits[index].position;
                            other.transform.rotation = exits[index].rotation;
                        }

                        if (exits[index].parent.GetComponent<Teleporter>().inverseRespawn) {
                            other.GetComponent<Movement>().respawnOrientationFactor = -1;
                        } else {
                            other.GetComponent<Movement>().respawnOrientationFactor = 1;
                        }
                    }

                }

            }
        }
    }
}