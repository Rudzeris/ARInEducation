using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using Unity.VisualScripting;
using UnityEngine;

public class PointS : MonoBehaviour
{
    public List<GameObject> lines = new List<GameObject>();
    private int x, y, z;
    GameObject PA, PB;
    Vector3 pointA, pointB;
    public void Moved(Vector3 begin, Vector3 pointC)
    {
        if (PA && PB)
        {
            this.pointA = PA.transform.localPosition;
            this.pointB = PB.transform.localPosition;
        }
        float max = Mathf.Max(Math.Abs(pointC.x), Math.Abs(pointC.y), Math.Abs(pointC.z));
        
        // »змен€ем другие координаты пропорционально
        if (Math.Abs(pointC.x) == max)
        {
            pointC += begin;
            if (pointB.y - pointA.y != 0)
                pointC.y = pointA.y + (pointB.y - pointA.y) / (pointB.x - pointA.x) * (pointC.x - pointA.x);
            if (pointB.z - pointA.z != 0)
                pointC.z = pointA.z + (pointB.z - pointA.z) / (pointB.x - pointA.x) * (pointC.x - pointA.x);
        }
        else if (Math.Abs(pointC.y) == max)
        {
            pointC += begin;
            if (pointB.x - pointA.x != 0)
                pointC.x = pointA.x + (pointB.x - pointA.x) / (pointB.y - pointA.y) * (pointC.y - pointA.y);
            if (pointB.z - pointA.z != 0)
                pointC.z = pointA.z + (pointB.z - pointA.z) / (pointB.y - pointA.y) * (pointC.y - pointA.y);
        }
        else if (Math.Abs(pointC.z) == max)
        {
            pointC += begin;
            if (pointB.x - pointA.x != 0)
                pointC.x = pointA.x + (pointB.x - pointA.x) / (pointB.z - pointA.z) * (pointC.z - pointA.z);
            if (pointB.y - pointA.y != 0)
                pointC.y = pointA.y + (pointB.y - pointA.y) / (pointB.z - pointA.z) * (pointC.z - pointA.z);
        }
        //else if (Math.Abs(pointC.x) == max)
        //{
        //    pointC.y = pointC.z = 0;
        //}
        //else if (Math.Abs(pointC.y) == max)
        //{
        //    pointC.z = pointC.x = 0;
        //}
        //else if (Math.Abs(pointC.z) == max)
        //{
        //    pointC.x = pointC.y = 0;
        //}
        else
        {
            return;
        }
        transform.localPosition = pointC;
    }
    public bool GetLines()
    {
        return PA && PB;
    }
    public void setLineCoords(GameObject pointA, GameObject pointB)
    {
        // ¬ычисл€ем разницу между координатами точек A и B
        this.PA = pointA;
        this.PB = pointB;
        this.pointA = PA.transform.localPosition;
        this.pointB = PB.transform.localPosition;

        // ѕровер€ем, кака€ координата имеет наибольшую разницу по модулю
    }
    public void OnDestroy()
    {
        ARToPlaceObject artpo = FindObjectOfType<ARToPlaceObject>();
        foreach (var i in lines)
        {
            if (i != null)
            {
                artpo.deleteLine(i.gameObject);
                Destroy(i.gameObject);
            }
        }
        artpo.deletePoint(gameObject);
        Destroy(gameObject);
    }
    public void removeLine(GameObject temp)
    {
        lines.Remove(temp);
    }
}
