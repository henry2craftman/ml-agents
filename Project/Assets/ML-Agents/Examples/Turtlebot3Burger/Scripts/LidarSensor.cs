using UnityEngine;

public class LidarSensor : MonoBehaviour
{
    public bool isLidarOn = false;
    public bool isGizmoOn = false;
    public int samplesPerScan = 360; // 레이의 개수 (360도)
    public float maxDistance = 8f; // 레이의 최대 거리 (8m -> 8000mm)
    public float minDistance = 0.16f; // 레이의 최소 거리 (0.16m -> 160mm)
    public float scanFrequency = 5f; // 초당 스캔 횟수
    private float scanInterval;
    private float nextScanTime;
    private float[] lastDistances; // 마지막 스캔 거리 저장


    void Start()
    {
        scanInterval = 1f / scanFrequency;
        nextScanTime = Time.time;
        lastDistances = new float[samplesPerScan]; // 초기화
    }

    void Update()
    {
        if (!isLidarOn) return;

        if (Time.time >= nextScanTime)
        {
            PerformScan();
            nextScanTime += scanInterval;
        }
    }

    void PerformScan()
    {
        float angleStep = 360f / samplesPerScan; // 각도 간격

        for (int i = 0; i < samplesPerScan; i++)
        {
            float angle = i * angleStep; // 현재 각도
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward; // 방향 벡터

            RaycastHit hit;
            float measuredDistance;

            if (Physics.Raycast(transform.position, direction, out hit, maxDistance))
            {
                measuredDistance = hit.distance; // 물체와의 거리
            }
            else
            {
                measuredDistance = maxDistance; // 최대 거리 설정
            }

            // 거리 정확도 적용 및 범위 클램핑
            lastDistances[i] = ApplyDistanceAccuracy(measuredDistance);
            lastDistances[i] = Mathf.Clamp(lastDistances[i], minDistance, maxDistance); // 최소/최대 거리 적용
        }

        // 샘플링 결과 처리
        ProcessScanData(lastDistances);
    }

    float ApplyDistanceAccuracy(float distance)
    {
        // 거리 범위에 따라 정확도를 적용
        if (distance < 0.3f) // 0.3m
        {
            return Mathf.Clamp(distance + Random.Range(-0.01f, 0.01f), minDistance, maxDistance); // ±10mm
        }
        else if (distance < 6f) // 6m
        {
            return distance * (1 + Random.Range(-0.03f, 0.03f)); // ±3.0%
        }
        else if (distance <= 8f) // 8m
        {
            return distance * (1 + Random.Range(-0.05f, 0.05f)); // ±5.0%
        }
        return distance;
    }

    void ProcessScanData(float[] distances)
    {
        // 여기에서 거리 데이터를 처리하는 코드를 추가하세요.
        Debug.Log("Scan completed: " + string.Join(", ", distances));
    }

    void OnDrawGizmos()
    {
        if (!isLidarOn || !isGizmoOn) return;

        if (lastDistances != null)
        {
            Gizmos.color = Color.red; // 레이 색상 설정
            float angleStep = 360f / samplesPerScan;

            for (int i = 0; i < samplesPerScan; i++)
            {
                float angle = i * angleStep; // 현재 각도
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward; // 방향 벡터
                Vector3 startPoint = transform.position; // 시작점

                // 레이의 끝점 클램핑
                float clampedDistance = Mathf.Clamp(lastDistances[i], minDistance, maxDistance);
                Vector3 endPoint = startPoint + direction * clampedDistance; // 끝점

                Gizmos.DrawLine(startPoint, endPoint); // 레이 그리기
            }
        }
    }
}
