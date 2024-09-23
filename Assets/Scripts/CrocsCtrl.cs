using UnityEngine;
using System.IO;

public class CrocsCtrl : MonoBehaviour
{
    public GameObject crocsPrefab;  // 크록스를 떨어뜨릴 프리팹
    public float averageCompression = 1;
    public GameObject crocs;

    private Escalator escalator;

    float time = 0;

    public int trying = 0;
    public int fail = 0;
    public int success = 0;

    private string filePath; // CSV 파일 경로

    void Start()
    {
        escalator = FindObjectOfType<Escalator>();
        filePath = Application.dataPath + "/success_coordinates.csv"; // 파일 경로를 영구 저장 경로로 설정
        InitializeCSV(); // CSV 파일 초기화
        SpawnCrocs();
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
    }

    void Update()
    {
        time += Time.deltaTime;
        if(trying <= 3000)
        {
            if (crocs.transform.position.y < 0 || crocs.transform.position.x > 20 || crocs.transform.position.x < -20)
            {
                fail += 1;
                Destroy(crocs);
                SpawnCrocs();
                time = 0;
            }

            if (averageCompression >= 0.25 && averageCompression <= 1 && crocs.transform.position.y > escalator.Sigmoid(crocs.transform.position.x) - 1)
            {
                if(time < 20)
                {
                    fail += 1;
                    Destroy(crocs);
                    SpawnCrocs();
                    time = 0;
                }
                else
                {
                    success += 1;
                    SaveSuccessCoordinates(crocs.transform.position.x, crocs.transform.position.y, averageCompression, time); // 성공한 크록스의 x, y 좌표를 CSV에 저장
                    Destroy(crocs);
                    SpawnCrocs();
                    time = 0;
                }
            }
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

    void InitializeCSV()
    {
        // 파일이 이미 존재하면 헤더를 추가하지 않음
        if (!File.Exists(filePath))
        {
            // 파일이 없으면 새 파일을 만들고 헤더를 추가
            File.WriteAllText(filePath, "Success_X_Coordinate,Success_Y_Coordinate,Success_Compression,times\n");
        }
    }


    // 성공한 크록스의 x, y 좌표를 CSV에 기록하는 함수
    void SaveSuccessCoordinates(float xCoordinate, float yCoordinate, float averageCompression, float time)
    {
        string data = xCoordinate.ToString() + "," + yCoordinate.ToString() + "," + averageCompression.ToString() + "," + time.ToString() + "\n";
        File.AppendAllText(filePath, data);
    }
}
