using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class checkmapa : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _mapText = null;
    public bool pausablemap = false;

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

        if (appManager.GetGameState("MAPA") == "TRUE")
        {
            _mapText.text = "Mapa Obtido";

            pausablemap = true;

        }
    }
}
