using UnityEngine;
using System.Collections;

public class AttachWeapon : MonoBehaviour {
	public Transform attachPoint;
	public GameObject Weapon;
    private bool hasWeapon = false;

	// Use this for initialization
	void Start () {
        if(Weapon)
        {
            Weapon.transform.parent = attachPoint;
            Weapon.transform.position = attachPoint.position;
            Weapon.transform.rotation = attachPoint.rotation;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (hasWeapon)
        {
            Weapon.SetActive(true);
            Weapon.transform.parent = attachPoint;
            Weapon.transform.position = attachPoint.position;
            Weapon.transform.rotation = attachPoint.rotation;
        }
    }

    public void weaponStatus(bool status) {
        hasWeapon = status;
        Weapon.SetActive(status);
    }

    public void setWeapon(GameObject weapon)
    {
        Weapon = weapon;
    }
}
