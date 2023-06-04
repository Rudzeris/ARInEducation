using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointS : MonoBehaviour
{
    public List<GameObject> lines = new List<GameObject>();
    public void OnDestroy()
    {
        ARToPlaceObject artpo = FindObjectOfType<ARToPlaceObject>();
        foreach (var i in lines)
        {
            artpo.deleteLine(i.gameObject);
            Destroy(i.gameObject);
        }
        artpo.deletePoint(gameObject);
        Destroy(gameObject);
    }
}
