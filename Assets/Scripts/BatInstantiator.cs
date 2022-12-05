using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code from RafaelKuebler on GitHub: https://github.com/RafaelKuebler/Flocking
public class BatInstantiator : MonoBehaviour
{
    public GameObject bat;

    [Range(0, 300)]
    public int number;

    private void Start()
    {
        for (int i = 0; i < number; i++)
        {
            Instantiate(bat, Vector3.zero, Quaternion.identity);
        }
    }
}