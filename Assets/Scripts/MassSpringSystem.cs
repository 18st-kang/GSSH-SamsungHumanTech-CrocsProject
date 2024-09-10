using UnityEngine;

public class MassSpringSystem : MonoBehaviour
{
    public GameObject massPointPrefab; // 질량점으로 사용할 프리팹
    public int size = 3;               // 3D 격자의 크기 (ex: 3x3x3)
    public float spacing = 1.0f;       // 질량점 사이의 간격
    public float springForce = 50.0f;  // 스프링의 강도
    public float damper = 5.0f;        // 감쇠 효과

    private GameObject[,,] massPoints; // 3D 배열로 질량점 저장

    void Start()
    {
        // 3D 배열 초기화
        massPoints = new GameObject[size, size, size];

        // 질량점 생성 및 배열에 저장
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 position = transform.position + new Vector3(x * spacing, y * spacing, z * spacing);
                    GameObject massPoint = Instantiate(massPointPrefab, position, Quaternion.identity);
                    massPoint.AddComponent<Rigidbody>();
                    massPoints[x, y, z] = massPoint;

                    // 인접한 질량점들에 스프링 연결
                    ConnectToAdjacentPoints(x, y, z);
                }
            }
        }
    }

    // 인접한 질량점들과 스프링으로 연결하는 함수
    void ConnectToAdjacentPoints(int x, int y, int z)
    {
        GameObject current = massPoints[x, y, z];

        // x, y, z 방향으로 인접한 질량점 연결
        if (x > 0) AddSpringJoint(current, massPoints[x - 1, y, z], spacing); // X 방향
        if (y > 0) AddSpringJoint(current, massPoints[x, y - 1, z], spacing); // Y 방향
        if (z > 0) AddSpringJoint(current, massPoints[x, y, z - 1], spacing); // Z 방향

        // 대각선 연결: 내부 정육면체의 대각선 방향
        if (x > 0 && y > 0) AddSpringJoint(current, massPoints[x - 1, y - 1, z], Mathf.Sqrt(2) * spacing); // XY 평면 대각선
        if (x > 0 && z > 0) AddSpringJoint(current, massPoints[x - 1, y, z - 1], Mathf.Sqrt(2) * spacing); // XZ 평면 대각선
        if (y > 0 && z > 0) AddSpringJoint(current, massPoints[x, y - 1, z - 1], Mathf.Sqrt(2) * spacing); // YZ 평면 대각선
        if (x > 0 && y > 0 && z > 0) AddSpringJoint(current, massPoints[x - 1, y - 1, z - 1], Mathf.Sqrt(3) * spacing); // 3D 대각선
    }

    // 두 질량점 사이에 스프링 조인트를 추가하는 함수
    void AddSpringJoint(GameObject pointA, GameObject pointB, float naturalLength)
    {
        SpringJoint springJoint = pointA.AddComponent<SpringJoint>();
        springJoint.connectedBody = pointB.GetComponent<Rigidbody>();
        springJoint.spring = springForce;      // 스프링 강도
        springJoint.damper = damper;          // 감쇠 효과
        springJoint.minDistance = 0.1f;       // 최소 거리
        springJoint.maxDistance = naturalLength; // 스프링의 자연 길이
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.anchor = Vector3.zero;
        springJoint.connectedAnchor = Vector3.zero;
    }
}
