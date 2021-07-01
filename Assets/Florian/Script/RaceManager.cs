using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;
using Florian.ActionSequencer;
using UnityEngine.UI;
using TMPro;


namespace Florian {
    public class Character : MonoBehaviour {

    }

    public class RaceManager : MonoBehaviour {
        [Serializable]
        public struct Place {
            public int playerId;
            public int points;
        }

        public Place[] places;
        public int lapsNumber = 0;
        public Vector3[] checkpoints = null;
        [HideInInspector] public List<Character> characters;

        [SerializeField] private List<int> checkpointReached = null;
        [SerializeField] private List<int> laps = null;
        public int[] placements = null;
        public int[] endPlacement = null;
        public bool raceStarted = false;

        [Header("EndGame")]
        public GameObject playerPlacementEndGame;
        public GameObject placementPanel;

        [Header("Checkpoints")]
        public Vector3 checkpointsSize = Vector3.zero;

        public GameObject startCountdownUI;

        public GameManager gameManager;

        public List<Sprite> playerColorImages = new List<Sprite>();
        public List<Sprite> playerNumberImages = new List<Sprite>();

        #region Properties

        public int NumberOfCharacters {
            get { return characters.Count; }
        }

        public int NumberOfCheckpoints {
            get { return checkpoints.Length; }
        }

        public bool EveryoneEnded {
            get {
                for (int i = 0; i < characters.Count; i++) {
                    if (!HasEnded(CharacterId(characters[i]))) {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion

        public void SpawnCheckpoints() {
            if (transform.childCount > 0) {
                for (int i = 0; i < transform.childCount; i++) {
                    if (transform.GetChild(i).name == "Checkpoints") {
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    }
                }
            }

            Transform parent = new GameObject("Checkpoints").transform;
            parent.parent = transform;
            for (int i = 0; i < checkpoints.Length; i++) {
                //GameObject insta = Instantiate(, checkpoints[i], Quaternion.identity);
                GameObject insta = new GameObject("Checkpoints " + i);
                insta.transform.position = checkpoints[i];
                insta.transform.parent = parent;
                AreaTrigger lastArea = insta.AddComponent<AreaTrigger>();
                lastArea.SetCollider(checkpointsSize);
                ActionCheckpointReached action = lastArea.AddAction<ActionCheckpointReached>(AreaTrigger.TriggerType.ON_ENTER);
                action.SetAction(Florian.ActionSequencer.Action.WaitType.NONE, ActionGameObject.TargetType.TRIGGERING, this, i);
            }
        }

        private void Update() {
            UpdateRace();
        }

        private void UpdateRace() {
            ComputePlacements();

            for (int i = 0; i < placements.Length; i++) {
                for (int j = 0; j < characters.Count; j++) {
                    if (placements[i] == j) {
                        MovementController player = characters[j] as MovementController;
                        player.Placement = i + 1;
                    }
                }
            }

            for (int i = 0; i < NumberOfCharacters; i++) {
                if (LapsDone(i)) {
                    DoALapse(i);
                    MovementController player = characters[i] as MovementController;
                    player.Laps = laps[i];
                    if (HasEnded(i)) {
                        for (int j = 0; j < endPlacement.Length; j++)
                            if (endPlacement[j] == -1) {
                                endPlacement[j] = i;
                                break;
                            }

                        player.stopYou = true;
                        if (EveryoneEnded) {
                            placements = endPlacement;
                            EndRace();
                        }
                    }
                }
            }
        }

        public void StartRace(Character[] characters) {
            SetCharacters(characters);

            placements = new int[characters.Length];
            places = new Place[characters.Length];
            endPlacement = new int[characters.Length];

            laps = new List<int>();
            checkpointReached = new List<int>();
            for (int i = 0; i < characters.Length; i++) {
                endPlacement[i] = -1;
                Debug.Log(characters[i].name);
                laps.Add(0);
                checkpointReached.Add(0);
                placements[i] = 0;
                (characters[i] as MovementController).maxLaps = lapsNumber;
                (characters[i] as MovementController).Laps = 0;
                places[i] = new Place();
            }
            AudioManager.Instance.PlayMusicWithFade(ClipsContainer.Instance.AllClips[15], 0.7f);
            AudioManager.Instance.PlayAmbiant(ClipsContainer.Instance.AllClips[16], 0.3f);
            TeleportCharacters(checkpoints[0], new Vector3(0, 180f, 0));
            StartCoroutine(BlockPlayerAtTheStartOfTheRace(characters));
            raceStarted = true;
        }

        IEnumerator BlockPlayerAtTheStartOfTheRace(Character[] characters) {
            startCountdownUI.SetActive(true);
            for (int i = 0; i < characters.Length; i++) {
                characters[i].GetComponent<MovementController>().lockMovements = true;
                characters[i].GetComponent<MovementController>().riderAnim.enabled = false;
            }

            yield return new WaitForSeconds(3);

            AudioManager.Instance.PlayMusicWithFade(ClipsContainer.Instance.AllClips[14], 0.3f);

            startCountdownUI.SetActive(false);
            for (int i = 0; i < characters.Length; i++) {
                characters[i].GetComponent<MovementController>().riderAnim.enabled = true;
                characters[i].GetComponent<MovementController>().lockMovements = false;
            }
        }
        public void TeleportCharacters(Vector3 position, Character character) {
            character.transform.position = position.Override(position.y + 5f, Axis.Y);
            Debug.Log(character.transform.position + " .. " + position);
        }

        public void TeleportCharacters(Vector3 position) {
            for (int i = 0; i < characters.Count; i++) {
                TeleportCharacters(position, characters[i]);
            }
        }

        public void TeleportCharacters(Vector3 position, Vector3 orientation, Character character) {
            character.transform.position = position.Override(position.y + 5f, Axis.Y);
            character.transform.localEulerAngles = orientation;
        }

        public void TeleportCharacters(Vector3 position, Vector3 orientation) {
            for (int i = 0; i < characters.Count; i++) {
                TeleportCharacters(position + new Vector3(2f, 0) * i, orientation, characters[i]);
            }
        }

        public void EndRace() {
            placementPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            AudioManager.Instance.PlayMusicWithCrossFade(ClipsContainer.Instance.AllClips[17], 0.3f);
            AudioManager.Instance.PlayAmbiant(null, 0f);
            PlacementPanels();

        }

        public void PlacementPanels() {

            List<Transform> AllPanels = new List<Transform>();

            int place = 0;

            for (int i = 0; i < characters.Count; i++) {

                for (int j = 0; j < placements.Length; j++) {
                    if (placements[j] == i)
                        place = j;

                }
                GameObject lastInst = Instantiate(playerPlacementEndGame, transform.position, Quaternion.identity, placementPanel.transform.Find("PlayersPlacement").transform);
                lastInst.transform.Find("1/CharSprite").GetComponent<Image>().sprite = gameManager.multiplayerPanel.portraitSprites[gameManager.multiplayerPanel.playerScreens[i].indexPortrait];
                lastInst.transform.SetSiblingIndex(0);
                lastInst.name = "Placement " + characters[i].name;
                AllPanels.Add(lastInst.transform);
                lastInst.transform.Find("1/PlayerName").GetComponent<TextMeshProUGUI>().text = "Player " + (i + 1);
                lastInst.transform.Find("1/PlayerColor").GetComponent<Image>().sprite = playerColorImages[i];
                lastInst.transform.Find("1/PlayerColor/Number").GetComponent<Image>().sprite = playerNumberImages[place];
                Vector3 lastPos = lastInst.transform.Find("1").transform.position;
                lastInst.transform.Find("1").transform.position = new Vector3(lastPos.x + 25 * place, lastPos.y, lastPos.z);
            }

            for (int i = 0; i < AllPanels.Count; i++) {
                for (int j = 0; j < placements.Length; j++) {
                    if (placements[j] == i)
                        place = j;
                }

                MovementController player = characters[i] as MovementController;
                AllPanels[i].transform.SetSiblingIndex(place);
                //Debug.Log(characters[i].name + ".." + characters[i].transform.GetSiblingIndex() + ".." + placements[i] + ".." + AllPanels[i].transform.name);
            }

        }

        #region Setters

        public bool CheckpointReached(int characterId, int index) {
            if (checkpointReached[characterId] == index - 1) {
                checkpointReached[characterId] = index;
                Debug.Log("Checkpoint Reached : " + index + " by " + characters[characterId].name);
                return true;
            }

            return false;
        }

        public void Clean() {
            for (int charIndex = 0; charIndex < checkpointReached.Count; charIndex++) {
                checkpointReached[charIndex] = 0;
            }
        }

        public void SetCharacters(Character[] characters) {
            this.characters.Clear();
            for (int i = 0; i < characters.Length; i++) {
                this.characters.Add(characters[i]);
            }
        }

        public void DoALapse(int characterIndex) {
            checkpointReached[characterIndex] = 0;
            laps[characterIndex]++;
        }

        #endregion

        #region Getters

        public int CharacterId(Character character) {
            if (!characters.Contains(character)) { return -1; }

            return characters.IndexOf(character);
        }

        public bool HasEnded(int characterIndex) {
            if (laps[characterIndex] < lapsNumber) {
                return false;
            }

            return true;
        }

        public bool LapsDone(int characterIndex) {
            return checkpointReached[characterIndex] == checkpoints.Length - 1;
        }

        public void ComputePlacements() {
            // Points = players;
            //int[] pointPlayer = new int[characters.Count];

            for (int i = 0; i < characters.Count; i++) {
                places[i].playerId = i;

                int points = GetPositionPoints(i);
                places[i].points = points;
            }

            bool finish = false;
            while (!finish) {
                finish = true;
                for (int i = 0; i < places.Length - 1; i++) {
                    if (places[i].points < places[i + 1].points) {
                        finish = false;
                        Place temp = places[i + 1];
                        places[i + 1] = places[i];
                        places[i] = temp;
                    }
                }
            }

            for (int i = 0; i < places.Length; i++) {
                placements[i] = places[i].playerId;
            }

            //Dictionary<int, List<int>> points = new Dictionary<int, List<int>>();

            //int maxPoint = 0;

            //for (int i = 0; i < characters.Count; i++) {
            //    int point = GetPositionPoints(i);
            //    if (!points.ContainsKey(point)) {
            //        points.Add(point, new List<int>());
            //    }
            //    points[point].Add(i);
            //    if (point > maxPoint) {
            //        maxPoint = point;
            //    }
            //}

            //int place = 0;

            //for (int i = 0; i < placements.Length; i++) {
            //    if (HasEnded(placements[i])) {
            //        place = i + 1;
            //    }
            //}

            //if (place >= placements.Length) {
            //    return;
            //}

            //for (int i = maxPoint; i >= 0; i--) {
            //    if (points.ContainsKey(i)) {
            //        for (int j = 0; j < points[i].Count; j++) {
            //            if (HasEnded(points[i][j])) {
            //                points[i].RemoveAt(j);
            //            }
            //        }

            //        if (points[i].Count > 1) {
            //            List<float> distances = new List<float>();
            //            Dictionary<float, int> almanac = new Dictionary<float, int>();
            //            for (int j = 0; j < points[i].Count; j++) {
            //                int index = points[i][j];
            //                distances.Add(Vector3.Distance(characters[index].transform.position, checkpoints[checkpointReached[index] + 1]));
            //                almanac.Add(distances[distances.Count - 1], index);
            //            }
            //            distances.Sort();
            //            for (int j = 0; j < distances.Count; j++) {
            //                placements[place] = almanac[distances[j]];
            //                place++;
            //            }
            //        } else if (points[i].Count > 0) {
            //            placements[place] = points[i][0];
            //            place++;
            //        }
            //    }
            //}
        }

        private int GetPositionPoints(int characterIndex) {
            int chunk = 20;

            int points = 0;
            points = laps[characterIndex] * checkpoints.Length;
            points += checkpointReached[characterIndex];

            points *= chunk;
            int prevCheckpointIndex = checkpointReached[characterIndex];
            int nextCheckpointIndex = checkpointReached[characterIndex] + 1;

            if (nextCheckpointIndex >= checkpoints.Length) {
                nextCheckpointIndex = 0;
            }

            float maxDistance = Vector3.Distance(checkpoints[prevCheckpointIndex], checkpoints[nextCheckpointIndex]);
            float distance = Vector3.Distance(characters[characterIndex].transform.position, checkpoints[nextCheckpointIndex]);
            int supplement = chunk - Mathf.RoundToInt((distance / maxDistance) * chunk);
            points += supplement;

            return points;
        }

        #endregion
    }
}
