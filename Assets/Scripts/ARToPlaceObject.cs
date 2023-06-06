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

public class ARToPlaceObject : MonoBehaviour
{

    //public GameObject objectToPlace;
    [SerializeField] public GameObject placementIndicator;
    [SerializeField] public GameObject[] vectorList;
    [SerializeField] public GameObject[] Text;
    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] public Button deletePointsButton;
    [SerializeField] public Button destroyPointButton;
    [SerializeField] public Button showPointsButton;
    [SerializeField] public Button showVectorsButton;
    [SerializeField] public Button addLineButton;
    [SerializeField] public Button[] addObjectButtons;
    [SerializeField] public Button[] xyzCoordsButtons;
    [SerializeField] public UnityEngine.UI.Slider heightSlider;
    [SerializeField] public UnityEngine.UI.Slider scaleSlider;
    [SerializeField] public Button heightSliderButton;
    [SerializeField] public Button sizeSliderButton;
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

    private GameObject selectedObject1;
    private GameObject selectedObject2;
    private List<GameObject> lines = new List<GameObject>();
    private List<GameObject> Points = new List<GameObject>();


    [SerializeField] public TextMeshProUGUI TMPro1;
    [SerializeField] public TextMeshProUGUI TMPro2;
    [SerializeField] public TextMeshProUGUI TMPro3;

    float defaultHeight = 1;
    float defaultScale = 1;

    private bool SearchObject(Vector3 temp)
    {
        float q = 0, w = 0;
        foreach (var i in Points)
        {
            if (Vector3.Distance(temp, i.transform.position) < eps / defaultScale * scaleSlider.value) return true;
        }
        return false;
    }

    private Vector3 AddPointCoord(Vector3 pose, Transform toPlaceTransform)
    {
        pose /= 50;
        //pose = pose / defaultScale * scaleSlider.value;
        double rotation = -toPlaceTransform.eulerAngles.y;
        float x = 0;
        x += pose.x;
        float x1 = toPlaceTransform.transform.position.x;
        float z = 0;
        z += pose.z;
        float z1 = toPlaceTransform.transform.position.z;
        double r = Math.Sqrt(x * x + z * z);
        double t = Math.Atan2(z, x);
        double t2 = t + rotation / 180.0 * Math.PI;
        double x2 = (double)x1 + r * Math.Cos(t2) * (toPlaceTransform.localScale.y / defaultScale);
        double z2 = (double)z1 + r * Math.Sin(t2) * (toPlaceTransform.localScale.y / defaultScale);
        pose.x = (float)x2;
        //pose.x = x + x1;
        //pose.z = z + z1;
        pose.z = (float)z2;
        pose.y = pose.y / defaultScale * scaleSlider.value + toPlaceTransform.transform.position.y;
        return pose;
    }

    public void SpawnObject(GameObject oToSpawn, Vector3 pose, Quaternion A)
    {
        if (oToSpawn != null)
        {
            Transform toPlaceTransform = placementIndicator.transform;
            pose = AddPointCoord(pose, toPlaceTransform);
            if (!SearchObject(pose))
            {
                GameObject newObject = Instantiate(oToSpawn, pose, A, toPlaceTransform);
                Points.Add(newObject);
                newObject.tag = "Point";
                if (newObject.GetComponent<MeshRenderer>() == null)
                {
                    newObject.AddComponent<MeshRenderer>();
                }
                newObject.GetComponent<MeshRenderer>().enabled = PointsEnabled;
                TMPro1.text = "X: " + pose.x.ToString() + ", Y: " + pose.y.ToString() + ", Z: " + pose.z.ToString();
                newObject.AddComponent<PointS>();
                //newObject.transform.localScale = newObject.transform.localScale / defaultScale * scaleSlider.value;
                //TMPro1.text = "X: " + globalPosition.x.ToString() + ", Y: " + globalPosition.y.ToString() + ", Z: " + globalPosition.z.ToString();

            }
        }
    }

    private void DeletePoint()
    {
        if (selectedObject1 != null && selectedObject2 == null)
        {
            Destroy(selectedObject1);
        }
    }

    private void ClearPoints()
    {
        foreach (var i in Points)
        {
            Destroy(i);
        }
        Points.Clear();
        foreach (var i in lines)
        {
            Destroy(i);
        }
        lines.Clear();
    }

    public void AddPointSelect(GameObject temp)
    {
        if (temp == null) return;

        if (selectedObject1 == temp || selectedObject2 == temp)
        {
            if (selectedObject2 == temp)
            {
                selectedObject2.GetComponent<Renderer>().material.color = defaultColor;
                selectedObject2 = null;
            }
            else
            {
                selectedObject1.GetComponent<Renderer>().material.color = defaultColor;
                selectedObject1 = selectedObject2;
                selectedObject2 = null;
            }
        }
        else
        {
            if (selectedObject2 != null)
                selectedObject2.GetComponent<Renderer>().material.color = defaultColor;
            selectedObject2 = selectedObject1;
            selectedObject1 = temp;
            selectedObject1.GetComponent<Renderer>().material.color = selectColor;
        }
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
    private void CreateLine()
    {
        CreateLine(selectedObject1, selectedObject2);
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
    private void CreateLine(GameObject selectedObject1 = null, GameObject selectedObject2 = null)
    {
        if (selectedObject1 != null && selectedObject2 != null)
        {
            if (!SearchLine(ref selectedObject1, ref selectedObject2)) return;
            GameObject lineObject = new GameObject("Line");

            Line line = lineObject.AddComponent<Line>();

            line.startPoint = selectedObject1;
            line.endPoint = selectedObject2;

            PointS pointScript1 = selectedObject1.GetComponent<PointS>();
            PointS pointScript2 = selectedObject2.GetComponent<PointS>();

            pointScript1.lines.Add(lineObject);
            pointScript2.lines.Add(lineObject);

            line.CreateLine();
            line.UpdateLine();

            lineObject.transform.SetParent(placementIndicator.transform);

            lines.Add(lineObject);
        }
    }


    private Color defaultColor = Color.white;
    private Color selectColor = Color.red;
    private void RotateAndMovePlace()
    {
        if (telephone) 
            if (placementActive) UpdatePlacementIndicator();
            else UpY(); // move and Y
        if (Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (selectedObject1 == null || xCoord && yCoord && zCoord || !xCoord && !yCoord && !zCoord)
                if (touch.phase == TouchPhase.Moved && Input.touchCount == 1)
                {
                    yRotation = Quaternion.Euler(0f, -touch.deltaPosition.x * 0.1f, 0f);
                    placementIndicator.transform.rotation *= yRotation;
                }
        }
    }

    public float doubleClickTime = 0.3f;
    private float lastClickTime;
    private void TFMovePlace()
    {
        if (Time.time - lastClickTime < doubleClickTime)
        {
            placementActive = !placementActive;

            //TMPro3.text = "Move: " + ((placementActive) ? "on" : "off");
        }
        lastClickTime = Time.time;
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
    }

    private GameObject ReturnGameObject(ref GameObject temp, Vector3 V)
    {
        Quaternion t = new Quaternion(0f, 0f, 0f, 0f);
        SpawnObject(temp, V, t);
        return Points[Points.Count - 1];
    }
    //Стандартные фигуры
    private void AddFigureSquare()
    {
        if (objectToSpawn == null) return;
        float weight = 3, length = 5;
        Vector3 begin = new Vector3(0f, 0f, 0f);
        Vector3 a, a2, b, b2, c, c2, d, d2;
        float h = 0;
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
    private void VisibleButton()
    {
        addLineButton.gameObject.SetActive(selectedObject1 != null && selectedObject2 != null);
        destroyPointButton.gameObject.SetActive(selectedObject1 != null && selectedObject2 == null);
        deletePointsButton.gameObject.SetActive(Points.Count > 0);
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
    private void MovePoint()
    {
        if (selectedObject2 != null || selectedObject1 == null)
            return;
        if (xCoord && yCoord && zCoord || !xCoord && !yCoord && !zCoord) return;
        if (Input.touchCount == 1)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            float x = 0, y = 0, z = 0;
            Vector3 newPosition = selectedObject1.transform.position;
            x = 0; y = 0; z = 0;
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                beginTouch = touch.position;

            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                endTouch = touch.position;
                float k = 2000;
                if (xCoord && yCoord)

                { // xy
                    y += (endTouch.y - beginTouch.y) / k;
                    x += (endTouch.x - beginTouch.x) / k;
                }
                else if (yCoord && zCoord)
                {// yz

                    y += (endTouch.y - beginTouch.y) / k;
                    z += (endTouch.x - beginTouch.x) / k;
                }
                else if (zCoord && xCoord)
                { // xz
                    z += (endTouch.y - beginTouch.y) / k;
                    x += (endTouch.x - beginTouch.x) / k;
                }else if(xCoord) x += (endTouch.x - beginTouch.x) / k;
                else if(yCoord) y += (endTouch.y - beginTouch.y) / k;
                else if(zCoord) z += (endTouch.x - beginTouch.x) / k;

                newPosition += AddPointCoord(new Vector3(x, y, z), placementIndicator.transform);
                selectedObject1.transform.position = newPosition;
            }
        }
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
        addLineButton.onClick.AddListener(CreateLine);
        heightSliderButton.onClick.AddListener(UpYDefault);
        sizeSliderButton.onClick.AddListener(ScaleSliderObjectDefault);
        addObjectButtons[0].onClick.AddListener(AddFigureSquare);
        xyzCoordsButtons[0].onClick.AddListener(XCoordsEnabled);
        xyzCoordsButtons[1].onClick.AddListener(YCoordsEnabled);
        xyzCoordsButtons[2].onClick.AddListener(ZCoordsEnabled);
        //buttons[0].onClick.AddListener(TFMovePlace);
        //buttons[1].onClick.AddListener(TFRotationPlace);
        defaultHeight = heightSlider.value;
        defaultScale = scaleSlider.value;
        UpYDefault();
        ScaleSliderObjectDefault();
        foreach (var i in xyzCoordsButtons)
            i.GetComponent<Image>().color = (xCoord) ? selectColor : defaultColor;
    }
    // Update is called once per frame
    void Update()
    {
        if(telephone)UpdatePlacementPose();
        VisibleButton();
        RotateAndMovePlace();
        MovePoint();
        //if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        //    TFMovePlace();
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Ended)// && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            TFMovePlace();
        ScaleSliderObject();
        if (Points.Count > 0)
        TMPro2.text = "X: " + Points[0].transform.localPosition.x.ToString() + ", Y: " + Points[0].transform.localPosition.y.ToString() + ", Z: " + Points[0].transform.localPosition.z.ToString();
        //TMPro2.text = "X: " + placementIndicator.transform.position.x.ToString() + ", Y: " + placementIndicator.transform.position.y.ToString() + ", Z: " + placementIndicator.transform.position.z.ToString();
        //TMPro1.text = "Rotation: " + placementIndicator.transform.eulerAngles.y.ToString();
    }
    bool telephone = false;
}
