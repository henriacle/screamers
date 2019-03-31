using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Posicao : MonoBehaviour
{
    public Text posicaox;
    public Text posicaoy;
    public Text posicaoz;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       posicaox.text = "X= " + (transform.position.x);
       posicaoy.text = "Y= " + (transform.position.y);
       posicaoz.text = "Z= " + (transform.position.z);
    }
}
