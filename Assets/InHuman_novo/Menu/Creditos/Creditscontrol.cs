using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Creditscontrol : MonoBehaviour
{

    //public AudioSource audioSource;
    // Use this for initialization
    void Start()
    {
     
    }

    
    void Update() {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Intro", LoadSceneMode.Single);
        }

    }



}

