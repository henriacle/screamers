using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class statusTV : MonoBehaviour
{
    [SerializeField] GameObject tvStatic = null;
    ApplicationManager _appManager = null;
    // Update is called once per frame
    void Update()
    {
        _appManager = ApplicationManager.instance;

        if(_appManager)
        {
            string statusTV = _appManager.GetGameState("TV_LIGADA_1");
            if(statusTV == "TRUE")
            {
                tvStatic.SetActive(true);
            } else
            {
                tvStatic.SetActive(false);
            }
        }
    }
}
