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

public class ARToPlaceObject : MonoBehaviour
{

    //public GameObject objectToPlace;
    [SerializeField] public GameObject placementIndicator;
    [SerializeField] public GameObject[] Text;
    [SerializeField] public Button deletePointsButton;

    private ARSessionOrigin arOrigin;
    private ARRaycastManager rayCastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = true;
    private bool placementActive = false;

    private Vector2 TouchPosition;

    private Camera ARCamera;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    //public Button[] buttons;
    //public bool bRotation = false;

    private Quaternion yRotation;

    private GameObject selectedObject1;
    private GameObject selectedObject2;
    private List<Line> lines = new List<Line>();
    private List<GameObject> Points = new List<GameObject>();


    [SerializeField] public TextMeshProUGUI TMPro1;
    [SerializeField] public TextMeshProUGUI TMPro2;
    [SerializeField] public TextMeshProUGUI TMPro3;

    public void SpawnObject(GameObject objectToSpawn, Vector3 pose, Quaternion A)
    {
        if (objectToSpawn != null)
        {
            // Получаем Transform компонент объекта ToPlace
            Transform toPlaceTransform = placementIndicator.transform;
            pose /= 50;
            double rotation = -toPlaceTransform.eulerAngles.y;
            float x = pose.x;
            float x1 = toPlaceTransform.transform.position.x;
            float z = pose.z;
            float z1 = toPlaceTransform.transform.position.z;
            double r = Math.Sqrt(x * x + z * z);
            double t = Math.Atan2(x, z);
            double t2 = t + rotation / 180.0 * Math.PI;
            double x2 = (double)x1 + r * Math.Cos(t2);
            double z2 = (double)z1 + r * Math.Sin(t2);
            pose.x = (float)z2;
            pose.z = (float)x2;
            pose.y += toPlaceTransform.transform.position.y;
            //Vector3 globalPosition = toPlaceTransform.TransformPoint(pose);
            // Создаем новый экземпляр objectToPlace
            //GameObject newObject = Instantiate(objectToSpawn, pose/50, A,toPlaceTransform);
            GameObject newObject = Instantiate(objectToSpawn, pose, A, toPlaceTransform);
            newObject.tag = "Point";
            Points.Add(newObject);
            //TMPro1.text = "X: " + pose.x.ToString() + ", Y: " + pose.y.ToString() + ", Z: " + pose.z.ToString();
            //TMPro1.text = "X: " + globalPosition.x.ToString() + ", Y: " + globalPosition.y.ToString() + ", Z: " + globalPosition.z.ToString();
        }
    }

    private void ClearPoints()
    {
        foreach (var i in Points)
        {
            Destroy(i);
        }
        Points.Clear();
    }

    public void AddPointSelect(GameObject temp)
    {
        if (temp==null) return;

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
            if(selectedObject2!=null) 
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
        {
            placementIndicator.SetActive(false);
        }
    }

    private void PlaceObject()
    {
        //placementIndicator.transform.position = placementPose.position;
    }

    //private void TFRotationPlace()
    //{
    //    if (bRotation) bRotation = false;
    //    else bRotation = true;
    //    buttons[1].GetComponentInChildren<TextMeshProUGUI>().text = ((bRotation) ? ("Закрепить") : ("Вращать"));
    //}

    private void CreateLine(GameObject startObject, GameObject endObject)
    {
        GameObject lineObject = new GameObject("Line");
        Line line = lineObject.AddComponent<Line>();
        line.startObject = startObject;
        line.endObject = endObject;
        line.UpdateLine();
        lines.Add(line);
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
                    yRotation = Quaternion.Euler(0f, touch.deltaPosition.x * 0.1f, 0f);
                    placementIndicator.transform.rotation *= yRotation;
                }
            }
        }
    }

    void TFMovePlace()
    {
        if (placementActive == false) placementActive = true;
        else placementActive = false;
        TMPro1.text = "Move: " + ((placementActive) ? "on" : "off");
        //buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = ((placementActive) ? ("Закрепить\nоси") : ("Передвинуть\nоси"));
    }


    // Start is called before the first frame update
    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        rayCastManager = arOrigin.GetComponent<ARRaycastManager>();
        deletePointsButton.onClick.AddListener(ClearPoints);
        //buttons[0].onClick.AddListener(TFMovePlace);
        //buttons[1].onClick.AddListener(TFRotationPlace);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdatePlacementPose();
        RotateAndMovePlace();
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Ended)// && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            TFMovePlace();


        //TMPro2.text = "X: " + placementIndicator.transform.position.x.ToString() + ", Y: " + placementIndicator.transform.position.y.ToString() + ", Z: " + placementIndicator.transform.position.z.ToString();
        //TMPro3.text = "Rotation: " + placementIndicator.transform.eulerAngles.y.ToString();
    }
}
