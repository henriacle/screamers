using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());

        if(machine && gameObject.CompareTag("Melee Zone"))
            machine.inMeleeRange    = true;
    }

    private void OnTriggerExit(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());

        if (machine && gameObject.CompareTag("Melee Zone"))
            machine.inMeleeRange    = false;
    }
}
