using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseoption : MonoBehaviour
{
    public bool paused = false;
    public bool maptaken = false;
    public checkmapa check;
    public GameObject PUI;
    public GameObject weaponobj;
    public GameObject mapcontainer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if ((Input.GetKeyDown(KeyCode.Tab)) && (maptaken == true))
        {
            pausarjogo();
        }
        if (check.pausablemap == true)
        {

            maptaken = true;
        }
    }

    private void pausarjogo()
    {
        if (paused == false)
        {
            paused = true;
            Time.timeScale = 0.0f;
            PUI.SetActive(false);
            weaponobj.SetActive(false);
            mapcontainer.SetActive(true);
        } else if 
        (paused == true)
        {
            PUI.SetActive(true);
            weaponobj.SetActive(true);
            mapcontainer.SetActive(false);
            Time.timeScale = 1.0f;
            paused = false;
        }
    }


}
