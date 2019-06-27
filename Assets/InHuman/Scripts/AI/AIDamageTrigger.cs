using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string _parameter = null;

    //private
    private AIStateMachine      _stateMachine   = null;
    [SerializeField] Animator   _animator       = null;
    int                         _parameterHash  = -1;

    private void Start()
    {
        _parameterHash = Animator.StringToHash(_parameter);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_animator)
            return;

        if (other.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            Debug.Log("batendo damage");
            CharacterManager charManager = other.GetComponent<CharacterManager>();
            charManager.meleeDamage(2f);
        }
    }
}
