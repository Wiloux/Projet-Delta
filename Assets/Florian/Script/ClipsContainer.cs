using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ClipsContainer : MonoBehaviour
{
    public List<AudioClip> AllClips;
    public static ClipsContainer Instance;

    private void Awake()
    {
        Instance = this;
    }
}
