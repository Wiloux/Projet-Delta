using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using ToolsBoxEngine;

namespace Florian {
    public class CharacterController : MonoBehaviour {
        private Rewired.Player player;

        private void Start() {
            player = ReInput.players.GetPlayer("Player1");
        }

        private void Update() {
            if (player.GetButtonDown("Jump")) {

            }
        }

    }
}