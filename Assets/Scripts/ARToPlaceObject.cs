using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
//using UnityEngine.Experimental.XR;
using System;
//using UnityEngine.UI;
using TMPro;
//using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
//using UnityEngine.XR.OpenXR.Input;
using Pose = UnityEngine.Pose;
//using UnityEngine.InputSystem.EnhancedTouch;
//using UnityEngine.EventSystems;
//using Unity.VisualScripting;
//using UnityEngine.Experimental.GlobalIllumination;
//using System.Drawing;
//using Microsoft.Unity.VisualStudio.Editor;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;
using UnityEngine.XR.OpenXR.Input;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ARToPlaceObject : MonoBehaviour
{

    //public GameObject objectToPlace;
    [SerializeField] public GameObject placementIndicator;
    [SerializeField] public GameObject[] vectorList;
    [SerializeField] public GameObject[] Text;
    [SerializeField] public TextMeshProUGUI[] HLN;
    [SerializeField] public TextMeshProUGUI[] HLNText;
    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] public Button deletePointsButton;
    [SerializeField] public Button destroyPointButton;
    [SerializeField] public Button showPointsButton;
    [SerializeField] public Button showVectorsButton;
    [SerializeField] public Button addLinePlaneButton;
    [SerializeField] public Button MaxSelectedButton;
    [SerializeField] public Button[] addObjectButtons;
    [SerializeField] public Button[] xyzCoordsButtons;
    [SerializeField] public UnityEngine.UI.Slider heightSlider;
    [SerializeField] public UnityEngine.UI.Slider scaleSlider;
    [SerializeField] public Button heightSliderButton;
    [SerializeField] public Button sizeSliderButton;
    [SerializeField] public TMPro.TMP_Dropdown FigureDropDown;

    private float eps = 0.001f;

    private AddPoint addPointClass;
    private Camera cameraPC;
    private ARSessionOrigin arOrigin;
    private ARRaycastManager rayCastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = true;
    private bool placementActive = true;

    private Vector2 TouchPosition;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    //public Button[] buttons;
    //public bool bRotation = false;

    private Quaternion yRotation;

    short maxSelectedObject = 1;
    private List<GameObject> selectedObject = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    private List<GameObject> Points = new List<GameObject>();
    private List<GameObject> planes = new List<GameObject>();


    [SerializeField] public TextMeshProUGUI TMPro1;
    [SerializeField] public TextMeshProUGUI TMPro2;
    [SerializeField] public TextMeshProUGUI TMPro3;

    float defaultHeight = 1;
    float defaultScale = 1;

    private int SearchObject(Vector3 temp)
    {
        float q = 0, w = 0;
        for (int i = 0; i < Points.Count; i++)
        {
            if (Vector3.Distance(temp, Points[i].transform.localPosition) < eps / defaultScale * scaleSlider.value) return i;
        }
        return -1;
    }

    private Vector3 AddPointCoord(Vector3 pose, Vector3 center, float rotation)
    {
        //pose /= 50;
        //pose = pose / defaultScale * scaleSlider.value;
        //double rotation = -toPlaceTransform.eulerAngles.y;
        float x = 0;
        x += pose.x;
        float x1 = center.x;
        float z = 0;
        z += pose.z;
        float z1 = center.z;
        double r = Math.Sqrt(x * x + z * z);
        double t = Math.Atan2(z, x);
        double t2 = t + rotation / 180.0 * Math.PI;
        double x2 = (double)x1 + r * Math.Cos(t2);
        double z2 = (double)z1 + r * Math.Sin(t2);
        pose.x = (float)x2;
        //pose.x = x + x1;
        //pose.z = z + z1;
        pose.z = (float)z2;
        pose.y = pose.y + center.y;
        return pose;
    }

    public bool SelectedTwoObject()
    {
        return selectedObject.Count == 2;
    }
    public GameObject SpawnObject(GameObject oToSpawn, Vector3 pose, Quaternion A)
    {
        if (selectedObject.Count == 2)
        {
            pose = (selectedObject[0].transform.localPosition + selectedObject[1].transform.localPosition) / 2;
        }
        if (oToSpawn != null && selectedObject.Count % 2 == 0)
        {
            Transform toPlaceTransform = placementIndicator.transform;
            //pose = AddPointCoord(pose, toPlaceTransform);
            if (SearchObject(pose) == -1)
            {
                GameObject newObject = Instantiate(oToSpawn, pose, A, toPlaceTransform);
                newObject.transform.localPosition = pose;
                Points.Add(newObject);
                newObject.tag = "Point";
                if (newObject.GetComponent<MeshRenderer>() == null)
                {
                    newObject.AddComponent<MeshRenderer>();
                }
                newObject.GetComponent<MeshRenderer>().enabled = PointsEnabled;
                TMPro1.text = "X: " + pose.x.ToString() + ", Y: " + pose.y.ToString() + ", Z: " + pose.z.ToString();
                newObject.AddComponent<PointS>();
                if (selectedObject.Count == 2)
                {
                    PointS ps = newObject.GetComponent<PointS>();
                    GameObject A1 = selectedObject[0];
                    GameObject A2 = selectedObject[1];
                    ps.setLineCoords(ref A1, ref A2);
                }
                //newObject.transform.localScale = newObject.transform.localScale / defaultScale * scaleSlider.value;
                //TMPro1.text = "X: " + globalPosition.x.ToString() + ", Y: " + globalPosition.y.ToString() + ", Z: " + globalPosition.z.ToString();
                return newObject;
            }
            return oToSpawn;
        }
        return oToSpawn;
    }

    private void DeletePoint()
    {
        if (selectedObject.Count == 1)
        {
            Destroy(selectedObject[0]);
            selectedObject.Clear();
        }
        else
        {
            ClearSelectedObject();
        }
    }

    private void ClearPoints()
    {
        foreach (var i in Points)
        {
            Destroy(i);
        }
        foreach (var i in planes)
        {
            Destroy(i);
        }
        planes.Clear();
        selectedObject.Clear();
        Points.Clear();
        lines.Clear();
    }

    private void ClearSelectedObject(int x = 0)
    {
        for (int i = 0; i < selectedObject.Count - x; i++)
        {
            selectedObject[i].GetComponent<Renderer>().material.color = defaultColor;
        }
        if (selectedObject.Count != 0)
            selectedObject.RemoveRange(0, selectedObject.Count - x);
    }

    private int SearchSelectedObject(GameObject temp)
    {
        for (int i = 0; i < selectedObject.Count; i++)
        {
            if (selectedObject[i] == temp) return i;
        }
        return -1;
    }


    public void AddPointSelect(GameObject temp)
    {
        if (temp == null) return;

        int i = SearchSelectedObject(temp);
        if (i >= 0)
        {
            selectedObject[i].GetComponent<Renderer>().material.color = defaultColor;
            selectedObject.RemoveAt(i);
        }
        else
        {
            if (selectedObject.Count == maxSelectedObject)
            {
                selectedObject[0].GetComponent<Renderer>().material.color = defaultColor;
                selectedObject.RemoveAt(0);
            }
            temp.GetComponent<Renderer>().material.color = selectColor;
            selectedObject.Add(temp);

        }
        /*if (selectedObject[0] == temp || selectedObject[1] == temp || selectedObject[2] == temp)
        {
            if (selectedObject[2] == temp)
            {
                selectedObject[2].GetComponent<Renderer>().material.color = defaultColor;
                selectedObject[2] = null;
            }
            else if (selectedObject[1] == temp)
            {
                selectedObject[1].GetComponent<Renderer>().material.color = defaultColor;
                selectedObject[1] = null;
            }
            else
            {
                selectedObject[0].GetComponent<Renderer>().material.color = defaultColor;
                selectedObject[0] = selectedObject[1];
                selectedObject[1] = null;
            }
        }
        else
        {
            if ()
                if (selectedObject[1] != null)
                    selectedObject[1].GetComponent<Renderer>().material.color = defaultColor;
            selectedObject[1] = selectedObject[0];
            selectedObject[0] = temp;
            selectedObject[0].GetComponent<Renderer>().material.color = selectColor;
        }*/
    }

    private void UpdatePlacementPose()
    {
        if (Camera.main != null) // Добавил 
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hitsList = new List<ARRaycastHit>();
            rayCastManager.Raycast(screenCenter, hitsList);

            placementPoseIsValid = hitsList.Count > 0;

            if (placementPoseIsValid)
            {
                placementPose = hitsList[0].pose;

                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
                placementPose.rotation = Quaternion.LookRotation(cameraBearing);

                foreach (var i in Text)
                {
                    i.transform.rotation = placementPose.rotation;
                }
            }
        }
    }

    float defaultY;
    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            UpYDefault();
            defaultY = placementIndicator.transform.position.y;
            placementIndicator.SetActive(true);
            //placementIndicator.transform.SetPositionAndRotation(placementPose.position,placementPose.rotation);
            placementIndicator.transform.position = placementPose.position;
            //placementIndicator.transform.position = new Vector3( placementIndicator.transform.position.x,
            //    placementIndicator.transform.position.y+heightSlider.value, placementIndicator.transform.position.z);
        }
        else
            placementIndicator.SetActive(false);

    }

    public void deleteLine(GameObject temp)
    {
        lines.Remove(temp);
    }
    public void deletePoint(GameObject temp)
    {
        Points.Remove(temp);
    }
    private void CreateLinePlane()
    {
        if (selectedObject.Count == 2)
            CreateLine(selectedObject[0], selectedObject[1]);
        else if (selectedObject.Count == 3)
            CreatePlane(selectedObject[0], selectedObject[1], selectedObject[2]);

    }
    private bool SearchLine(ref GameObject selectedObject1, ref GameObject selectedObject2)
    {
        if (selectedObject1 != null && selectedObject2 != null)
        {
            PointS pSO1 = selectedObject1.GetComponent<PointS>();
            foreach (var x in pSO1.lines)
                if (x.GetComponent<Line>().startPoint == selectedObject2 || selectedObject2 == x.GetComponent<Line>().endPoint)
                    return false;
            return true;
        }
        return false;
    }
    private bool RemoveLine(GameObject selectedObject1 = null, GameObject selectedObject2 = null)
    {
        if (selectedObject1 != null && selectedObject2 != null)
        {
            PointS pSO1 = selectedObject1.GetComponent<PointS>();
            foreach (var x in pSO1.lines)
                if (x.GetComponent<Line>().startPoint == selectedObject2 || selectedObject2 == x.GetComponent<Line>().endPoint)
                {
                    Destroy(x);
                    return true;
                }

        }
        return false;
    }
    private void CreateLine(GameObject selectedObject1 = null, GameObject selectedObject2 = null)
    {
        if (selectedObject1 != null && selectedObject2 != null && selectedObject1 != selectedObject2)
        {
            if (!SearchLine(ref selectedObject1, ref selectedObject2))
                if (RemoveLine(selectedObject1, selectedObject2))
                    return;
            GameObject lineObject = new GameObject("Line");

            Line line = lineObject.AddComponent<Line>();

            line.startPoint = selectedObject1;
            line.endPoint = selectedObject2;

            PointS pointScript1 = selectedObject1.GetComponent<PointS>();
            PointS pointScript2 = selectedObject2.GetComponent<PointS>();

            pointScript1.lines.Add(lineObject);
            pointScript2.lines.Add(lineObject);

            line.lineWidth = 0.001f * scaleSlider.value;
            line.CreateLine();
            line.UpdateLine();

            lineObject.transform.SetParent(placementIndicator.transform);

            lines.Add(lineObject);
        }
    }

    private void CreatePlane(GameObject A, GameObject B, GameObject C)
    {
        // Получаем позиции и повороты точек
        Vector3 pointA = A.transform.localPosition;
        Vector3 pointB = B.transform.localPosition;
        Vector3 pointC = C.transform.localPosition;

        // Создаем четырехугольную плоскость
        GameObject quadPlane = new GameObject("QuadPlane");
        MeshFilter meshFilter = quadPlane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = quadPlane.AddComponent<MeshRenderer>();

        // Создаем вершины четырехугольника
        Vector3 temp = ((Vector3.Distance(pointA, pointC) > Vector3.Distance(pointB, pointC)) ? (pointA) : (pointB));
        Vector3 temp2 = ((Vector3.Distance(pointA, pointC) > Vector3.Distance(pointB, pointC)) ? (pointB) : (pointA));
        temp = ((Vector3.Distance(temp, temp2) < Vector3.Distance(-temp, temp2)) ? (-temp2 + temp) : (-temp + temp2));
        Vector3[] vertices = new Vector3[4];
        vertices[0] = pointA;
        vertices[1] = pointB;
        vertices[2] = pointC;
        vertices[3] = pointC - temp; // Четвертая вершина - среднее значение точек A и C

        // Создаем треугольники
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        // Создаем нормали для вершин
        Vector3[] normals = new Vector3[4];
        normals[0] = Vector3.up;
        normals[1] = Vector3.up;
        normals[2] = Vector3.up;
        normals[3] = Vector3.up;

        // Создаем UV-координаты
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(0, 1);

        // Создаем меш и применяем его к фильтру меша
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        meshFilter.mesh = mesh;

        // Создаем материал и применяем его к рендереру меша
        Material material = new Material(Shader.Find("Standard"));
        material.doubleSidedGI = true; // Делаем плоскость непрозрачной с обеих сторон
        meshRenderer.material = material;

        // Добавляем плоскость как дочерний объект placementIndicator.transform
        quadPlane.transform.SetParent(placementIndicator.transform);

        // Устанавливаем позицию и поворот плоскости
        Vector3 center = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4f;
        quadPlane.transform.localPosition = center;
        quadPlane.transform.localScale = Vector3.one;
        quadPlane.transform.rotation = placementIndicator.transform.rotation;
    }





    private Color defaultColor = Color.white;
    private Color selectColor = Color.red;
    private void RotateAndMovePlace()
    {
        if (telephone)
            if (placementActive) UpdatePlacementIndicator();
            else UpY(); // move and Y
        if (Input.touchCount == 1)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (selectedObject.Count == 0 || xCoord && yCoord && zCoord || !xCoord && !yCoord && !zCoord)
                if (touch.phase == TouchPhase.Moved)
                {
                    yRotation = Quaternion.Euler(0f, -touch.deltaPosition.x * 0.1f, 0f);
                    placementIndicator.transform.rotation *= yRotation;
                }
        }
    }


    private void TFMovePlace()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
            placementActive = true;
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            placementActive = false;

        //TMPro3.text = "Move: " + ((placementActive) ? "on" : "off");
        //buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = ((placementActive) ? ("Закрепить\nоси") : ("Передвинуть\nоси"));
    }

    bool PointsEnabled = true;
    private void ShowPoints()
    {
        PointsEnabled = !PointsEnabled;
        foreach (var i in Points)
            i.GetComponent<MeshRenderer>().enabled = PointsEnabled;
    }
    bool VectorEnabled = true;
    private void ShowVectors()
    {
        VectorEnabled = !VectorEnabled;
        foreach (var i in vectorList)
            i.GetComponent<MeshRenderer>().enabled = VectorEnabled;
    }

    private void UpYDefault()
    {
        heightSlider.value = defaultHeight;
    }
    private void UpY()
    {
        placementIndicator.transform.position = new Vector3(placementIndicator.transform.position.x, defaultY + heightSlider.value, placementIndicator.transform.position.z);
    }

    private void ScaleSliderObjectDefault()
    {
        scaleSlider.value = defaultScale;
    }
    private void ScaleSliderObject()
    {
        float scale = scaleSlider.value;
        placementIndicator.transform.localScale = new Vector3(scale, scale, scale);
        foreach (var i in lines)
        {
            i.GetComponent<Line>().lineWidth = 0.001f * scale;
        }
    }

    private GameObject ReturnGameObject(ref GameObject temp, Vector3 V)
    {
        return SpawnObject(temp, V / 250, new Quaternion(0f, 0f, 0f, 0f));
    }
    //Стандартные фигуры
    private void AddFigureSquare()
    {
        if (objectToSpawn == null) return;

        string sx, sy, sz;
        sx = HLN[0].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sy = HLN[1].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sz = HLN[2].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sx = sx.Substring(0, sx.Length - 1);
        sy = sy.Substring(0, sy.Length - 1);
        sz = sz.Substring(0, sz.Length - 1);
        float x = 0;
        float y = 0;
        float z = 0;
        bool bx = (sx.Length > 0 ? float.TryParse(sx, out x) : false);
        bool by = (sy.Length > 0 ? float.TryParse(sy, out y) : false);
        bool bz = (sz.Length > 0 ? float.TryParse(sz, out z) : false);

        if (!bx || !by || !bz) return;
        float weight = y;
        float length = x;
        float h = z;
        Vector3 begin = new Vector3(0f, 0f, 0f);
        Vector3 a, a2, b, b2, c, c2, d, d2;
        a = new Vector3(begin.x, h + begin.y, begin.z);
        b = new Vector3(begin.x + weight, h + begin.y, begin.z);
        c = new Vector3(begin.x + weight, h + begin.y, begin.z + length);
        d = new Vector3(begin.x, h + begin.y, begin.z + length);
        h += 5;
        a2 = new Vector3(begin.x, h + begin.y, begin.z);
        b2 = new Vector3(begin.x + weight, h + begin.y, begin.z);
        c2 = new Vector3(begin.x + weight, h + begin.y, begin.z + length);
        d2 = new Vector3(begin.x, h + begin.y, begin.z + length);

        List<GameObject> tempList = new List<GameObject>();
        GameObject A, B, C, D, A2, B2, C2, D2;
        A = ReturnGameObject(ref objectToSpawn, a);
        A2 = ReturnGameObject(ref objectToSpawn, a2);
        if (A == A2) return;
        B = ReturnGameObject(ref objectToSpawn, b);
        B2 = ReturnGameObject(ref objectToSpawn, b2);
        C = ReturnGameObject(ref objectToSpawn, c);
        C2 = ReturnGameObject(ref objectToSpawn, c2);
        D = ReturnGameObject(ref objectToSpawn, d);
        D2 = ReturnGameObject(ref objectToSpawn, d2);
        tempList.Add(A); tempList.Add(B); tempList.Add(C); tempList.Add(D);
        tempList.Add(A2); tempList.Add(B2); tempList.Add(C2); tempList.Add(D2);
        CreateLine(A, A2); CreateLine(B, B2); CreateLine(C, C2); CreateLine(D, D2);
        CreateLine(A, B); CreateLine(B, C); CreateLine(C, D); CreateLine(D, A);
        CreateLine(A2, B2); CreateLine(B2, C2); CreateLine(C2, D2); CreateLine(D2, A2);
    }
    
    private void AddFigurePyramid()
    {
        if (objectToSpawn == null) return;

        string sx, sy, sz;
        sx = HLN[0].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sy = HLN[1].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sz = HLN[2].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sx = sx.Substring(0, sx.Length - 1);
        sy = sy.Substring(0, sy.Length - 1);
        sz = sz.Substring(0, sz.Length - 1);
        float x = 0;
        float y = 0;
        float z = 0;
        bool bx = (sx.Length > 0 ? float.TryParse(sx, out x) : false);
        bool by = (sy.Length > 0 ? float.TryParse(sy, out y) : false);
        bool bz = (sz.Length > 0 ? float.TryParse(sz, out z) : false);

        if (!bx || !by || !bz) return;

        byte countPoint = (byte)x;
        float radiusFigure = y;
        float heightFigure = z;

        List<Vector3> vector3List = new List<Vector3>();
        vector3List.Add(new Vector3((float)Math.Sqrt(radiusFigure), 0f, (float)Math.Sqrt(radiusFigure)));
        //vector3List.Add(new Vector3(2f, 0f, 2f));
        vector3List.Add(AddPointCoord(new Vector3(-vector3List[0].x, vector3List[0].y, -vector3List[0].z),
                vector3List[0], 0));
        for (byte i = 2; i < countPoint + 1; i++)
        {
            vector3List.Add(AddPointCoord(new Vector3(-vector3List[0].x, vector3List[0].y, -vector3List[0].z),
                vector3List[0], 360 * (i - 1) / (float)countPoint));
        }
        if (vector3List.Count > 0)
        {
            Vector3 temp = vector3List[0];
            temp.y = heightFigure;
            vector3List[0] = temp;
        }
        List<GameObject> gameObjectList = new List<GameObject>();
        for (byte i = 0; i < vector3List.Count; i++)
        {
            gameObjectList.Add(ReturnGameObject(ref objectToSpawn, vector3List[i]));
            Points.Add(gameObjectList[i]);
            if (i >= 1)
            {
                CreateLine(gameObjectList[i - 1], gameObjectList[i]);
                if (i >= 2)
                {
                    CreateLine(gameObjectList[0], gameObjectList[i]);
                }
            }
        }
        if (gameObjectList.Count >= 2) CreateLine(gameObjectList[1], gameObjectList[gameObjectList.Count - 1]);

    }

    private void AddFigureCylinder()
    {
        if (objectToSpawn == null) return;

        string sx, sy, sz;
        sx = HLN[0].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sy = HLN[1].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sz = HLN[2].GetComponentInChildren<TextMeshProUGUI>().text.ToString();
        sx = sx.Substring(0, sx.Length - 1);
        sy = sy.Substring(0, sy.Length - 1);
        sz = sz.Substring(0, sz.Length - 1);
        float x = 0;
        float y = 0;
        float z = 0;
        bool bx = (sx.Length > 0 ? float.TryParse(sx, out x) : false);
        bool by = (sy.Length > 0 ? float.TryParse(sy, out y) : false);
        bool bz = (sz.Length > 0 ? float.TryParse(sz, out z) : false);

        if (!bx || !by || !bz) return;

        byte countPoint = (byte)x;
        float radiusFigure = y;
        float heightFigure = z;

        List<Vector3> vector3List = new List<Vector3>();
        Vector3 centre = new Vector3((float)Math.Sqrt(radiusFigure), 0f, (float)Math.Sqrt(radiusFigure));
        //vector3List.Add(new Vector3(2f, 0f, 2f));
        for (byte i = 0; i < countPoint; i++)
        {
            vector3List.Add(AddPointCoord(new Vector3(-centre.x, centre.y, -centre.z),
                centre, 360 * (i) / (float)countPoint));
            vector3List.Add(AddPointCoord(new Vector3(-centre.x, centre.y + heightFigure, -centre.z),
                centre, 360 * (i) / (float)countPoint));
        }

        List<GameObject> gameObjectList = new List<GameObject>();
        for (byte i = 1; i < vector3List.Count; i += 2)
        {
            gameObjectList.Add(ReturnGameObject(ref objectToSpawn, vector3List[i - 1]));
            gameObjectList.Add(ReturnGameObject(ref objectToSpawn, vector3List[i]));
            Points.Add(gameObjectList[i - 1]);
            Points.Add(gameObjectList[i]);
            CreateLine(gameObjectList[i - 1], gameObjectList[i]);
            if (i >= 2)
            {
                CreateLine(gameObjectList[i - 2], gameObjectList[i]);
                CreateLine(gameObjectList[i - 3], gameObjectList[i - 1]);
            }
        }
        if (gameObjectList.Count >= 4)
        {
            CreateLine(gameObjectList[0], gameObjectList[gameObjectList.Count - 2]);
            CreateLine(gameObjectList[1], gameObjectList[gameObjectList.Count - 1]);
        }
    }

    private void AddFigure()
    {
        switch (FigureDropDown.value)
        {
            case 0:
                AddFigureSquare();
                break;
            case 1:
                AddFigurePyramid();
                break;
            case 2:
                AddFigureCylinder();
                break;
        }
        
    }

    Vector2 beginTouch, endTouch;
    bool xCoord = false, yCoord = false, zCoord = false;
    private void XCoordsEnabled()
    {
        xCoord = !xCoord;
        xyzCoordsButtons[0].GetComponent<Image>().color = (xCoord) ? selectColor : defaultColor;
    }
    private void YCoordsEnabled()
    {
        yCoord = !yCoord;
        xyzCoordsButtons[1].GetComponent<Image>().color = (yCoord) ? selectColor : defaultColor;
    }
    private void ZCoordsEnabled()
    {
        zCoord = !zCoord;
        xyzCoordsButtons[2].GetComponent<Image>().color = (zCoord) ? selectColor : defaultColor;
    }
    List<Vector3> tempMove = new List<Vector3>();
    private void MovePoint()
    {
        if (selectedObject.Count == 0 || selectedObject.Count > 1 && maxSelectedObject != -3)
            return;
        if (xCoord && yCoord && zCoord || !xCoord && !yCoord && !zCoord) return;
        if (Input.touchCount == 1)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            float x = 0, y = 0, z = 0;
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                for (int i = 0; i < selectedObject.Count; i++)
                    tempMove.Add(selectedObject[i].transform.localPosition);
                beginTouch = touch.position;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                endTouch = touch.position;
                float k = 10000;
                if (xCoord && yCoord)

                { // xy
                    y = (endTouch.y - beginTouch.y) / k;
                    x = (endTouch.x - beginTouch.x) / k;
                }
                else if (yCoord && zCoord)
                {// yz

                    y = (endTouch.y - beginTouch.y) / k;
                    z = (endTouch.x - beginTouch.x) / k;
                }
                else if (zCoord && xCoord)
                { // xz
                    z = (endTouch.y - beginTouch.y) / k;
                    x = (endTouch.x - beginTouch.x) / k;
                }
                else if (xCoord) x = (endTouch.x - beginTouch.x) / k;
                else if (yCoord) y = (endTouch.y - beginTouch.y) / k;
                else if (zCoord) z = (endTouch.x - beginTouch.x) / k;
                for (int i = 0; i < selectedObject.Count && i < tempMove.Count; i++)
                    if (selectedObject[i].GetComponent<PointS>().GetLines()) selectedObject[i].GetComponent<PointS>().Moved(tempMove[i], new Vector3(x, y, z));
                    else selectedObject[i].transform.localPosition = tempMove[i] + new Vector3(x, y, z);
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                tempMove.Clear();
        }
    }
    private void CountSelectedObject()
    {
        if (maxSelectedObject != 3)
        {
            maxSelectedObject = (short)((maxSelectedObject) % 3 + 1);
            MaxSelectedButton.GetComponentInChildren<TextMeshProUGUI>().text = "x" + maxSelectedObject.ToString();
        }
        else
        {
            maxSelectedObject = -3;
            MaxSelectedButton.GetComponentInChildren<TextMeshProUGUI>().text = "xN";
        }
        if (maxSelectedObject == 1)
            ClearSelectedObject(maxSelectedObject);
    }
    private void DefailtObjects()
    {
        bool aLPB = selectedObject.Count >= 2 && maxSelectedObject != -3;
        addLinePlaneButton.gameObject.SetActive(aLPB);
        addLinePlaneButton.GetComponentInChildren<TextMeshProUGUI>().text = (selectedObject.Count <= 2) ? ("Add Line") : ("Add Plane");
        destroyPointButton.GetComponentInChildren<TextMeshProUGUI>().text = (selectedObject.Count <= 1) ? ("Delete Point") : ("Clear select");
        destroyPointButton.gameObject.SetActive(selectedObject.Count >= 1);

        deletePointsButton.gameObject.SetActive(Points.Count > 0);

        if (HLNText.Length == 3)
        {
            HLNText[0].GetComponentInChildren<TextMeshProUGUI>().text = (FigureDropDown.value==0)?("Length"):("Points");
            HLNText[1].GetComponentInChildren<TextMeshProUGUI>().text = (FigureDropDown.value == 0) ? ("Width") : ("Radius");
            //HLN[2].GetComponentInChildren<TextMeshProUGUI>().text = "Height";
        }
    }

    private void EmptyF()
    {
        if (Input.touchCount == 1)
            if (Input.GetTouch(0).phase == TouchPhase.Began)
                TMPro3.text = "Began";
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                TMPro3.text = "Moved";
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                TMPro3.text = "Ended";
    }
    // Start is called before the first frame update
    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        rayCastManager = arOrigin.GetComponent<ARRaycastManager>();
        deletePointsButton.onClick.AddListener(ClearPoints);
        destroyPointButton.onClick.AddListener(DeletePoint);
        showPointsButton.onClick.AddListener(ShowPoints);
        showVectorsButton.onClick.AddListener(ShowVectors);
        addLinePlaneButton.onClick.AddListener(CreateLinePlane);
        heightSliderButton.onClick.AddListener(UpYDefault);
        sizeSliderButton.onClick.AddListener(ScaleSliderObjectDefault);
        addObjectButtons[0].onClick.AddListener(AddFigure);
        xyzCoordsButtons[0].onClick.AddListener(XCoordsEnabled);
        xyzCoordsButtons[1].onClick.AddListener(YCoordsEnabled);
        xyzCoordsButtons[2].onClick.AddListener(ZCoordsEnabled);
        MaxSelectedButton.onClick.AddListener(CountSelectedObject);
        //buttons[0].onClick.AddListener(TFMovePlace);
        //buttons[1].onClick.AddListener(TFRotationPlace);
        defaultHeight = heightSlider.value;
        defaultScale = scaleSlider.value;
        UpYDefault();
        ScaleSliderObjectDefault();
        foreach (var i in xyzCoordsButtons)
            i.GetComponent<Image>().color = (xCoord) ? selectColor : defaultColor;
        MaxSelectedButton.GetComponentInChildren<TextMeshProUGUI>().text = "x" + maxSelectedObject.ToString();
        telephone = SystemInfo.deviceType == DeviceType.Handheld; // Определяет устройство

    }
    // Update is called once per frame
    void Update()
    {
        //FigureDropDown.value = dDown.value;
        if (telephone) UpdatePlacementPose();
        DefailtObjects();
        RotateAndMovePlace();
        MovePoint();
        //if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        //    TFMovePlace();
        if (Input.touchCount == 2)// && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            TFMovePlace();
        ScaleSliderObject();
        if (selectedObject.Count > 0)
        {
            //TMPro1.text = "X: " + selectedObject[0].transform.localPosition.x.ToString() + ", Y: " + selectedObject[0].transform.localPosition.y.ToString() + ", Z: " + selectedObject[0].transform.localPosition.z.ToString();
            //if (selectedObject.Count > 1) TMPro2.text = "X: " + selectedObject[1].transform.localPosition.x.ToString() + ", Y: " + selectedObject[1].transform.localPosition.y.ToString() + ", Z: " + selectedObject[1].transform.localPosition.z.ToString();
        }
        //TMPro2.text = "X: " + placementIndicator.transform.position.x.ToString() + ", Y: " + placementIndicator.transform.position.y.ToString() + ", Z: " + placementIndicator.transform.position.z.ToString();
        //TMPro1.text = "Rotation: " + placementIndicator.transform.eulerAngles.y.ToString();
    }
    bool telephone = true;
}
