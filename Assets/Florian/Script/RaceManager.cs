using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian {
    public class Character : MonoBehaviour {

    }

    public class RaceManager : MonoBehaviour {
        public int lapsNumber = 0;
        public Vector3[] checkpoints = null;
        public List<Character> characters;

        private List<bool[]> checkpointReached = null;
        private List<int> laps = null;
        public bool raceStarted = false;

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

        private void Update() {
            UpdateRace();
        }

        private void UpdateRace() {
            for (int i = 0; i < NumberOfCharacters; i++) {
                if (LapsDone(i)) {
                    DoALapse(i);
                    if (HasEnded(i)) {
                        if (EveryoneEnded) {
                            EndRace();
                        }
                    }
                }
            }
        }

        public void StartRace() {
            laps = new List<int>();
            checkpointReached = new List<bool[]>();
            for (int i = 0; i < NumberOfCharacters; i++) {
                laps.Add(0);
                checkpointReached.Add(new bool[NumberOfCheckpoints]);
                for (int j = 0; j < checkpointReached[i].Length; j++) {
                    checkpointReached[i][j] = false;
                }
            }

            TeleportCharacters(checkpoints[0], characters.ToArray());

            raceStarted = true;
        }

        public void TeleportCharacters(Vector3 position, Character character) {
            character.transform.position = position;
        }

        public void TeleportCharacters(Vector3 position, Character[] characters) {
            for (int i = 0; i < characters.Length; i++) {
                TeleportCharacters(position, characters[i]);
            }
        }

        public void EndRace() {
            // Fin.
        }

        #region Setters

        public void CheckpointReached(Character character, int index) {

        }

        public void Clean() {

        }

        public void SetCharacters(Character[] characters) {
            this.characters.Clear();
            for (int i = 0; i < characters.Length; i++) {
                this.characters.Add(characters[i]);
            }
        }

        public void DoALapse(int characterIndex) {
            for (int i = 0; i < NumberOfCheckpoints; i++) {
                checkpointReached[characterIndex][i] = false;
            }
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
            return checkpointReached[characterIndex][NumberOfCheckpoints - 1];
        }

        #endregion
    }
}