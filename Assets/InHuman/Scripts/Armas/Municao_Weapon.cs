using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Municao_Weapon : MonoBehaviour
{
    public TextMeshProUGUI textoammo;
    public Weapon _wp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
  

        textoammo.text = " " + _wp.currentAmmo;
    }
}
