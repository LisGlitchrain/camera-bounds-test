using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class SphereTrackingCamera : MonoBehaviour
{
    public List<GameObject> givenRenderedObjects = new List<GameObject>();
    List<GameObject> renderedObjects = new List<GameObject>();
    Dictionary<GameObject, MeshFilter> meshFilters = new Dictionary<GameObject, MeshFilter>();
    Dictionary<GameObject, Renderer> renderers = new Dictionary<GameObject, Renderer>();

    public List<GameObject> worldSpacePoints;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public float baseFoV = 60;
    public float spacing = 1.05f;
    float horizontalSpacing;

    Camera camera;

    public List<RectTransform> panels;


    void Start()
    { 
        camera = GetComponent<Camera>();
        foreach (var givenObject in givenRenderedObjects)
            AddRenderer(givenObject);

        horizontalSpacing = (spacing - 1) / camera.aspect + 1;
        Debug.Log($"HOrizontal spacing: {horizontalSpacing}");

        SetPanels();
    }

    void SetPanels()
    {
        panels[0].anchorMin = new Vector2(0, 0);
        panels[0].anchorMax = new Vector2(1, (spacing - 1) / 2 /  camera.aspect);

        panels[1].anchorMin = new Vector2(0, 1 - (spacing - 1) / 2 / camera.aspect);
        panels[1].anchorMax = new Vector2(1, 1);

        panels[2].anchorMin = new Vector2(0, (spacing - 1) / 2 / camera.aspect);
        panels[2].anchorMax = new Vector2((horizontalSpacing - 1) / 2 , 1 - (spacing - 1) / 2 / camera.aspect);
        Debug.Log($"Anchor2: { panels[2].anchorMin}| {panels[2].anchorMax}");

        panels[3].anchorMin = new Vector2(1 - (horizontalSpacing - 1) / 2, (spacing - 1) / 2 / camera.aspect);
        panels[3].anchorMax = new Vector2(1f, 1 - (spacing - 1) / 2 / camera.aspect);
        Debug.Log($"Anchor2: { panels[3].anchorMin}| {panels[3].anchorMax}");


        foreach (var panel in panels)
        {
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }
    }

    public void AddRenderer(GameObject renderedObj)
    {
        if (!renderedObjects.Contains(renderedObj) && renderedObj.TryGetComponent<Renderer>(out var renderer) && renderedObj.TryGetComponent<MeshFilter>(out var meshFilter))
        {
            renderedObjects.Add(renderedObj);
            meshFilters.Add(renderedObj, meshFilter);
            renderers.Add(renderedObj, renderer);
        }
    }

    public void RemoveRenderer(GameObject renderedObj)
    {
        if (renderedObjects.Contains(renderedObj))
        {
            renderedObjects.Remove(renderedObj);
            meshFilters.Remove(renderedObj);
            renderers.Remove(renderedObj);
        }
    }

    private void Update()
    {
        var points = new List<Vector3>();

        foreach (var renderedObj in renderedObjects)
        {
            //Getting object's points in cam local space
            var objPoints = GetObjectPointsInCamSpace(renderedObj);
            //Getting local obj's points in spherical coords.
            for (var i = 0; i < objPoints.Length; i++)
                objPoints[i] = ToSpherical(objPoints[i]);

            points.AddRange(objPoints);
        }

        //Checking max and min polar and elevation in spherical coords
        var minPolar = (float)( points.Min(x => x.y));
        var maxPolar = (float)( points.Max(x => x.y));
        var minElevation = points.Min(x => x.z);
        var maxElevation = points.Max(x => x.z);

        //Calculating required camera's rotation
        var polarRequiredRotation = (maxPolar + minPolar) / 2;
        var elevationRequiredRotation = (maxElevation + minElevation) / 2;
        var directionVectLocal = new Vector3(1, polarRequiredRotation, elevationRequiredRotation);
        directionVectLocal = ToCartesian(directionVectLocal);
        var directionVect = camera.transform.TransformPoint(directionVectLocal);
        camera.transform.LookAt(directionVect, Vector3.up);

        //Calculating required FoV
        var verticalFoVFromHorizontal = Camera.HorizontalToVerticalFieldOfView((maxPolar - minPolar) * (1 + horizontalSpacing * Mathf.Abs(Mathf.Atan(maxPolar - minPolar) / Mathf.PI) ), camera.aspect);
        var verticalFoV = (maxElevation - minElevation) * spacing;
        var requiredFoV = verticalFoVFromHorizontal > verticalFoV ? verticalFoVFromHorizontal : verticalFoV;
        var requiredFoVInDegrees = requiredFoV * Mathf.Rad2Deg;
        camera.fieldOfView = requiredFoVInDegrees;
    }


    Vector3[] GetObjectPointsInCamSpace(GameObject renderedObject)
    {
        //Redundant precision for current algorithm. Just in case of future improvements.
        var points = meshFilters[renderedObject].mesh.vertices;
        for (var i = 0; i < points.Length; i++)
        {        
            points[i] = renderedObject.transform.TransformPoint(points[i]);
            points[i] = camera.transform.InverseTransformPoint(points[i]);
        }
        return points;
    }

    public Vector3 ToCartesian(Vector3 sphericalVector)
    {
        float a = sphericalVector.x * Mathf.Cos(sphericalVector.z);
        return new Vector3(a * Mathf.Cos(sphericalVector.y), sphericalVector.x * Mathf.Sin(sphericalVector.z), a * Mathf.Sin(sphericalVector.y));
    }

    public Vector3 ToSpherical(Vector3 cartesianCoordinate)
    {
        if (cartesianCoordinate.x == 0f)
            cartesianCoordinate.x = Mathf.Epsilon;
        var radius = cartesianCoordinate.magnitude;

        var polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

        if (cartesianCoordinate.x < 0f)
            polar += Mathf.PI;
        var elevation = Mathf.Asin(cartesianCoordinate.y / radius);
        return new Vector3(radius, polar, elevation);
    }
}