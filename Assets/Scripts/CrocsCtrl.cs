using UnityEngine;

public class CrocsCtrl : MonoBehaviour
{
    public GameObject crocsPrefab;  // 크록스를 떨어뜨릴 프리팹
    public float averageCompression = 1;
    public GameObject crocs;

    void Start()
    {
        SpawnCrocs();
    }

    void Update()
    {
        Debug.Log("현재 크록스의 평균 압축율: " + averageCompression);
    }

    void SpawnCrocs()
    {
        // 크록스를 무작위로 스폰할 위치를 결정하고 생성
        Vector3 spawnPosition = new Vector3(0, 10, Random.Range(-0.6f, 0.6f));
        crocs = Instantiate(crocsPrefab);
        crocs.transform.position = spawnPosition;
    }
}
