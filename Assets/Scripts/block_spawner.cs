using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class block_spawner : MonoBehaviour
{
    public GameObject block;
    float time = 0;
    void Update()
    {
        time += Time.deltaTime;
        if(time >= block.transform.lossyScale.x)
        {
            Instantiate(block, gameObject.transform);
            time = 0;
        }
    }
}
