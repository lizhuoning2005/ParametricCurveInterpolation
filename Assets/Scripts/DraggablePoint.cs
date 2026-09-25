using UnityEngine;

public class DraggablePoint : MonoBehaviour
{
    private Camera mainCam;
    public CurveManager curveMgr;

    void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnMouseDrag()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float dis))
        {
            Vector3 newPos = ray.GetPoint(dis);
            transform.position = new Vector3(newPos.x, 0, newPos.z);
            curveMgr.ReDrawCurve();
        }
    }
}

