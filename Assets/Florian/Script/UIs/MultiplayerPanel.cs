using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerPanel : MonoBehaviour {
    [Serializable]
    public struct PlayerScreen {
        public Image portrait;
        [HideInInspector] public int indexPortrait;
        public Transform arrowsParent;
    }

    public Sprite[] portraitSprites;
    public PlayerScreen[] playerScreens;

    public void ChangePortraitSprite(int index, int delta) {
        PlayerScreen actual = playerScreens[index];
        actual.indexPortrait += delta;
        actual.indexPortrait %= portraitSprites.Length;
        actual.indexPortrait = actual.indexPortrait < 0 ? portraitSprites.Length - 1 : actual.indexPortrait;

        ChangePortraitSprite(index, portraitSprites[actual.indexPortrait]);
        playerScreens[index] = actual;
    }

    public void ChangePortraitSprite(int index, Sprite sprite) {
        playerScreens[index].portrait.sprite = sprite;
    }
}
