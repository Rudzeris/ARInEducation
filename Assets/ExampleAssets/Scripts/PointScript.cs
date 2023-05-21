using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Renderer cubeRenderer;
    private ARToPlaceObject arToPlaceObject;

    void Start()
    {
        arToPlaceObject = FindObjectOfType<ARToPlaceObject>();
    }

    void OnMouseDown()
    {
        if (arToPlaceObject != null)
        {
            arToPlaceObject.AddPointSelect(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
