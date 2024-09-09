using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class escalator : MonoBehaviour
{
    Vector3 first_pos = new Vector3(-8.5f, 0, 0);
    float x_pos = 0;
    void Update()
    {
        float yfunction = -5 / (1 + Mathf.Pow(100, (x_pos - 8) / 8)) + 5;
        transform.position = first_pos + new Vector3(x_pos, yfunction, 0);

        x_pos += 1 * Time.deltaTime;

        if(x_pos >= 17)
        {
            Destroy(gameObject);
        }
    }
}
