using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class totaldisplay : MonoBehaviour
{
    public TextMeshProUGUI showammo;
    public Weapon machinegun;
    public Weapon pistol;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (machinegun.isActiveAndEnabled == true){ 
        showammo.text = "Munição: " + machinegun.totalAmmo;
        }
        else if (pistol.isActiveAndEnabled == true)

        {
            showammo.text = "Munição: " + pistol.totalAmmo;
        }
       
        
    }
}
