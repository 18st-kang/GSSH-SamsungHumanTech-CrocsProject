using UnityEngine;

public class Escalator : MonoBehaviour
{
    public GameObject blockPrefab;  // 블럭으로 사용할 3D 큐브 프리팹
    public GameObject wallPrefab;
    public int blockAmount = 10;    // 블럭의 개수
    public float blockScale = 1.0f; // 각 블럭의 크기
    public float blockInterval = 1.0f;  // 블럭 사이의 간격
    public float blockWidth = 1.6f; // 블럭의 z축 폭 (진행방향의 폭)

    public float speed = 1.0f;      // 에스컬레이터 속도
    public float angle = 30.0f;     // 시그모이드 함수의 경사각 (60분법으로 입력)
    public float height = 5.0f;     // 시그모이드 함수의 높이

    public float wallHeight = 10.0f; // 벽의 높이
    public float wallThickness = 0.1f; // 벽의 두께
    public float wallMargin = 0.01f;

    private float slope;            // 시그모이드 함수의 기울기
    private GameObject[] blocks;    // 블럭들을 저장할 배열
    private Rigidbody[] blockRigidbodies; // 각 블럭에 대한 Rigidbody 배열
    private float[] xPositions;     // 각 블럭의 x축 좌표값을 저장할 배열
    private float totalLength;      // 전체 에스컬레이터의 길이 (블럭과 간격 포함)

    void Start()
    {
        blocks = new GameObject[blockAmount];
        blockRigidbodies = new Rigidbody[blockAmount];
        xPositions = new float[blockAmount];

        // 각도를 라디안으로 변환하고, 그에 따른 slope 값 계산
        float radianAngle = Mathf.Deg2Rad * angle;
        slope = 4 * Mathf.Tan(radianAngle) / height;

        // 전체 길이 계산 (블럭의 크기와 간격 포함)
        totalLength = blockAmount * (blockScale + blockInterval);

        // 블럭을 생성하고 초기 위치 설정
        for (int i = 0; i < blockAmount; i++)
        {
            blocks[i] = Instantiate(blockPrefab, transform);
            blocks[i].transform.localScale = new Vector3(blockScale, wallHeight * slope * blockScale / 4, blockWidth);  // z축 크기 설정
            
            // Rigidbody를 추가하고 속도를 설정
            Rigidbody rb = blocks[i].GetComponent<Rigidbody>();
            rb.isKinematic = true; // 충돌하지 않게 설정
            rb.useGravity = false;
            blockRigidbodies[i] = rb;

            // 초기 x 위치를 블럭 간격과 스케일에 따라 설정
            xPositions[i] = i * (blockScale + blockInterval);
        }

        // 벽을 생성하고 위치 설정
        CreateWalls();
    }

    void FixedUpdate()
    {
        for (int i = 0; i < blockAmount; i++)
        {
            // 블럭의 x 위치는 시간과 speed에 따라 이동
            xPositions[i] += speed * Time.deltaTime;
            
            // 블럭의 x 위치가 전체 길이를 넘으면 다시 0으로 순환
            if (xPositions[i] > totalLength)
            {
                xPositions[i] -= totalLength;
            }

            // 시그모이드 함수를 기반으로 y 위치 설정
            float yPosition = Sigmoid(xPositions[i]);

            // 블럭 위치를 부모 오브젝트의 0,0을 기준으로 설정
            Vector3 localPosition = new Vector3(xPositions[i] - totalLength / 2, yPosition, 0);
            blockRigidbodies[i].MovePosition(transform.TransformPoint(localPosition));  // 부모 오브젝트의 상대 좌표에서 절대 좌표로 변환
        }
    }

    // 시그모이드 함수
    float Sigmoid(float x)
    {
        // 중앙값을 기준으로 x값이 변할 수 있도록 조정
        float center = totalLength / 2;
        return height / (1.0f + Mathf.Exp(-slope * (x - center)));
    }

    // 벽을 생성하는 함수
    void CreateWalls()
    {
        // 왼쪽 벽 생성 및 설정 (blockPrefab을 사용)
        GameObject leftWall = Instantiate(blockPrefab, transform);
        leftWall.transform.localScale = new Vector3(totalLength, wallHeight, wallThickness); // x축 길이 설정, z축 두께 설정
        leftWall.transform.localPosition = new Vector3(0, wallHeight / 2, -blockWidth / 2 - wallThickness / 2 - wallMargin); // x축 중심에 배치

        // 오른쪽 벽 생성 및 설정 (blockPrefab을 사용)
        GameObject rightWall = Instantiate(wallPrefab, transform);
        rightWall.transform.localScale = new Vector3(totalLength, wallHeight, wallThickness); // x축 길이 설정, z축 두께 설정
        rightWall.transform.localPosition = new Vector3(0, wallHeight / 2, blockWidth / 2 + wallThickness / 2 + wallMargin); // x축 중심에 배치
    }
}
