using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());

        if (machine && gameObject.CompareTag("Fire Zone") && !machine.inMeleeRange)
            machine.inFireRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());

        if (machine && gameObject.CompareTag("Fire Zone"))
            machine.inFireRange = false;
    }
}
