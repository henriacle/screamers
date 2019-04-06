using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public void gameObjectHit(HitInfo hit)
    {
        Debug.Log("object hit");
        //takeDamage(hit.hit.point, 5.0f, hit.damage, hit.hit.rigidbody);
    }
}
