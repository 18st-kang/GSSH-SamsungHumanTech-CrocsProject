using UnityEngine;

public class ElasticBody : MonoBehaviour
{
    public GameObject massPointPrefab; // 질량점으로 사용할 프리팹
    public int size = 10;              // 3D 격자의 크기
    public float spacing = 1.0f;       // 질량점 사이의 간격 (모서리 스프링의 자연 길이로 사용)
    public float springConstant = 10.0f; // 스프링 상수
    public float damping = 1.0f;       // 감쇠 계수

    private float axisRestLength;         // 축 방향 스프링의 자연 길이
    private float diagonal2DRestLength;   // 2D 대각선 스프링의 자연 길이
    private float diagonal3DRestLength;   // 3D 대각선 스프링의 자연 길이

    public GameObject[,,] massPoints;     // 3D 배열로 질량점 저장
    private Vector3 lastParentPosition;   // 부모 오브젝트의 이전 위치

    void Start()
    {
        // 자연 길이 설정 (spacing을 모서리 길이로 사용)
        axisRestLength = spacing;                    // 축 방향 자연 길이
        diagonal2DRestLength = spacing * Mathf.Sqrt(2.0f); // 2D 대각선 자연 길이
        diagonal3DRestLength = spacing * Mathf.Sqrt(3.0f); // 3D 대각선 자연 길이

        CreateMassPoints(); // 질량점 생성
        lastParentPosition = transform.position; // 부모 오브젝트의 초기 위치 저장
        UpdateParentPosition(); // 부모 오브젝트의 위치를 초기화
    }

    void Update()
    {
        ApplyForces(); // 각 프레임마다 힘 적용
        UpdateParentPosition(); // 부모 오브젝트의 중심점을 지속적으로 업데이트
        AdjustChildPositions(); // 자식 오브젝트의 위치 보정
    }

    // 질량점을 생성하는 함수
    void CreateMassPoints()
    {
        massPoints = new GameObject[size, size, size];

        Vector3 startPos = transform.position - new Vector3((size - 1) * spacing / 2, (size - 1) * spacing / 2, (size - 1) * spacing / 2);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 position = startPos + new Vector3(x * spacing, y * spacing, z * spacing);
                    GameObject massPoint = Instantiate(massPointPrefab, position, Quaternion.identity, transform);
                    massPoints[x, y, z] = massPoint; // 부모 오브젝트의 자식으로 설정하지 않음
                }
            }
        }
    }

    // 부모 오브젝트의 위치를 질량점들의 평균 위치로 업데이트
    void UpdateParentPosition()
    {
        Vector3 averagePosition = Vector3.zero;
        int count = 0;

        foreach (var point in massPoints)
        {
            if (point != null)
            {
                averagePosition += point.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            averagePosition /= count;
        }

        transform.position = averagePosition; // 부모 오브젝트 위치를 질량점들의 중심으로 이동
    }

    // 부모 오브젝트가 이동한 만큼 자식 오브젝트들의 위치를 보정하는 함수
    void AdjustChildPositions()
    {
        Vector3 parentMovement = transform.position - lastParentPosition; // 부모 오브젝트의 이동량 계산
        foreach (var point in massPoints)
        {
            if (point != null)
            {
                point.transform.position -= parentMovement; // 자식 오브젝트를 부모의 이동 반대 방향으로 이동
            }
        }
        lastParentPosition = transform.position; // 부모 오브젝트의 현재 위치를 저장
    }

    // 각 질량점에 대해 스프링의 힘을 적용하는 함수
    void ApplyForces()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    GameObject currentPoint = massPoints[x, y, z];

                    // 축 방향 적용
                    ApplySpringForce(currentPoint, x + 1, y, z, axisRestLength); // 오른쪽
                    ApplySpringForce(currentPoint, x - 1, y, z, axisRestLength); // 왼쪽
                    ApplySpringForce(currentPoint, x, y + 1, z, axisRestLength); // 위
                    ApplySpringForce(currentPoint, x, y - 1, z, axisRestLength); // 아래
                    ApplySpringForce(currentPoint, x, y, z + 1, axisRestLength); // 뒤
                    ApplySpringForce(currentPoint, x, y, z - 1, axisRestLength); // 앞

                    // 2D 대각선 방향
                    ApplySpringForce(currentPoint, x + 1, y + 1, z, diagonal2DRestLength); // 위 오른쪽
                    ApplySpringForce(currentPoint, x - 1, y - 1, z, diagonal2DRestLength); // 아래 왼쪽
                    ApplySpringForce(currentPoint, x + 1, y, z + 1, diagonal2DRestLength); // 뒤 오른쪽
                    ApplySpringForce(currentPoint, x - 1, y, z - 1, diagonal2DRestLength); // 앞 왼쪽
                    ApplySpringForce(currentPoint, x + 1, y - 1, z, diagonal2DRestLength); // 아래 오른쪽
                    ApplySpringForce(currentPoint, x - 1, y + 1, z, diagonal2DRestLength); // 위 왼쪽
                    ApplySpringForce(currentPoint, x, y + 1, z + 1, diagonal2DRestLength); // 위 뒤
                    ApplySpringForce(currentPoint, x, y - 1, z - 1, diagonal2DRestLength); // 아래 앞
                    ApplySpringForce(currentPoint, x, y + 1, z - 1, diagonal2DRestLength); // 위 앞
                    ApplySpringForce(currentPoint, x, y - 1, z + 1, diagonal2DRestLength); // 아래 뒤

                    // 3D 대각선 방향
                    ApplySpringForce(currentPoint, x + 1, y + 1, z + 1, diagonal3DRestLength); // 대각선 위 오른쪽 뒤
                    ApplySpringForce(currentPoint, x - 1, y - 1, z - 1, diagonal3DRestLength); // 대각선 아래 왼쪽 앞
                    ApplySpringForce(currentPoint, x + 1, y + 1, z - 1, diagonal3DRestLength); // 대각선 위 오른쪽 앞
                    ApplySpringForce(currentPoint, x - 1, y - 1, z + 1, diagonal3DRestLength); // 대각선 아래 왼쪽 뒤
                    ApplySpringForce(currentPoint, x + 1, y - 1, z + 1, diagonal3DRestLength); // 대각선 위 오른쪽 뒤
                    ApplySpringForce(currentPoint, x - 1, y + 1, z - 1, diagonal3DRestLength); // 대각선 아래 왼쪽 앞
                }
            }
        }
    }

    // 두 질량점 간 스프링 힘을 적용하는 함수
    void ApplySpringForce(GameObject point, int x, int y, int z, float restLength)
    {
        if (x < 0 || x >= size || y < 0 || y >= size || z < 0 || z >= size)
        {
            return; // 배열 범위를 벗어나는 경우 무시합니다.
        }

        GameObject neighborPoint = massPoints[x, y, z];
        if (neighborPoint == null)
        {
            return; // 이웃 점이 없는 경우 무시합니다.
        }

        Rigidbody pointRb = point.GetComponent<Rigidbody>();
        Rigidbody neighborRb = neighborPoint.GetComponent<Rigidbody>();

        // 월드 좌표계 기준으로 거리와 방향을 계산
        Vector3 direction = neighborPoint.transform.position - point.transform.position;
        float distance = direction.magnitude;

        // 후크의 법칙에 따라 스프링의 힘 계산
        float forceMagnitude = springConstant * (distance - restLength); // 후크의 법칙
        Vector3 force = direction.normalized * forceMagnitude;

        // 힘 적용
        pointRb.AddForce(force);
        neighborRb.AddForce(-force);

        // 디버그용 레이 표시: 힘의 크기에 따라 색상 변화
        Color color = Color.Lerp(Color.green, Color.red, Mathf.Abs(forceMagnitude) / 10.0f);
        Debug.DrawRay(point.transform.position, direction, color, Time.deltaTime);
    }
}
