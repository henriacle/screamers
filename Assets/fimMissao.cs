using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class fimMissao : MonoBehaviour
{
    ApplicationManager _appManager = null;
    private bool chaveObtida = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        _appManager = ApplicationManager.instance;

        if (_appManager && _appManager.GetGameState("FIMMISSAO") == "TRUE" && chaveObtida == false)
        {
            chaveObtida = true;
            SceneManager.LoadScene("Creditos", LoadSceneMode.Single);
            Debug.Log("Missão Finalizada");
        }
    }
}

