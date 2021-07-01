using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Florian {
    public class PlayerTrigger : MonoBehaviour {
        // 0f = black
        public List<Collider> _enterList = new List<Collider>();
        public List<Collider> _exitList = new List<Collider>();

        private void Start() {
            ResetList();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enter")) {
                if (_enterList.Contains(other)) {
                    StartCoroutine(TeleporterFade(this.gameObject, 0.04f, true));
                    _enterList.Remove(other);
                }
            } else if (other.CompareTag("Exit")) {
                if (_exitList.Contains(other) && this.gameObject.GetComponent<VFXManager>()._fadeImg.material.GetFloat("_Radius") != 25f) {
                    StartCoroutine(TeleporterFade(this.gameObject, 0.2f, false));
                    _exitList.Remove(other);
                }
            }
        }

        IEnumerator TeleporterFade(GameObject player, float time, bool inverse) {
            for (int i = 25; i >= 0; i--) {
                float radius = Mathf.Lerp(25f, 0f, i / 25f);
                if (inverse)
                    radius = 25f - radius;
                player.GetComponent<VFXManager>()._fadeImg.material.SetFloat("_Radius", radius);
                yield return new WaitForEndOfFrame();
            }
        }

        public void ResetList() {
            _enterList = CloneList(GameObject.Find("--- FadeColliders ---").GetComponent<PlayerList>().enterList);
            _exitList = CloneList(GameObject.Find("--- FadeColliders ---").GetComponent<PlayerList>().exitList);
        }

        private List<T> CloneList<T>(List<T> list) {
            List<T> clonedList = new List<T>();
            for (int i = 0; i < list.Count; i++) {
                clonedList.Add(list[i]);
            }
            return clonedList;
        }

    }
}
