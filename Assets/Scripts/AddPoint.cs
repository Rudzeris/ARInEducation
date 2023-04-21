using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.UI;

public class AddPoint : MonoBehaviour
{
    Button button;
    Vector3 pose;
    [SerializeField] private TextMeshProUGUI[] coords;
    [SerializeField] private GameObject spawn;
    [SerializeField] private GameObject objectToSpawn;
    //InputField InF;

    void AddPointToPlace()
    {
        //button.GetComponentInChildren<TextMeshProUGUI>().text = texts[0].text;
        //button.GetComponentInChildren<TextMeshProUGUI>().text;
        string sx, sy, sz;
        sx=coords[0].text.ToString();
        sy = coords[1].text.ToString();
        sz = coords[2].text.ToString();
        int x = int.Parse(sx);
        //pose.x = x; pose.y = y; pose.z = z;
        //button.GetComponentInChildren<TextMeshProUGUI>().text = x.ToString()+";"+y.ToString()+";"+z.ToString();
        button.GetComponentInChildren<TextMeshProUGUI>().text = x.ToString();
        //button.GetComponentInChildren<TextMeshProUGUI>().text = coords[0].text + ";"+ coords[1].text + ";"+ coords[2].text;
        //if (bx && by && bz)
        //{
        //    //Instantiate(objectToSpawn, pose,new Quaternion(0,0,0,0));
        //    //button.GetComponentInChildren<TextMeshProUGUI>().text = "True";
        //    //foreach(var i in coords)
        //    //    i.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        //}
        //else
        //{
        //    //button.GetComponentInChildren<TextMeshProUGUI>().text = "False";
        //    //if (!bx) coords[0].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        //    //else coords[0].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        //    //if (!by) coords[1].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        //    //else coords[1].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        //    //if (!bz) coords[2].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        //    //else coords[2].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        //}
    }

    void Start()
    {
        button = GetComponent<Button>();
        //for(int i = 0; i < coords.Length; i++)
        //{
        //    texts[i] = coords[i].GetComponentInChildren<TextMeshProUGUI>();
        //}
        
    }
    void Update()
    {
        button.onClick.AddListener(AddPointToPlace);
    }
}
