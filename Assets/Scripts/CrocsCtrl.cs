using UnityEngine;

public class CrocsCtrl : MonoBehaviour
{
    public GameObject crocsPrefab;  // 크록스를 떨어뜨릴 프리팹
    public float averageCompression = 1;
    public GameObject crocs;

    private Escalator escalator;

    public int trying = 0;
    public int fail = 0;
    public int success = 0;

    void Start()
    {
        escalator = FindObjectOfType<Escalator>();
        SpawnCrocs();
    }

    void Update()
    {
        if(crocs.transform.position.y < 0 || crocs.transform.position.x > 20 || crocs.transform.position.x < -20)
        {
            fail += 1;
            Destroy(crocs);
            SpawnCrocs();
        }

        if(averageCompression >= 0.3)
        {
            success += 1;
            Destroy(crocs);
            SpawnCrocs();
        }
    }

    void SpawnCrocs()
    {
        trying += 1;
        float i = Random.Range(-19, 19);
        // 크록스를 무작위로 스폰할 위치를 결정하고 생성
        Vector3 spawnPosition = new Vector3(i, escalator.Sigmoid(i) + 1, Random.Range(-0.2f, 0.2f));
        crocs = Instantiate(crocsPrefab, spawnPosition, Quaternion.identity);
        Debug.DrawRay(spawnPosition, Vector3.forward, Color.green);
    }
}
