using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTeste : MonoBehaviour
{
   public float duration = 1.0f;
    Color color0 = Color.red;
    Color color1 = Color.blue;

   public Light lt;
    public bool inside = false;
    float t = 0f;


   public void changecolor()
    {
        inside = true;
    }

    public void stopcolor()
    {
        inside = false;
    }

    void Start()
    {
        lt = GetComponent<Light>();
    }

    void Update()
    {
        if (inside)
        {
            // set light color
            // float t = 1f;
            // float t = Mathf.Lerp(0f, 1f, t);
            lt.color = Color.Lerp(color0, color1, t);
        } else
        {
            return;
        }
    }




}