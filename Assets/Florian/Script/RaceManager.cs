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
        public int lapsNumber = 0;
        public Vector3[] checkpoints = null;
        [HideInInspector] public List<Character> characters;

        [SerializeField] private List<int> checkpointReached = null;
        [SerializeField] private List<int> laps = null;
        public int[] placements = null;
        public bool raceStarted = false;

        [Header("EndGame")]
        public GameObject playerPlacementEndGame;
        public GameObject placementPanel;

        [Header("Checkpoints")]
        public Vector3 checkpointsSize = Vector3.zero;



        public GameManager gameManager;

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
                action.SetAction(Action.WaitType.NONE, ActionGameObject.TargetType.TRIGGERING, this, i);
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
                        player.lockMovements = true;
                        if (EveryoneEnded) {
                            EndRace();
                        }
                    }
                }
            }
        }

        public void StartRace(Character[] characters) {
            SetCharacters(characters);

            placements = new int[characters.Length];

            laps = new List<int>();
            checkpointReached = new List<int>();
            for (int i = 0; i < characters.Length; i++) {
                Debug.Log(characters[i].name);
                laps.Add(0);
                checkpointReached.Add(0);
                placements[i] = 0;
                (characters[i] as MovementController).maxLaps = lapsNumber;
                (characters[i] as MovementController).Laps = 0;
            }

            TeleportCharacters(checkpoints[0], new Vector3(0,180f,0));

            raceStarted = true;
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
            Debug.Log(character.transform.position + " .. " + position);
        }

        public void TeleportCharacters(Vector3 position, Vector3 orientation) {
            for (int i = 0; i < characters.Count; i++) {
                TeleportCharacters(position + new Vector3(0, 0, 2f) * i, orientation, characters[i]);
            }
        }

        public void EndRace() {
            placementPanel.SetActive(true);

            List<Transform> AllPanels = new List<Transform>();

            for (int i = 0; i < characters.Count; i++) {
                GameObject lastInst = Instantiate(playerPlacementEndGame, transform.position, Quaternion.identity, placementPanel.transform.Find("PlayersPlacement").transform);
                //  Debug.Log(gameManager.multiplayerPanel.portraitSprites[gameManager.multiplayerPanel.playerScreens[i].indexPortrait];
                lastInst.transform.Find("CharSprite").GetComponent<Image>().sprite = gameManager.multiplayerPanel.portraitSprites[gameManager.multiplayerPanel.playerScreens[i].indexPortrait];
                //     AllPanels.Add(lastInst.transform);
                //     AllPanels.Sort(characters[i].transform);
                //     Movement player = characters[i] as Movement;
                lastInst.transform.SetSiblingIndex(0);
                lastInst.name = "Placement" + characters[i].name;
                AllPanels.Add(lastInst.transform);
                lastInst.transform.Find("CharSprite/Placement").GetComponent<TextMeshProUGUI>().text = placements[i] + 1 + "";
            }

            for (int i = 0; i < AllPanels.Count; i++) {
                AllPanels[i].transform.SetSiblingIndex(placements[i]);
                Debug.Log(characters[i].name + ".." + characters[i].transform.GetSiblingIndex() + ".." + placements[i] + ".." + AllPanels[i].transform.name);


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
            Dictionary<int, List<int>> points = new Dictionary<int, List<int>>();

            int maxPoint = 0;

            for (int i = 0; i < characters.Count; i++) {
                int point = GetPositionPoints(i);
                if (!points.ContainsKey(point)) {
                    points.Add(point, new List<int>());
                }
                points[point].Add(i);
                if (point > maxPoint) {
                    maxPoint = point;
                }
            }

            int place = 0;
            for (int i = maxPoint; i >= 0; i--) {
                if (points.ContainsKey(i)) {
                    if (points[i].Count > 1) {
                        List<float> distances = new List<float>();
                        Dictionary<float, int> almanac = new Dictionary<float, int>();
                        for (int j = 0; j < points[i].Count; j++) {
                            int index = points[i][j];
                            distances.Add(Vector3.Distance(characters[index].transform.position, checkpoints[checkpointReached[index] + 1]));
                            almanac.Add(distances[distances.Count - 1], index);
                            //Debug.LogWarning("Player : " + characters[index].name + " // " + distances[distances.Count - 1]);
                        }
                        distances.Sort();
                        for (int j = 0; j < distances.Count; j++) {
                            //Debug.LogWarning("/// " + distances[j] + " .. " + characters[almanac[distances[j]]].name + " .. " + almanac[distances[j]]);
                            placements[place] = almanac[distances[j]];
                            place++;
                        }
                    } else {
                        placements[place] = points[i][0];
                        place++;
                    }
                }
            }
            //  Debug.Log("----------------------------");
        }

        private int GetPositionPoints(int characterIndex) {
            int points = 0;
            points = laps[characterIndex] * checkpoints.Length;
            points += checkpointReached[characterIndex];
            return points;
        }

        #endregion
    }
}
