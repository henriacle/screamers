using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class bunkerdoor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(appManager.GetGameState("CHAVE") == "TRUE")
        {
            SceneManager.LoadScene("Creditos", LoadSceneMode.Single);
        }
    }
    // Update is called once per frame
    void Update()
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null) return;
    }
}
