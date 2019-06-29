using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class medkit : MonoBehaviour
{
    public CharacterManager player;
    public AudioSource som;
    // Start is called before the first frame update
    void Start()
    {
        som = som.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        player.medpick();
        Destroy(this.gameObject);
        som.Play();
    }
}