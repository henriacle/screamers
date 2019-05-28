/// <summary>
/// WeaponSystem.cs
/// Author: MutantGopher
/// This script manages weapon switching.  It's recommended that you attach this to a parent GameObject of all your weapons, but this is not necessary.
/// This script allows the player to switch weapons in two ways, by pressing the numbers corresponding to each weapon, or by scrolling with the mouse.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    public GameObject[] weapons;                // The array that holds all the weapons that the player has
    public int startingWeaponIndex = 0;         // The weapon index that the player will start with
    public int weaponIndex;              // The current index of the active weapon

    //public Weapon _wpsmg;
    //public Weapon _wppistol;

    public GameObject _wp;
        [SerializeField] CharacterManager charManager;


    public a_machinegun a_m;
    public A_Pistol a_p;

    public bool ischanging = false;

    public bool change1;
    public bool change2;

    // Use this for initialization
    void Start()
    {
        // Make sure the starting active weapon is the one selected by the user in startingWeaponIndex
        weaponIndex = startingWeaponIndex;
        SetActiveWeapon(weaponIndex);



    }


    public void weaponchange()
    {
        if (change1) {
            SetActiveWeapon(0);
            StartCoroutine(finaltransition());
        }
        else if (change2)
        {
           SetActiveWeapon(1);
            StartCoroutine(finaltransition());
            
        }
    
    }
    IEnumerator wptransition()
    {
        yield return new WaitForSeconds(0.5f);
        
        weaponchange();
    }

    IEnumerator finaltransition()

    {
       
        yield return new WaitForSeconds(0.5f);
        _wp.GetComponent<Weapon>().enabled = true;
        change1 = false;
        change2 = false;
        ischanging = false;
        a_m.change = false;
        a_p.change = false;
    }

        // Update is called once per frame
        void Update()
	{
        _wp = GameObject.FindWithTag("Weapon");
        // Allow the user to instantly switch to any weapon
        if (Input.GetButtonDown("Weapon 1") && !ischanging && weaponIndex != 0)
        {
            ischanging = true;
            change1 = true;
            _wp.GetComponent<Weapon>().enabled = false;


            StartCoroutine(wptransition());
        
        }
        if (Input.GetButtonDown("Weapon 2") && !ischanging && weaponIndex != 1)
        {
            ischanging = true;
            change2 = true;

             _wp.GetComponent<Weapon>().enabled = false;
            StartCoroutine(wptransition());
            
        }
		//if (Input.GetButtonDown("Weapon 3"))
			//SetActiveWeapon(2);
	//	if (Input.GetButtonDown("Weapon 4"))
			//SetActiveWeapon(3);
		//if (Input.GetButtonDown("Weapon 5"))
			//SetActiveWeapon(4);
		//if (Input.GetButtonDown("Weapon 6"))
		//	SetActiveWeapon(5);
		//if (Input.GetButtonDown("Weapon 7"))
			//SetActiveWeapon(6);
		//if (Input.GetButtonDown("Weapon 8"))
			//SetActiveWeapon(7);
		//if (Input.GetButtonDown("Weapon 9"))
			//SetActiveWeapon(8);

		// Allow the user to scroll through the weapons
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
			NextWeapon();
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
			PreviousWeapon();
	}



    


    void OnGUI()
	{


	}

	public void SetActiveWeapon(int index)
	{
		// Make sure this weapon exists before trying to switch to it
		if (index >= weapons.Length || index < 0)
		{
			Debug.LogWarning("Tried to switch to a weapon that does not exist.  Make sure you have all the correct weapons in your weapons array.");
			return;
		}

		// Send a messsage so that users can do other actions whenever this happens
		SendMessageUpwards("OnEasyWeaponsSwitch", SendMessageOptions.DontRequireReceiver);

		// Make sure the weaponIndex references the correct weapon
		weaponIndex = index;

        // Make sure beam game objects aren't left over after weapon switching
        // weapons[index].GetComponent<Weapon>().StopBeam();

		// Start be deactivating all weapons
		for (int i = 0; i < weapons.Length; i++)
		{
			weapons[i].SetActive(false);
		}

		// Activate the one weapon that we want
		weapons[index].SetActive(true);
	}

	public void NextWeapon()
	{
		weaponIndex++;
		if (weaponIndex > weapons.Length - 1)
			weaponIndex = 0;
		SetActiveWeapon(weaponIndex);
	}

	public void PreviousWeapon()
	{
		weaponIndex--;
		if (weaponIndex < 0)
			weaponIndex = weapons.Length - 1;
		SetActiveWeapon(weaponIndex);
	}


}
