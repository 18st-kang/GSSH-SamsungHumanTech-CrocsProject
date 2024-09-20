using UnityEngine;

public class CrocsCtrl : MonoBehaviour
{
    public GameObject crocsPrefab;  // 크록스를 떨어뜨릴 프리팹
    public Transform spawnPoint;    // 크록스를 스폰할 위치
    public float spawnInterval = 2.0f; // 크록스를 생성할 간격
    private ElasticBodyPhysics elasticBodyPhysics; // ElasticBodyPhysics 컴포넌트 참조

    private float timeSinceLastSpawn = 0.0f; // 마지막으로 크록스를 생성한 시간

    void Start()
    {
        // ElasticBodyPhysics 컴포넌트를 가져옵니다.
        elasticBodyPhysics = FindObjectOfType<ElasticBodyPhysics>();
        if (elasticBodyPhysics == null)
        {
            Debug.LogError("ElasticBodyPhysics 컴포넌트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        // 일정 시간 간격으로 크록스를 생성
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnCrocs();
            timeSinceLastSpawn = 0.0f;
        }

        // ElasticBodyPhysics의 압축율 계산 함수 호출
        if (elasticBodyPhysics != null)
        {
            float averageCompression = elasticBodyPhysics.CalculateAverageCompression();
            Debug.Log("현재 크록스의 평균 압축율: " + averageCompression);
        }
    }

    void SpawnCrocs()
    {
        // 크록스를 무작위로 스폰할 위치를 결정하고 생성
        Vector3 spawnPosition = spawnPoint.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, 0);
        Instantiate(crocsPrefab, spawnPosition, Quaternion.identity);
    }
}
