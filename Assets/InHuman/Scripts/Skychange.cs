using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skychange : MonoBehaviour
{

    public float step = 0f;
    private Color colorStart;
    private Color colorEnd;
    private float duration = 100f;

    // Start is called before the first frame update
    void Start()
    {
       
    }
    // Update is called once per frame
    void Update()
    {
        Color32 colorStart = new Color32(43, 99, 57, 128);
        Color32 colorEnd = new Color32(255, 0, 7, 255);
        RenderSettings.skybox.SetColor("_Tint", Color.Lerp(colorStart, colorEnd, step));
        step = Mathf.PingPong(Time.time, duration) / duration;
    }
}
