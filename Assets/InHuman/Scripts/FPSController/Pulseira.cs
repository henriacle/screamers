using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulseira : MonoBehaviour
{
    ApplicationManager _appManager = null;
    // Start is called before the first frame update
    void Start()
    {
        _appManager = ApplicationManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        string tempulseira      = _appManager.GetGameState("tempulseira");
        string pulseiraLigada   = _appManager.GetGameState("pulseiraLigada");

        if (Input.GetButtonDown("pulseira"))
        {
            if(tempulseira == "True")
            {
                if(pulseiraLigada == "False")
                {
                    Debug.Log("ligar Pulseira");
                }
                if (pulseiraLigada == "True")
                {
                    Debug.Log("Desligar Pulseira");
                }
            }
        }
    }
}
