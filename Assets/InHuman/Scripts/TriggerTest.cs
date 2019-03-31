using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerTest : MonoBehaviour
{
    public bool enter = true;
    public GameObject luz;

    public float Rotacao = 0.2f;



    // Start is called before the first frame update
    void Start()
    {
       

    }
        //private void OnTriggerEnter(Collider other)
        //{
        //    if (enter)
        //    {
        //        luz.GetComponent<LightTeste>().changecolor();
        //        Debug.Log("entered");
        //    }
        //}

    //void OnTriggerExit(Collider other)
    //{
    //    luz.GetComponent<LightTeste>().stopcolor();
    //}

// Update is called once per frame
void Update()
    {


        RenderSettings.skybox.SetFloat("_Rotation", Time.time * Rotacao);
       

        if (Input.GetKeyDown("p"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    

    }

}
   

