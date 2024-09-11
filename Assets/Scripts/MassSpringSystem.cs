using UnityEngine;

public class MassSprintingSystem : MonoBehaviour
{
    public GameObject massPointPrefab; // 질량점으로 사용할 프리팹
    public int size = 3;               // 3D 격자의 크기 (size x size x size)
    public float spacing = 1.0f;       // 질량점 사이의 간격
    public float springForce = 50.0f;  // 스프링의 강도
    public float damper = 5.0f;        // 감쇠 효과
    public float repelFactor = 20.0f;  // 압축 시 밀어내는 힘의 계수

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

                    // Rigidbody 컴포넌트 추가
                    Rigidbody rb = massPoint.AddComponent<Rigidbody>();
                    rb.mass = 1.0f;      // 질량 설정
                    rb.drag = 0.5f;      // 이동 저항 설정
                    rb.angularDrag = 0.5f; // 회전 저항 설정
                    rb.freezeRotation = false; // 회전을 고정하지 않음

                    massPoints[x, y, z] = massPoint;

                    // 각 질량점을 기준으로 스프링 연결
                    ConnectToAllPossiblePoints(x, y, z);
                }
            }
        }
    }

    // 인접한 질량점들과 모든 가능한 방향으로 스프링으로 연결하는 함수
    void ConnectToAllPossiblePoints(int x, int y, int z)
    {
        GameObject current = massPoints[x, y, z];
        if (current == null)
        {
            Debug.LogError("Current mass point is null at position: (" + x + ", " + y + ", " + z + ")");
            return;
        }

        // 모서리(기본 축 방향) 연결
        if (x > 0) AddSpringJoint(current, massPoints[x - 1, y, z], spacing); // X 방향
        if (y > 0) AddSpringJoint(current, massPoints[x, y - 1, z], spacing); // Y 방향
        if (z > 0) AddSpringJoint(current, massPoints[x, y, z - 1], spacing); // Z 방향

        // 평면 내 2D 대각선 연결 (길이: √2 * spacing)
        if (x > 0 && y > 0) AddSpringJoint(current, massPoints[x - 1, y - 1, z], Mathf.Sqrt(2) * spacing); // XY 평면 대각선
        if (x > 0 && z > 0) AddSpringJoint(current, massPoints[x - 1, y, z - 1], Mathf.Sqrt(2) * spacing); // XZ 평면 대각선
        if (y > 0 && z > 0) AddSpringJoint(current, massPoints[x, y - 1, z - 1], Mathf.Sqrt(2) * spacing); // YZ 평면 대각선

        // 3D 대각선 연결 (길이: √3 * spacing)
        if (x > 0 && y > 0 && z > 0) AddSpringJoint(current, massPoints[x - 1, y - 1, z - 1], Mathf.Sqrt(3) * spacing); // 3D 대각선

        // 반대 방향 3D 대각선 연결 (길이: √3 * spacing), 격자의 끝을 넘어가는 경우는 제외
        if (x < size - 1 && y > 0 && z > 0 && massPoints[x + 1, y - 1, z - 1] != null) 
            AddSpringJoint(current, massPoints[x + 1, y - 1, z - 1], Mathf.Sqrt(3) * spacing);
        if (x > 0 && y < size - 1 && z > 0 && massPoints[x - 1, y + 1, z - 1] != null) 
            AddSpringJoint(current, massPoints[x - 1, y + 1, z - 1], Mathf.Sqrt(3) * spacing);
        if (x > 0 && y > 0 && z < size - 1 && massPoints[x - 1, y - 1, z + 1] != null) 
            AddSpringJoint(current, massPoints[x - 1, y - 1, z + 1], Mathf.Sqrt(3) * spacing);
    }

    // 두 질량점 사이에 스프링 조인트를 추가하는 함수
    void AddSpringJoint(GameObject pointA, GameObject pointB, float naturalLength)
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Null reference when adding Spring Joint between points.");
            return;
        }

        Rigidbody rbA = pointA.GetComponent<Rigidbody>();
        Rigidbody rbB = pointB.GetComponent<Rigidbody>();

        if (rbA == null || rbB == null)
        {
            Debug.LogError("Rigidbody is missing from one of the mass points.");
            return;
        }

        SpringJoint springJoint = pointA.AddComponent<SpringJoint>();
        springJoint.connectedBody = rbB; // 연결된 Rigidbody 설정
        springJoint.spring = springForce;      // 동일한 탄성계수 적용
        springJoint.damper = damper;          // 감쇠 효과
        springJoint.minDistance = 0.0f;       // 최소 거리
        springJoint.maxDistance = naturalLength; // 스프링의 자연 길이
        springJoint.autoConfigureConnectedAnchor = true; // 자동으로 연결 위치 설정
        springJoint.anchor = Vector3.zero;
        springJoint.connectedAnchor = Vector3.zero;
    }

    void FixedUpdate()
    {
        // 모든 질량점 쌍에 대해 양방향 복원력 및 밀어내는 힘 추가 적용
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    ApplyBidirectionalForce(x, y, z);
                }
            }
        }
    }

    // 조인트의 자연 길이보다 길거나 짧을 때 복원력 및 밀어내는 힘 적용
    void ApplyBidirectionalForce(int x, int y, int z)
    {
        GameObject current = massPoints[x, y, z];
        if (current == null) return;

        Rigidbody rbA = current.GetComponent<Rigidbody>();
        if (rbA == null) return;

        // 연결된 모든 질량점에 대해 복원력과 밀어내는 힘 적용
        foreach (SpringJoint joint in current.GetComponents<SpringJoint>())
        {
            if (joint == null || joint.connectedBody == null) continue;

            Rigidbody rbB = joint.connectedBody;
            Vector3 direction = rbB.position - rbA.position;
            float currentDistance = direction.magnitude;

            if (currentDistance < joint.maxDistance) // 압축된 경우
            {
                direction.Normalize();
                float forceMagnitude = repelFactor * (joint.maxDistance - currentDistance); // 밀어내는 힘 크기
                Vector3 repelForceVector = forceMagnitude * direction;

                // 두 질량점에 반대 방향으로 밀어내는 힘 적용
                rbA.AddForce(-repelForceVector);
                rbB.AddForce(repelForceVector);
            }
            else if (currentDistance > joint.maxDistance) // 늘어난 경우
            {
                direction.Normalize();
                float forceMagnitude = springForce * (currentDistance - joint.maxDistance); // 당기는 힘 크기
                Vector3 pullForceVector = forceMagnitude * direction;

                // 두 질량점에 반대 방향으로 당기는 힘 적용
                rbA.AddForce(pullForceVector);
                rbB.AddForce(-pullForceVector);
            }
        }
    }
}
