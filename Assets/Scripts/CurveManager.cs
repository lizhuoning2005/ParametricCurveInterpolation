using System.Collections.Generic;
using UnityEngine;

public enum CurveType
{
    Linear,
    Hermite,
    CatmullRom,
    CubicBezier
}

public class CurveManager : MonoBehaviour
{
    [Header("曲线配置")]
    public CurveType curveType;
    public GameObject pointPrefab;
    public Transform pointParent;
    public LineRenderer lineRenderer;
    [Range(50, 300)] public int sampleCount = 150;

    [Header("Hermite 曲线参数(仅Hermite模式生效)")]
    public Vector3 tan0 = new Vector3(2, 0, 0);
    public Vector3 tan1 = new Vector3(2, 0, -2);

    [Header("Catmull‑Rom 参数 alpha，课件s，常用0.5")]
    public float alpha = 0.5f;

    private List<Transform> controlPoints = new List<Transform>();

    void Start()
    {
        // 4个初始控制点 P0 P1 P2 P3
        Vector3[] initPos = {
            new Vector3(-4,0,-2),
            new Vector3(-1,0,1),
            new Vector3(2,0,-1),
            new Vector3(4,0,2)
        };
        foreach (var p in initPos)
        {
            GameObject go = Instantiate(pointPrefab, p, Quaternion.identity, pointParent);
            DraggablePoint dp = go.GetComponent<DraggablePoint>();
            dp.curveMgr = this;
            controlPoints.Add(go.transform);
        }
        ReDrawCurve();
    }

    public void ReDrawCurve()
    {
        if (controlPoints.Count < 2) return;
        List<Vector3> curvePoints = new List<Vector3>();
        for (int i = 0; i <= sampleCount; i++)
        {
            float u = (float)i / sampleCount;
            Vector3 pt = ComputePoint(u);
            curvePoints.Add(pt);
        }
        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer.SetPositions(curvePoints.ToArray());
    }

    Vector3 ComputePoint(float u)
    {
        switch (curveType)
        {
            case CurveType.Linear:
                return LinearInterp(u);
            case CurveType.Hermite:
                // Hermite：单段，只用第0、1两个控制点 + 外部切线
                return Hermite(controlPoints[0].position, controlPoints[1].position, tan0, tan1, u);
            case CurveType.CatmullRom:
                return CatmullRomMultiSegment(u);
            case CurveType.CubicBezier:
                // 单段三次贝塞尔，4点：P0起点，P1P2手柄，P3终点
                return CubicBezier(
                    controlPoints[0].position,
                    controlPoints[1].position,
                    controlPoints[2].position,
                    controlPoints[3].position, u);
            default: return Vector3.zero;
        }
    }

    // 分段线性插值：全部控制点串联
    Vector3 LinearInterp(float u)
    {
        int segCount = controlPoints.Count - 1;
        float t = u * segCount;
        int idx = Mathf.FloorToInt(t);
        idx = Mathf.Clamp(idx, 0, segCount - 1);
        float localU = t - idx;
        return Vector3.Lerp(controlPoints[idx].position, controlPoints[idx + 1].position, localU);
    }

    // 单段Hermite
    Vector3 Hermite(Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1, float u)
    {
        float h0 = 2 * u * u * u - 3 * u * u + 1;
        float h1 = -2 * u * u * u + 3 * u * u;
        float h2 = u * u * u - 2 * u * u + u;
        float h3 = u * u * u - u * u;
        return h0 * p0 + h1 * p1 + h2 * m0 + h3 * m1;
    }

    // ✅修复：多段Catmull‑Rom，整条平滑穿过全部内部控制点
    Vector3 CatmullRomMultiSegment(float u)
    {
        int pointCnt = controlPoints.Count;
        // Catmull‑Rom边界处理：首尾补充虚拟控制点
        List<Vector3> pts = new List<Vector3>();
        foreach (var t in controlPoints) pts.Add(t.position);

        // 虚拟头点，解决第一段边界切线
        Vector3 pHead = 2 * pts[0] - pts[1];
        pts.Insert(0, pHead);
        // 虚拟尾点，解决最后一段边界切线
        Vector3 pTail = 2 * pts[pts.Count - 1] - pts[pts.Count - 2];
        pts.Add(pTail);

        int segTotal = pts.Count - 3;
        float tParam = u * segTotal;
        int segIdx = Mathf.FloorToInt(tParam);
        segIdx = Mathf.Clamp(segIdx, 0, segTotal - 1);
        float s = tParam - segIdx;

        Vector3 p0 = pts[segIdx];
        Vector3 p1 = pts[segIdx + 1];
        Vector3 p2 = pts[segIdx + 2];
        Vector3 p3 = pts[segIdx + 3];

        Vector3 t1 = alpha * (p2 - p0);
        Vector3 t2 = alpha * (p3 - p1);
        return Hermite(p1, p2, t1, t2, s);
    }

    // 单段三次贝塞尔
    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float u)
    {
        float u1 = 1 - u;
        return u1 * u1 * u1 * p0
                + 3 * u1 * u1 * u * p1
                + 3 * u1 * u * u * p2
                + u * u * u * p3;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ReDrawCurve();
        }
    }
}

