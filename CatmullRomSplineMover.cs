using UnityEngine;

public class CatmullRomSplineMover : MonoBehaviour
{
    public Transform[] controlPoints; // 控制点，包括起点和终点
    public float speed = 0.1f; // 物体移动速度
    public bool loop = false; // 是否循环
    public bool closeCurve = false; // 是否闭合曲线

    private float t = 0f; // 参数t，用于计算样条上的位置
    private int currentSegment = 0; // 当前样条段

    void Update()
    {
        if (controlPoints == null || controlPoints.Length < 4) return;

        Vector3 p0 = GetControlPoint(currentSegment - 1);
        Vector3 p1 = GetControlPoint(currentSegment);
        Vector3 p2 = GetControlPoint(currentSegment + 1);
        Vector3 p3 = GetControlPoint(currentSegment + 2);

        Vector3 position = ComputeCatmullRomPosition(t, p0, p1, p2, p3);
        Vector3 tangent = ComputeCatmullRomTangent(t, p0, p1, p2, p3); // 计算切线来确定方向

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(tangent); // 使用切线方向设置旋转

        t += speed * Time.deltaTime;

        if (t > 1f)
        {
            t = 0f;
            currentSegment++;

            if (currentSegment > controlPoints.Length - (closeCurve ? 1 : 3))
            {
                if (loop)
                {
                    currentSegment = 0; // 重置以重新开始
                }
                else
                {
                    // 停止移动
                    this.enabled = false;
                }
            }
        }
    }
    Vector3 ComputeCatmullRomTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;

        Vector3 a = (-p0 + 3f * p1 - 3f * p2 + p3) / 2f;
        Vector3 b = (2f * p0 - 5f * p1 + 4f * p2 - p3) / 2f;
        Vector3 c = (-p0 + p2) / 2f;

        return 3 * a * t2 + 2 * b * t + c; // 切线方程的导数
    }
    
    private Vector3 GetControlPoint(int i)
    {
        int index = (i + controlPoints.Length) % controlPoints.Length;
        return controlPoints[Mathf.Clamp(index, 0, controlPoints.Length - 1)].position;
    }

    private Vector3 ComputeCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + b * t + c * t2 + d * t3);
    }
    
    void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Length < 4) return;

        Gizmos.color = Color.green;

        int segments = controlPoints.Length - (closeCurve ? 0 : 2);
        for (int i = 0; i < segments; i++)
        {
            Vector3 p0 = GetControlPoint(i - 1);
            Vector3 p1 = GetControlPoint(i);
            Vector3 p2 = GetControlPoint(i + 1);
            Vector3 p3 = GetControlPoint(i + 2);

            for (float t = 0; t <= 1; t += 0.05f)
            {
                Vector3 start = ComputeCatmullRomPosition(t, p0, p1, p2, p3);
                Vector3 end = ComputeCatmullRomPosition(t + 0.05f, p0, p1, p2, p3);

                Gizmos.DrawLine(start, end);
            }
        }
    }

}
