using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sitanim : MonoBehaviour
{
    Animator animator;
    public bool lookatyou = false;
    public bool looktv = false;
    
    public float intervalo = 3.0f;
  
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    IEnumerator delayanim()
    {
        intervalo = 48.0f;
        animator.SetBool("Lookatyou", true);
        yield return new WaitForSeconds(5);
        animator.SetBool("Lookatyou", false);
        yield return new WaitForSeconds(7);
        animator.SetBool("Looktv", true);
        yield return new WaitForSeconds(5);
        animator.SetBool("Looktv", false);
    
    }

    // Update is called once per frame
    void Update()
    {
        intervalo -= Time.deltaTime;
       



        if (intervalo <= 0.0f)
        {
            StartCoroutine("delayanim");
           

        }


    }
  
}
