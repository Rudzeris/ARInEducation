using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using UnityEngine.XR.OpenXR.Input;
using Pose = UnityEngine.Pose;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;

public class ARToPlaceObject : MonoBehaviour
{

    //public GameObject objectToPlace;
    [SerializeField] public GameObject placementIndicator;
    [SerializeField] public GameObject[] vectorList;
    [SerializeField] public GameObject[] Text;
    [SerializeField] public Button deletePointsButton;
    [SerializeField] public Button destroyPointButton;
    [SerializeField] public Button showPointsButton;
    [SerializeField] public Button showVectorsButton;
    [SerializeField] public Button addLineButton;
    [SerializeField] public UnityEngine.UI.Slider heightSlider;
    [SerializeField] public UnityEngine.UI.Slider scaleSlider;
    [SerializeField] public Button heightSliderButton;
    [SerializeField] public Button sizeSliderButton;
    private float eps = 0.001f;

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

    bool SearchObject(Vector3 temp)
    {
        float q = 0, w = 0;
        foreach (var i in Points)
        {
            if (Vector3.Distance(temp, i.transform.position) < eps) return true;
        }
        return false;
    }

    public void SpawnObject(GameObject objectToSpawn, Vector3 pose, Quaternion A)
    {
        if (objectToSpawn != null)
        {
            // Получаем Transform компонент объекта ToPlace
            Transform toPlaceTransform = placementIndicator.transform;
            pose /= 50;
            double rotation = -toPlaceTransform.eulerAngles.y;
            float x = 0;
            x+= pose.x;
            float x1 = toPlaceTransform.transform.position.x;
            float z = 0;
            z += pose.z;
            float z1 = toPlaceTransform.transform.position.z;
            double r = Math.Sqrt(x * x + z * z);
            double t = Math.Atan2(z, x);
            double t2 = t + rotation / 180.0 * Math.PI;
            double x2 = (double)x1 + r * Math.Cos(t2)*(toPlaceTransform.localScale.y/5);
            double z2 = (double)z1 + r * Math.Sin(t2)*(toPlaceTransform.localScale.y/5);
            pose.x = (float)x2;
            //pose.x = x + x1;
            //pose.z = z + z1;
            pose.z = (float)z2;
            pose.y += toPlaceTransform.transform.position.y;

            //Vector3 globalPosition = toPlaceTransform.TransformPoint(pose);
            // Создаем новый экземпляр objectToPlace
            //GameObject newObject = Instantiate(objectToSpawn, pose/50, A,toPlaceTransform);
            if (!SearchObject(pose))
            {
                GameObject newObject = Instantiate(objectToSpawn, pose, A, toPlaceTransform);
                newObject.tag = "Point";
                newObject.AddComponent<PointS>();
                newObject.GetComponent<MeshRenderer>().enabled = PointsEnabled;
                Points.Add(newObject);
                TMPro1.text = "X: " + pose.x.ToString() + ", Y: " + pose.y.ToString() + ", Z: " + pose.z.ToString();
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

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            //placementIndicator.transform.SetPositionAndRotation(placementPose.position,placementPose.rotation);
            placementIndicator.transform.position = placementPose.position;
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
        if (selectedObject1 != null && selectedObject2 != null)
        {
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
    void RotateAndMovePlace()
    {
        //if (placementActive) UpdatePlacementIndicator(); // move
        if (Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved && Input.touchCount == 1)
            {
                if (true)//bRotation)
                {
                    yRotation = Quaternion.Euler(0f, -touch.deltaPosition.x * 0.1f, 0f);
                    placementIndicator.transform.rotation *= yRotation;
                }
            }
        }
    }

    public float doubleClickTine = 0.3f;
    private float lastClickTime;
    void TFMovePlace()
    {
        if (Time.time - lastClickTime < doubleClickTine)
        {
            placementActive = !placementActive;

            TMPro3.text = "Move: " + ((placementActive) ? "on" : "off");
        }
        lastClickTime = Time.time;
        //buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = ((placementActive) ? ("Закрепить\nоси") : ("Передвинуть\nоси"));
    }

    bool PointsEnabled = true;
    void ShowPoints()
    {
        PointsEnabled = !PointsEnabled;
        foreach (var i in Points)
            i.GetComponent<MeshRenderer>().enabled = PointsEnabled;
    }
    bool VectorEnabled = true;
    void ShowVectors()
    {
        VectorEnabled = !VectorEnabled;
        foreach (var i in vectorList)
            i.GetComponent<MeshRenderer>().enabled = VectorEnabled;
    }

    void UpYDefault()
    {
        heightSlider.value = 0.35f;
    }
    void UpY()
    {
        float height = heightSlider.value;
        placementIndicator.transform.position = new Vector3(placementIndicator.transform.position.x, height-0.35f, placementIndicator.transform.position.z);
    }

    void ScaleSliderObjectDefault()
    {
        scaleSlider.value = 5;
    }
    void ScaleSliderObject()
    {
        float scale = scaleSlider.value;
        placementIndicator.transform.localScale = new Vector3(scale, scale, scale);
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
        //buttons[0].onClick.AddListener(TFMovePlace);
        //buttons[1].onClick.AddListener(TFRotationPlace);
    }
    // Update is called once per frame
    void Update()
    {
        //UpdatePlacementPose();
        RotateAndMovePlace();
        //if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        //    TFMovePlace();
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Ended)// && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            TFMovePlace();
        UpY();
        ScaleSliderObject();
        TMPro2.text = "X: " + placementIndicator.transform.position.x.ToString() + ", Y: " + placementIndicator.transform.position.y.ToString() + ", Z: " + placementIndicator.transform.position.z.ToString();
        //TMPro1.text = "Rotation: " + placementIndicator.transform.eulerAngles.y.ToString();
    }
}
