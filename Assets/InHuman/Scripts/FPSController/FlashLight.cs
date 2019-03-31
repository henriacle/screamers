using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [SerializeField] private GameObject _flashlight = null;


    private void Update()
    {
        if(Input.GetButtonDown("FlashLight"))
        {
            _flashlight.SetActive(!_flashlight.activeSelf);
        }
    }
}
