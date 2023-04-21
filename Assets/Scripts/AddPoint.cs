using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class AddPoint : MonoBehaviour
{
    Button button;
    Vector3 pose;
    [SerializeField] private TextMeshProUGUI[] coords;
    [SerializeField] private GameObject spawn;
    [SerializeField] private GameObject objectToSpawn;
    //InputField InF;

    int intparse(char a)
    {
        return a - '0';
    }

    float Parse(string text)
    {
        int x = 0;
        int y = 0;
        double z = 0;
        int n = 1;
        int k = 0;
        bool bl = false;
        for (int i = text.Length - 2; i >= 0; i--)
        {
            if (text[i] == '.')
            {
                if (bl) return 0;
                bl = true;
                n = 1;
            }
            if (!bl)
            {
                x += intparse(text[i]) * n;
                k++;
            }
            else
            {
                y += intparse(text[i]) * n;
            }
            n *= 10;
        }
        z = x;
        if (bl)
            while ((int)y >= 0)
            {
                z /= 10;
            }
        z += y;
        return (float)z;
    }

    void AddPointToPlace()
    {
        //button.GetComponentInChildren<TextMeshProUGUI>().text = texts[0].text;
        //button.GetComponentInChildren<TextMeshProUGUI>().text;
        string sx, sy, sz;
        sx = coords[0].text.ToString();
        sy = coords[1].text.ToString();
        sz = coords[2].text.ToString();
        sx = sx.Substring(0, sx.Length - 1);
        sy = sy.Substring(0, sy.Length - 1);
        sz = sz.Substring(0, sz.Length - 1);
        float x=0;
        float y=0;
        float z=0;
        bool bx = (sx.Length > 0 ? float.TryParse(sx, out x) : false);
        bool by = (sy.Length > 0 ? float.TryParse(sy, out y) : false);
        bool bz = (sz.Length > 0 ? float.TryParse(sz, out z) : false);
        // coords[0].text = "232";
        //pose.x = x; pose.y = y; pose.z = z;
        //button.GetComponentInChildren<TextMeshProUGUI>().text = x.ToString();
        //button.GetComponentInChildren<TextMeshProUGUI>().text = coords[0].text + ";"+ coords[1].text + ";"+ coords[2].text;
        if (bx && by && bz)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = x.ToString() + ";" + y.ToString() + ";" + z.ToString();
            pose.x = x / 100;
            pose.y = y / 100;
            pose.z = z / 100;
            objectToSpawn.tag = "Point";
            Instantiate(objectToSpawn, pose, new Quaternion(0, 0, 0, 0));
            //foreach (var i in coords)
            //    i.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }
        else
        {
            char st2 = 'q';
            string str2 = "";
            if (bx) str2 += x.ToString();
            else str2 += st2;
            str2 += ";";
            if (by) str2 += y.ToString();
            else str2 += st2;
            str2 += ";";
            if (bz) str2 += z.ToString();
            else str2 += st2;
            button.GetComponentInChildren<TextMeshProUGUI>().text = str2;

            //button.GetComponentInChildren<TextMeshProUGUI>().text = "False";
            //if (!bx) coords[0].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
            //else coords[0].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            //if (!by) coords[1].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
            //else coords[1].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            //if (!bz) coords[2].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
            //else coords[2].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }
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
