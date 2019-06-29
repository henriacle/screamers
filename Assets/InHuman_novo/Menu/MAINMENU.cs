using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MAINMENU : MonoBehaviour
{

    public AudioSource som;
    public AudioSource musica;
    // Start is called before the first frame update
    void Start()
    {
        som = som.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {



    }

    public void startgame()
    {
        musica.Stop();
        som.Play();
        StartCoroutine(waitfor());
    }

    public void leavegame()
    {
        Application.Quit();
    }


    private IEnumerator waitfor()
    {
        print("esperando som");
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("fase main", LoadSceneMode.Single);
    }


}
