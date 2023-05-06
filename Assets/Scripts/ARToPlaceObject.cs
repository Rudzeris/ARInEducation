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

public class ARToPlaceObject : MonoBehaviour
{

    //public GameObject objectToPlace;
    public GameObject placementIndicator;
    public GameObject[] Text;

    private ARSessionOrigin arOrigin;
    private ARRaycastManager rayCastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private bool placementActive = true;

    private Vector2 TouchPosition;

    private Camera ARCamera;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public Button[] buttons;
    public bool bRotation=false;

    private Quaternion yRotation;

    private GameObject selectedObject1;
    private GameObject selectedObject2;
    private List<Line> lines = new List<Line>();

    public void SpawnObject(GameObject objectToSpawn,Vector3 pose,Quaternion A)
    {
        if (objectToSpawn!=null)
        {
            // Получаем Transform компонент объекта ToPlace
            Transform toPlaceTransform = placementIndicator.transform;
            // Создаем новый экземпляр objectToPlace
            GameObject newObject = Instantiate(objectToSpawn, pose, A);
            newObject.transform.parent = toPlaceTransform;
            newObject.tag = "Point";
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

    private void TFRotationPlace()
    {
        if (bRotation) bRotation = false;
        else bRotation = true;
        buttons[1].GetComponentInChildren<TextMeshProUGUI>().text = ((bRotation) ? ("Закрепить") : ("Вращать"));
    }

    private void CreateLine(GameObject startObject, GameObject endObject)
    {
        GameObject lineObject = new GameObject("Line");
        Line line = lineObject.AddComponent<Line>();
        line.startObject = startObject;
        line.endObject = endObject;
        line.UpdateLine();
        lines.Add(line);
    }


    void RotateAndMovePlace()
    {
        if (placementActive) UpdatePlacementIndicator();
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            TouchPosition = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = ARCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;

                if (Physics.Raycast(ray, out hitObject))
                {
                    if (hitObject.collider.CompareTag("Point"))
                    {
                        if (selectedObject1 == null)
                        {
                            selectedObject1 = hitObject.collider.gameObject;
                        }
                        else if (selectedObject2 == null && selectedObject1 != hitObject.collider.gameObject)
                        {
                            selectedObject2 = hitObject.collider.gameObject;
                            // создание линии между двумя выделенными объектами Point
                            CreateLine(selectedObject1, selectedObject2);
                            // сброс выделения
                            selectedObject1 = null;
                            selectedObject2 = null;
                        }
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && Input.touchCount == 1)
            {
                if (bRotation)
                {
                    yRotation = Quaternion.Euler(0f, touch.deltaPosition.x * 0.1f, 0f);
                    placementIndicator.transform.rotation *= yRotation;
                }
            }
        }
    }


    //void RotateAndMovePlace()
    //{


    //    if (placementActive) UpdatePlacementIndicator();
    //    if (Input.touchCount > 0)
    //    {   
    //        Touch touch = Input.GetTouch(0);
    //        TouchPosition = touch.position;
    //        //if (touch.phase == TouchPhase.Began)
    //        //{
    //        //    Ray ray = ARCamera.ScreenPointToRay(touch.position);
    //        //    RaycastHit hitObject;

    //        //    if(Physics.Raycast(ray,out hitObject))
    //        //    {
    //        //        if (hitObject.collider.CompareTag("UnSelected"))
    //        //        {
    //        //            hitObject.collider.gameObject.tag = "Selected";
    //        //        }
    //        //    }
    //        //}
    //        if(touch.phase == TouchPhase.Moved && Input.touchCount == 1)
    //        {
    //            if (bRotation)
    //            {
    //                yRotation = Quaternion.Euler(0f, touch.deltaPosition.x * 0.1f, 0f);
    //                placementIndicator.transform.rotation *= yRotation;
    //            }
    //        }
    //    }
    //}

    void TFMovePlace()
    {
        if (placementActive == false) placementActive = true;
        else placementActive = false;
        buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = ((placementActive) ? ("Закрепить\nоси") : ("Передвинуть\nоси"));
    }


    // Start is called before the first frame update
    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        rayCastManager = arOrigin.GetComponent<ARRaycastManager>();
        buttons[0].onClick.AddListener(TFMovePlace);
        buttons[1].onClick.AddListener(TFRotationPlace);
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        RotateAndMovePlace();
    }
}
