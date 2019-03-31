using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Pistol : MonoBehaviour
{
    Animator animator;
    private bool shoot = false;
    private bool reload;
    private bool canshoot = true;
    private bool trigger;
    public bool change = false;
    public float firetime;
    public float delay;
    public float holding;

    public int totalammodisplay;
    public int totalammo = 90;


    public Weapon _wp;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        _wp.totalAmmo = totalammo;

    }



    // Update is called once per frame
    void Update()
    {
        totalammodisplay = _wp.totalAmmo;
        firetime = _wp.fireTimer;
        delay = _wp.burstTimer;
        animator.SetBool("Shoot", shoot);
        animator.SetBool("Reload", reload);
        animator.SetBool("Canshoot", canshoot);
        animator.SetBool("Triggerhold", trigger);
        animator.SetBool("Change", change);


        if (Input.GetButtonDown("Weapon 1")) // Troca para outras armas. Esta arma é a "weapon2"
        {
            change = true;
            reload = false;
            canshoot = true;
            StopAllCoroutines();
        }

        if (Input.GetButtonDown("Reload"))
        {
            StartCoroutine(reloadtime());
            

        }
  
        if (_wp.burstTimer >= 0.1f)
        {
            shoot = false;
            canshoot = false;

        }
        if (_wp.burstTimer == 0f && !reload)
        {
            canshoot = true;

        }
        if (_wp.currentAmmo == 0)
        {
            StartCoroutine(lastshoot());
         
        }
        if ( _wp.burstTimer >= 0.01f && canshoot == true)
        {

            shoot = true;

        }
        if (Input.GetMouseButton(0))
        {
            holding += Time.deltaTime;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            holding = 0;
          
        }
    
        if (holding > 0.2)
        {
            trigger = false;
        }
        else if (holding == 0)
        {
            trigger = true;
        }



        //if (Input.GetButtonDown("Weapon 2"))
        //{

        //    StartCoroutine(change2());
        //}
    }

    IEnumerator lastshoot()
    {
        yield return new WaitForSeconds(0.1f);
        shoot = false;
        canshoot = false;
    }

    IEnumerator reloadtime()
    {
        
        reload = true;
        canshoot = false;
        yield return new WaitForSeconds(3.35f);
        reload = false;
        canshoot = true;
    }

 

}
