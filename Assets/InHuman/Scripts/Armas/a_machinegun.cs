using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class a_machinegun : MonoBehaviour
{
    Animator animator;
    [SerializeField] CharacterManager _characterManager;

    private bool shoot;
    private bool reload;
    private bool canshoot = true;
    public bool change = false;


    public float firetime;
    public float delay;
    public int totalammodisplay;
    public int totalammo = 90;
    public int reloadammount;

    public Weapon _wp;
    public WeaponSystem _wps;
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
        animator.SetBool("Change", change);

  

        if (_wp.currentAmmo == 0)
        {
            shoot = false;
            canshoot = false;
        }


        if  (_wp.fireTimer >= 0f && (Input.GetMouseButton(0)) && !reload)
        {
            shoot = true;

        }
          else 
        {
            shoot = false;
        }

     
        if (Input.GetButtonDown("Reload"))
        {
            StartCoroutine(reloadtime());
           

        }

        if (Input.GetButtonDown("Weapon 2")) // Troca para outras armas
        {
            Debug.Log("firing");
            change = true;
            reload = false;
            canshoot = true;
        }
   
    }
    IEnumerator reloadtime()
    {
        reload = true;
        canshoot = false;
        yield return new WaitForSeconds(3.57f);
        reload = false;
        canshoot = true;
        

    }

}


     