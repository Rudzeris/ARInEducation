using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{

    public GameObject startObject;
    public GameObject endObject;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void UpdateLine()
    {
        if (startObject != null && endObject != null)
        {
            lineRenderer.SetPosition(0, startObject.transform.position);
            lineRenderer.SetPosition(1, endObject.transform.position);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
