using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fimMissao : MonoBehaviour
{
    ApplicationManager _appManager = null;
    // Update is called once per frame
    void FixedUpdate()
    {
        _appManager = ApplicationManager.instance;

        if (_appManager && _appManager.GetGameState("FIMMISSAO") == "TRUE")
        {
            Debug.Log("Missão Finalizada");
        }
    }
}
