using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class liveSettings : MonoBehaviour
{
    public float time = 1f;

    void Update()
    {
        Time.timeScale = time;
    }
}
