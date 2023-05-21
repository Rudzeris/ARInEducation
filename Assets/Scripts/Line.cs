using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class Line : MonoBehaviour
{

    public GameObject startPoint; // Начальная точка линии
    public GameObject endPoint; // Конечная точка линии
    public float lineWidth = 0.01f; // Толщина линии
    public Color lineColor = Color.white; // Цвет линии
    public Material lineMaterial;
    private LineRenderer lineRenderer;
    public void CreateLine()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // Получаем компонент LineRenderer или добавляем его, если его нет
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        // Настройка параметров LineRenderer
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.materials[0]=lineMaterial;
    }

    public void UpdateLine()
    {
        // Устанавливаем начальную и конечную точки линии
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
