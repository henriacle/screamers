using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string _parameter = null;

    //private
    private AIStateMachine  _stateMachine   = null;
    private Animator        _animator       = null;
    int                     _parameterHash = -1;

    private void Start()
    {
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();
        if(_stateMachine != null)
            _animator = _stateMachine.animator;

        _parameterHash = Animator.StringToHash(_parameter);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_animator)
            return;

        if (other.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            Debug.Log("damage player");
        }
    }
}
