using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVNoise : MonoBehaviour
{
    [SerializeField] private Material _noise;
    Renderer rend;
    float _x = 0;
    float _y = 0;
    float scrollSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.EnableKeyword("_EMISSION");
    }

    // Update is called once per frame
    void Update()
    {
        float offset = Time.time * scrollSpeed;
    }
}
