// author: Stevie Giovanni
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// interface for everything that uses a ray to interact with objects
/// </summary>
public interface IHasRay
{
    Ray GetRay();
}

public enum SnapperMode { Exact, Vertice, Edge};

/// <summary>
/// raycast with the ability to snap to the nearest vertex or edge
/// </summary>
public class Snapper : MonoBehaviour {
    private void Start()
    {
        Mode = SnapperMode.Exact;
    }

    /// <summary>
    /// mode of the snapper
    /// </summary>
    [SerializeField]
    private SnapperMode _mode = SnapperMode.Exact;
    public SnapperMode Mode {
        get { return _mode; }
        set {
            _mode = value;
            if (Mode == SnapperMode.Exact) {
                nearestEdgeGO.SetActive(false);
                nearestVertexGO.SetActive(false);
            }
            if (Mode == SnapperMode.Vertice)
            {
                nearestEdgeGO.SetActive(false);
                nearestVertexGO.SetActive(true);
            }
            if (Mode == SnapperMode.Edge)
            {
                nearestEdgeGO.SetActive(true);
                nearestVertexGO.SetActive(false);
            }
        }
    }

    /// <summary>
    /// cycle through different modes
    /// </summary>
    public void NextMode()
    {
        if (Mode == SnapperMode.Exact)
            Mode = SnapperMode.Vertice;
        else if (Mode == SnapperMode.Vertice)
            Mode = SnapperMode.Edge;
        else if (Mode == SnapperMode.Edge)
            Mode = SnapperMode.Exact;
    }

    /// <summary>
    /// Line to show triangle
    /// </summary>
    private LineRenderer _line;
    public LineRenderer Line
    {
        get {
            if (_line == null)
                _line = GetComponent<LineRenderer>();
            return _line;
        }
    }

    [SerializeField]
    [Range(1,100)]
    private int _step = 10;
    public int Step
    {
        get { return _step; }
        set { _step = value; }
    }

    /// <summary>
    /// get the point
    /// </summary>
    public Vector3 ReturnValue
    {
        get
        {
            if (Mode == SnapperMode.Exact)
                return IntersectionPoint;
            else if (Mode == SnapperMode.Vertice)
                return ClosestVertice;
            else
                return ClosestPointOnEdge;
        }
    }

    /// <summary>
    /// raycast ray
    /// </summary>
    [SerializeField]
    private Ray _ray;
    public Ray Ray {
        get { return _ray; }
        set { _ray = value; }
    }

    /// <summary>
    /// indicator GOs
    /// </summary>
    [SerializeField]
    private GameObject intersectionPointGO;
    [SerializeField]
    private GameObject nearestVertexGO;
    [SerializeField]
    private GameObject nearestEdgeGO;

    /// <summary>
    /// whether we're hitting something or not
    /// </summary>
    [SerializeField]
    private bool _hit = false;
    public bool Hit { get { return _hit; } set { _hit = value; } }

    /// <summary>
    /// accessors for return values
    /// </summary>
    [SerializeField]
    private Vector3 _intersectionPoint;
    public Vector3 IntersectionPoint {
        get { return _intersectionPoint; }
        set
        {
            _intersectionPoint = value;
            if (intersectionPointGO != null)
                intersectionPointGO.transform.position = _intersectionPoint;
        }
    }

    [SerializeField]
    private Vector3 _closestVertice;
    public Vector3 ClosestVertice {
        get { return _closestVertice; }
        set
        {
            _closestVertice = value;
            if (nearestVertexGO != null)
                nearestVertexGO.transform.position = _closestVertice;
        }
    }

    [SerializeField]
    private Vector3 _closestPointOnEdge;
    public Vector3 ClosestPointOnEdge {
        get { return _closestPointOnEdge; }
        set
        {
            _closestPointOnEdge = value;
            if (nearestEdgeGO != null)
                nearestEdgeGO.transform.position = _closestPointOnEdge;
        }
    }

    /// <summary>
    /// calculates the projection of point p onto the line from v1 to v2
    /// </summary>
    public Vector3 CalculateProjection(Vector3 p, Vector3 v1, Vector3 v2)
    {
        Vector3 pToIntersection = p - v1;
        Vector3 pToNextP = (v2 - v1).normalized;
        Vector3 pointOnEdge = v1 + Vector3.Dot(pToIntersection, pToNextP) * pToNextP;
        return pointOnEdge;
    }

    /// <summary>
    /// calculates the step closest to point p on line from v1 to v2
    /// </summary>
    public Vector3 GetClosestStep(Vector3 p, Vector3 v1, Vector3 v2, int step)
    {
        if (step == 0) step = 1;

        float length = Vector3.Magnitude(v2 - v1); // length from v1 to v2
        float stepLength = length / step; // length of each step
        float lengthToP = Vector3.Magnitude(p - v1); // length from v1 to the point on edge
        int division = (int)Mathf.RoundToInt(lengthToP / stepLength); // get the closest division
        if (division > step) division = step;
        return (v1 + (v2 - v1) * division / step);
    }

	// Update is called once per frame
	void Update () {
        RaycastHit hitInfo;
        if(Physics.Raycast(Ray,out hitInfo, 20.0f, Physics.DefaultRaycastLayers)) { 
        /*if (Physics.Raycast(
            Camera.main.transform.position,
            Camera.main.transform.forward,
            out hitInfo,
            20.0f,
            Physics.DefaultRaycastLayers))
        {*/
            // only works for meshcolliders 
            MeshCollider meshCollider = hitInfo.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                // update hit flag
                Hit = true;

                // exact intersection point
                IntersectionPoint = hitInfo.point;

                // to find closest vertex and closest edge
                Mesh mesh = meshCollider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                Vector3 p0 = vertices[triangles[hitInfo.triangleIndex * 3 + 0]];
                Vector3 p1 = vertices[triangles[hitInfo.triangleIndex * 3 + 1]];
                Vector3 p2 = vertices[triangles[hitInfo.triangleIndex * 3 + 2]];
                Transform hitTransform = hitInfo.collider.transform;
                p0 = hitTransform.TransformPoint(p0);
                p1 = hitTransform.TransformPoint(p1);
                p2 = hitTransform.TransformPoint(p2);

                Line.enabled = true;
                Line.SetPosition(0, p0);
                Line.SetPosition(1, p1);
                Line.SetPosition(2, p2);

                // find the closest vertex
                List<Vector3> points = new List<Vector3>();
                points.Add(p0);
                points.Add(p1);
                points.Add(p2);
                float closestDist = Mathf.Infinity;
                int closestIndex = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    float curDist = Vector3.SqrMagnitude(hitInfo.point - points[i]);
                    if (curDist <= closestDist)
                    {
                        closestIndex = i;
                        closestDist = curDist;
                    }
                }

                ClosestVertice = points[closestIndex];

                // closest point on edge to intersection point
                Vector3 pointOnEdge1 = CalculateProjection(hitInfo.point, p0, p1);
                Vector3 pointOnEdge2 = CalculateProjection(hitInfo.point, p1, p2);
                Vector3 pointOnEdge3 = CalculateProjection(hitInfo.point, p2, p0);

                List<Vector3> pointsOnEdge = new List<Vector3>();
                pointsOnEdge.Add(pointOnEdge1);
                pointsOnEdge.Add(pointOnEdge2);
                pointsOnEdge.Add(pointOnEdge3);

                float closestDistToEdge = Mathf.Infinity;
                int closestIndexToEdge = 0;
                for (int i = 0; i < pointsOnEdge.Count; i++)
                {
                    float curDist = Vector3.SqrMagnitude(hitInfo.point - pointsOnEdge[i]);
                    if (curDist <= closestDistToEdge)
                    {
                        closestIndexToEdge = i;
                        closestDistToEdge = curDist;
                    }
                }

                if (closestIndexToEdge == 0)
                    ClosestPointOnEdge = GetClosestStep(pointsOnEdge[closestIndexToEdge], p0, p1, Step);
                else if(closestIndexToEdge == 1)
                    ClosestPointOnEdge = GetClosestStep(pointsOnEdge[closestIndexToEdge], p1, p2, Step);
                else
                    ClosestPointOnEdge = GetClosestStep(pointsOnEdge[closestIndexToEdge], p2, p0, Step);
            }
            else
            {
                // update hit flag
                Hit = false;

                Line.enabled = false;

                // zero everything
                IntersectionPoint = Vector3.zero;
                ClosestVertice = Vector3.zero;
                ClosestPointOnEdge = Vector3.zero;
            }
        }
        else
        {
            // update hit flag
            Hit = false;

            Line.enabled = false;

            // zero everything
            IntersectionPoint = Vector3.zero;
            ClosestVertice = Vector3.zero;
            ClosestPointOnEdge = Vector3.zero;
        }
	}

    private void OnDrawGizmos()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(
            Camera.main.transform.position,
            Camera.main.transform.forward,
            out hitInfo,
            20.0f,
            Physics.DefaultRaycastLayers))
        {
            MeshCollider meshCollider = hitInfo.collider as MeshCollider;
            if(meshCollider != null && meshCollider.sharedMesh != null)
            {
                // draw intersection
                Gizmos.color = Color.red;
                Gizmos.DrawLine(hitInfo.point, hitInfo.point + Vector3.up);
                Gizmos.DrawLine(hitInfo.point, hitInfo.point + Vector3.down);
                Gizmos.DrawLine(hitInfo.point, hitInfo.point + Vector3.left);
                Gizmos.DrawLine(hitInfo.point, hitInfo.point + Vector3.right);
                Gizmos.DrawLine(hitInfo.point, hitInfo.point + Vector3.forward);
                Gizmos.DrawLine(hitInfo.point, hitInfo.point + Vector3.back);

                // to get closest vertex and closest point on edge
                Mesh mesh = meshCollider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                Vector3 p0 = vertices[triangles[hitInfo.triangleIndex * 3 + 0]];
                Vector3 p1 = vertices[triangles[hitInfo.triangleIndex * 3 + 1]];
                Vector3 p2 = vertices[triangles[hitInfo.triangleIndex * 3 + 2]];
                Transform hitTransform = hitInfo.collider.transform;
                p0 = hitTransform.TransformPoint(p0);
                p1 = hitTransform.TransformPoint(p1);
                p2 = hitTransform.TransformPoint(p2);

                // draw triangles
                Gizmos.color = Color.green;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p0);

                // find the closest vertex
                List<Vector3> points = new List<Vector3>();
                points.Add(p0);
                points.Add(p1);
                points.Add(p2);
                float closestDist = Mathf.Infinity;
                int closestIndex = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    float curDist = Vector3.SqrMagnitude(hitInfo.point - points[i]);
                    if (curDist <= closestDist)
                    {
                        closestIndex = i;
                        closestDist = curDist; 
                    }
                }

                // draw closest vertice
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(points[closestIndex], 0.025f);

                // point on edges to intersection point
                Vector3 pointOnEdge1 = CalculateProjection(hitInfo.point, p0, p1);
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(pointOnEdge1, 0.025f);
                
                Vector3 pointOnEdge2 = CalculateProjection(hitInfo.point, p1, p2);
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(pointOnEdge2, 0.025f);
                
                Vector3 pointOnEdge3 = CalculateProjection(hitInfo.point, p2, p0);
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(pointOnEdge3, 0.025f);

                List<Vector3> pointsOnEdge = new List<Vector3>();
                pointsOnEdge.Add(pointOnEdge1);
                pointsOnEdge.Add(pointOnEdge2);
                pointsOnEdge.Add(pointOnEdge3);
                
                float closestDistToEdge = Mathf.Infinity;
                int closestIndexToEdge = 0;
                for (int i = 0; i < pointsOnEdge.Count; i++)
                {
                    float curDist = Vector3.SqrMagnitude(hitInfo.point - pointsOnEdge[i]);
                    if (curDist <= closestDistToEdge)
                    {
                        closestIndexToEdge = i;
                        closestDistToEdge = curDist; 
                    }
                }
                Vector3 finalPoint;
                if (closestIndexToEdge == 0)
                    finalPoint = GetClosestStep(pointsOnEdge[closestIndexToEdge], p0, p1, Step);
                else if (closestIndexToEdge == 1)
                    finalPoint = GetClosestStep(pointsOnEdge[closestIndexToEdge], p1, p2, Step);
                else
                    finalPoint = GetClosestStep(pointsOnEdge[closestIndexToEdge], p2, p0, Step);

                // closest point on edge
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(finalPoint, 0.03f);
            }
        }
    }
}
