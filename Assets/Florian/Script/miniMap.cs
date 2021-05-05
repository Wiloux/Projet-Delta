using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Florian
{
    public class miniMap : MonoBehaviour
    {
        public GameObject pInMapPrefab;

        [System.Serializable]
        public struct PlayerMapInfo
        {
            public Transform pTransform;
            public GameObject pInMapGameObject;
            public RectTransform pInMapPos;
        }

        public List<PlayerMapInfo> players = new List<PlayerMapInfo>();
        public RectTransform map;
        public RectTransform playerInMap;
        public RectTransform map2dEnd;
        public Transform map3dParent;
        public Transform map3dEnd;

        private Vector3 normalized, mapped;

        private void Start()
        {

        }

        private void Update()
        {
            foreach (PlayerMapInfo p in players)
            {
                normalized = Divide(
                        map3dParent.InverseTransformPoint(p.pTransform.position),
                        map3dEnd.position - map3dParent.position
                    );
                normalized.y = normalized.z;
                mapped = Multiply(normalized, map2dEnd.localPosition);
                mapped.z = 0;
                p.pInMapPos.localPosition = mapped;
            }
        }
        public void AddPlayer(Transform transform, Sprite sprite)
        {
            PlayerMapInfo pInfo = new PlayerMapInfo();
            pInfo.pTransform = transform;
            pInfo.pInMapGameObject = Instantiate(pInMapPrefab, pInMapPrefab.transform.parent);
            pInfo.pInMapGameObject.SetActive(true);
            pInfo.pInMapGameObject.GetComponent<Image>().sprite = sprite;
            pInfo.pInMapPos = pInfo.pInMapGameObject.GetComponent<RectTransform>();
            players.Add(pInfo);
        }

        public void CheckDisplayMode(int pAmount)
        {
            switch (pAmount)
            {
                case 1:
                    map.anchorMin = new Vector2(0, 1);
                    map.anchorMax = new Vector2(0, 1);
                    map.anchoredPosition = new Vector2(257, -233);
                    map.localScale = new Vector3(3.2f, 3.2f, 3.2f);
                    break;
                case 2:
                    map.anchorMin = new Vector2(0, 0.5f);
                    map.anchorMax = new Vector2(0, 0.5f);
                    map.anchoredPosition = new Vector2(357, 12);
                    map.localScale = new Vector3(4.3f, 4.3f, 4.3f);
                    break;
                case 3:
                    map.anchorMin = new Vector2(0, 0.5f);
                    map.anchorMax = new Vector2(0, 0.5f);
                    map.anchoredPosition = new Vector2(1426, -267);
                    map.localScale = new Vector3(4, 4, 4);
                    break;
                case 4:
                    map.anchorMin = new Vector2(0.5f, 0.5f);
                    map.anchorMax = new Vector2(0.5f, 0.5f);
                    map.anchoredPosition = Vector2.zero;
                    map.localScale = new Vector3(2.7f, 2.7f, 2.7f);
                    break;
            }

        }

        private static Vector3 Divide(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        private static Vector3 Multiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}