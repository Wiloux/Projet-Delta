using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour {
    public void PlayStepSound(float volume) {
        AudioManager.Instance.PlaySFX(ClipsContainer.Instance.AllClips[10], 0.3f);
    }
}
