using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class FPSController : MonoBehaviour
{
    private FirstPersonController fpc;

    private void Start()
    {
        fpc = gameObject.GetComponent<FirstPersonController>();
    }
}
