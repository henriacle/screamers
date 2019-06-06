using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class checkchave : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _mapText = null;

    // Start is called before the first frame update
    void Start()
    {
        _mapText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null) return;

        if(appManager.GetGameState("CHAVE") == "TRUE")
        {
            _mapText.text = "Chave Obtida";
        }
    }
}
