using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    public List<Transform> exits = new List<Transform>();

    public List<string> playerNames = new List<string>();

    public bool original;

    //Gizmos
    private BoxCollider col;
    private Transform child;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
        child = GetComponentInChildren<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && exits.Count > 0 && original)
        {
            
            int index = Random.Range(0, exits.Count);
            foreach (Transform exit in exits)
            {
                if (index != exits.IndexOf(exit))
                {
                    exit.parent.GetComponent<Teleporter>().playerNames.Add(other.transform.name);
                    break;
                }
            }

            other.transform.position = exits[index].position;
            other.transform.rotation = exits[index].rotation;
        }
        else if (!original && other.CompareTag("Player") && exits.Count > 0)
        {
            for (int i = 0; i < playerNames.Count; i++)
            {
                string n = playerNames[i];
                if (other.transform.name == n)
                {
                    int index = Random.Range(0, exits.Count);
                    if (exits[index].parent.GetComponent<Teleporter>().exits.Count != 0)
                    {
                        foreach (Transform exit in exits)
                        {
                            if (index != exits.IndexOf(exit))
                            {
                                exit.parent.GetComponent<Teleporter>().playerNames.Add(other.transform.name);
                                break;
                            }
                        }
                    }


                    playerNames.Remove(n);
                    other.transform.position = exits[index].position;
                    other.transform.rotation = exits[index].rotation;
                }

            }

        }
    }
}
