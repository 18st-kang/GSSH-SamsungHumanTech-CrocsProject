using UnityEngine;
using System.Collections.Generic;

public class ElasticBodyMesh : MonoBehaviour
{
    private GameObject[,,] massPoints;  // 3D 배열로 질량점 저장
    private List<Vector3> vertices = new List<Vector3>(); // 메쉬 정점들
    private List<int> triangles = new List<int>();        // 메쉬 삼각형 인덱스
    private Mesh surfaceMesh;                             // 메쉬 객체
    public Material surfaceMaterial;                      // 겉면 렌더링에 사용할 재질

    void Start()
    {
        surfaceMesh = new Mesh(); // 메쉬 객체 생성
    }

    // 물리 구현 코드로부터 질량점 배열을 설정하는 함수
    public void SetMassPoints(GameObject[,,] massPointsArray)
    {
        massPoints = massPointsArray;
        GenerateSurfaceMesh(); // 질량점 배열이 설정되면 메쉬 생성
    }

    // Update는 필요할 경우 메쉬를 매 프레임 업데이트
    void Update()
    {
        if (massPoints == null)
        {
            return; 
        }
        GenerateSurfaceMesh();
    }

    // 겉면 메쉬를 동적으로 생성하고 업데이트하는 함수
    void GenerateSurfaceMesh()
    {
        if (massPoints == null)
        {
            Debug.LogWarning("massPoints 배열이 설정되지 않았습니다.");
            return;
        }

        surfaceMesh.Clear();
        vertices.Clear();
        triangles.Clear();

        int size = massPoints.GetLength(0);

        // 각 면에 대해 단위 사각형을 생성하고 쿼드 추가
        for (int x = 0; x < size - 1; x++)
        {
            for (int y = 0; y < size - 1; y++)
            {
                for (int z = 0; z < size - 1; z++)
                {
                    // 윗면 (Top Face)
                    if (y == size - 2)
                        AddQuad(x, y + 1, z, x + 1, y + 1, z, x + 1, y + 1, z + 1, x, y + 1, z + 1);

                    // 아랫면 (Bottom Face)
                    if (y == 0)
                        AddQuad(x, y, z, x, y, z + 1, x + 1, y, z + 1, x + 1, y, z);

                    // 앞면 (Front Face)
                    if (z == 0)
                        AddQuad(x, y, z, x + 1, y, z, x + 1, y + 1, z, x, y + 1, z);

                    // 뒷면 (Back Face)
                    if (z == size - 2)
                        AddQuad(x, y, z + 1, x, y + 1, z + 1, x + 1, y + 1, z + 1, x + 1, y, z + 1);

                    // 왼쪽면 (Left Face)
                    if (x == 0)
                        AddQuad(x, y, z, x, y + 1, z, x, y + 1, z + 1, x, y, z + 1);

                    // 오른쪽면 (Right Face)
                    if (x == size - 2)
                        AddQuad(x + 1, y, z, x + 1, y, z + 1, x + 1, y + 1, z + 1, x + 1, y + 1, z);
                }
            }
        }

        surfaceMesh.vertices = vertices.ToArray();
        surfaceMesh.triangles = triangles.ToArray();
        surfaceMesh.RecalculateNormals();

        // 메쉬 렌더러와 필터를 설정하여 렌더링
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = surfaceMaterial;
        }

        meshFilter.mesh = surfaceMesh;
    }

    // 유효한 인덱스인지 확인하는 함수
    bool IsValidIndex(int x, int y, int z, int size)
    {
        return x >= 0 && x < size && y >= 0 && y < size && z >= 0 && z < size;
    }

    // 각 면의 단위 사각형을 쿼드로 추가하는 함수
    void AddQuad(int x0, int y0, int z0, int x1, int y1, int z1, int x2, int y2, int z2, int x3, int y3, int z3)
    {
        if (IsValidIndex(x0, y0, z0, massPoints.GetLength(0)) && 
            IsValidIndex(x1, y1, z1, massPoints.GetLength(0)) && 
            IsValidIndex(x2, y2, z2, massPoints.GetLength(0)) && 
            IsValidIndex(x3, y3, z3, massPoints.GetLength(0)))
        {
            Vector3 v0 = massPoints[x0, y0, z0].transform.localPosition;
            Vector3 v1 = massPoints[x1, y1, z1].transform.localPosition;
            Vector3 v2 = massPoints[x2, y2, z2].transform.localPosition;
            Vector3 v3 = massPoints[x3, y3, z3].transform.localPosition;

            AddQuad(v0, v1, v2, v3);
        }
    }

    // 두 개의 삼각형으로 구성된 쿼드를 메쉬에 추가하는 함수 (법선 방향 반대로)
    void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;
        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // 첫 번째 삼각형 (v0, v1, v2) - 순서 시계 방향으로 변경
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        // 두 번째 삼각형 (v0, v2, v3) - 순서 시계 방향으로 변경
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }
}
