using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class machinegunammo : MonoBehaviour
{
     public Weapon _wp;
    public int quantia;
    public AudioSource som;
    // Start is called before the first frame update
    void Start()
    {
        som = som.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _wp.totalAmmo += quantia;
        Destroy(this.gameObject);
        som.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
