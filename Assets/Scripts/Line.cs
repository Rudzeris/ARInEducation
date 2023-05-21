using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class Line : MonoBehaviour
{

    public GameObject startPoint; // ��������� ����� �����
    public GameObject endPoint; // �������� ����� �����
    public float lineWidth = 0.01f; // ������� �����
    public Color lineColor = Color.white; // ���� �����
    public Material lineMaterial;
    private LineRenderer lineRenderer;
    public void CreateLine()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // �������� ��������� LineRenderer ��� ��������� ���, ���� ��� ���
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        // ��������� ���������� LineRenderer
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.materials[0]=lineMaterial;
    }

    public void UpdateLine()
    {
        // ������������� ��������� � �������� ����� �����
        if (startPoint != null && endPoint != null)
        {
            lineRenderer.SetPosition(0, startPoint.transform.position);
            lineRenderer.SetPosition(1, endPoint.transform.position);
        }
    }
    public void OnDestroy()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        //CreateLine();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLine();
    }
}
