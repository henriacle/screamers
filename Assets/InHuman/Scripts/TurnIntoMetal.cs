using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnIntoMetal : MonoBehaviour
{
    private bool    _isTurnedIntoMetal = false;
    [SerializeField] Texture    texture = null;
    [SerializeField] GameObject wall    = null;
    public bool turnedIntoMetal { get { return _isTurnedIntoMetal; } }

    public void OnTriggerEnter(Collider other)
    {
        if (other.name == "kiddo" && !_isTurnedIntoMetal)
        {
            Material material = other.GetComponent<Renderer>().material;
            material.SetTexture("_MainTex", texture);
            wall.SetActive(false);
            _isTurnedIntoMetal = true;
        }
    }
}
