using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class a_shootgun : MonoBehaviour
{
    Animator animator;

    private bool shoot = false;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("Shoot", shoot);

        if (Input.GetMouseButton(0))
        {
            shoot = true;
        } else
        {
            shoot = false;
        }



    }
}
