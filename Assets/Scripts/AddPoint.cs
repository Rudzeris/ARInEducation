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
using static UnityEngine.InputSystem.HID.HID;
using Button = UnityEngine.UI.Button;

public class AddPoint : MonoBehaviour
{
    Button button;
    Vector3 pose;
    //[SerializeField] private TMP_InputField[] TMPList;
    [SerializeField] private TextMeshProUGUI[] coords;
    //[SerializeField] private GameObject spawn;
    [SerializeField] private GameObject objectToSpawn;
    //[SerializeField] private 
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
        string sx, sy, sz;
        sx = coords[0].text.ToString();
        sy = coords[1].text.ToString();
        sz = coords[2].text.ToString();
        sx = sx.Substring(0, sx.Length - 1);
        sy = sy.Substring(0, sy.Length - 1);
        sz = sz.Substring(0, sz.Length - 1);
        float x = 0;
        float y = 0;
        float z = 0;
        bool bx = (sx.Length > 0 ? float.TryParse(sx, out x) : false);
        bool by = (sy.Length > 0 ? float.TryParse(sy, out y) : false);
        bool bz = (sz.Length > 0 ? float.TryParse(sz, out z) : false);

        if (bx && by && bz)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = x.ToString() + ";" + y.ToString() + ";" + z.ToString();
            pose.x = x;
            pose.y = y;
            pose.z = z;
            objectToSpawn.tag = "Point";
            ARToPlaceObject arToPlaceObject = FindObjectOfType<ARToPlaceObject>();
            if (arToPlaceObject != null)
            {
                pose /= 50*5;
                arToPlaceObject.SpawnObject(objectToSpawn,pose,new Quaternion(0f,0f,0f,0f));
            }
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
        }

        // Удаляем обработчик нажатия на кнопку
        //button.onClick.RemoveListener(AddPointToPlace);
    }

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(AddPointToPlace);
    }
    int buttonIndex;

    public void VisibleButton()
    {
        string sx, sy, sz;
        sx = coords[0].text.ToString();
        sy = coords[1].text.ToString();
        sz = coords[2].text.ToString();
        sx = sx.Substring(0, sx.Length - 1);
        sy = sy.Substring(0, sy.Length - 1);
        sz = sz.Substring(0, sz.Length - 1);
        float x = 0;
        float y = 0;
        float z = 0;
        bool bx = (sx.Length > 0);
        bool by = (sy.Length > 0);
        bool bz = (sz.Length > 0);
        gameObject.SetActive(bx & by & bz);
    }
    void Update() 
    {
    }
}
