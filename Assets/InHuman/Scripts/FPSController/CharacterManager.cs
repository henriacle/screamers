﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 100.0f)] private float _playerHealth = 100.0f;
    [SerializeField] private CameraBloodEffect _cameraBloodEffect = null;
    [SerializeField] private AISoundEmitter _soundEmitter = null;
    [SerializeField] private GameObject _healthBar = null;
    [SerializeField] float _currentHealth = 0.0f;
    [SerializeField] private CapsuleCollider _meleeTrigger = null;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private float _health = 100.0f;
    [SerializeField] private float _walkRadius = 0.0f;
    [SerializeField] private float _runRadius = 7.0f;
    [SerializeField] private float _landingRadius = 12.0f;
    [SerializeField] private float _bloodRadiusScale = 6.0f;
    [SerializeField] private PlayerHUD _playerHUD = null;
    [SerializeField] private bool invencible = false;
    [SerializeField] private GameObject _playerObjects = null;
    [SerializeField] private float hpup = 33.0f;

    // Pain Damage Audio
    [SerializeField] private AudioSource     _punchSound    = null;
    [SerializeField] private AudioCollection _damageSounds  = null;
    [SerializeField] private AudioCollection _painSounds    = null;
    [SerializeField] private AudioCollection _tauntSounds   = null;

    RectTransform _lifeRect = null;
    float _lifeRectWidth = 0.0f;
    float _lifeRectHeight = 0.0f;

    [SerializeField] private float _nextPainSoundTime = 0.0f;
    [SerializeField] private float _painSoundOffset = 0.35f;
    [SerializeField] private float _tauntRadius = 10.0f;

    private Collider _collider = null;
    private FirstPersonController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;
    private int _aiBodyPartLayer = -1;
    private int _interactiveMask = 0;
    private int _dialogMask = 0;
    private float _nextAttackTime = 0;
    private float _nextTauntTime = 0;

    public float playerHealth { get { return _playerHealth; } set { _playerHealth = value; } }
    public float playerCurrentHealth { get { return _currentHealth; } set { _currentHealth = value; } }
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller;

    public FirstPersonController fpsController { get { return _fpsController; } }


    public void medpick()
    {
        playerCurrentHealth = _currentHealth + hpup;

        if(playerCurrentHealth > 100.0f)
        {
            playerCurrentHealth = 100.0f;
        }

        _lifeRectWidth = 250 - (250 - (250 * playerCurrentHealth / 100));
        _lifeRect.sizeDelta = new Vector2(_lifeRectWidth, _lifeRect.rect.height);
        _lifeRect.ForceUpdateRectTransforms();
    }

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GameObject.FindObjectOfType<FirstPersonController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;
        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
        _interactiveMask = 1 << LayerMask.NameToLayer("Interactive");
        _dialogMask = 1 << LayerMask.NameToLayer("Dialog");

        _lifeRect = _healthBar.GetComponent<RectTransform>();
        if (_lifeRect)
        {
            _lifeRectWidth = _lifeRect.rect.width;
            _lifeRectHeight = _lifeRect.rect.height;
            playerCurrentHealth = playerHealth;
        }
    }

    public void gameObjectHit(HitInfo hit)
    {
        takeDamage(hit.hit.point, 5.0f, hit.force, hit.hit.rigidbody);
    }

    public void takeDamage(Vector3 position, float force, float damage, Rigidbody bodyPart, int hitDirection = 0)
    {
        doDamage(damage);
    }

    public void meleeDamage(float damage)
    {
        _punchSound.Play();
        doDamage(damage);
    }

    public void freezeFPSController(bool freeze)
    {
        _fpsController.enabled = !freeze;
        if(!_fpsController.enabled)
        {
            _playerObjects.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _fpsController.m_MouseLook.lockCursor = false;
            _fpsController.m_MouseLook.UpdateCursorLock();
        } else
        {
            _playerObjects.SetActive(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _fpsController.m_MouseLook.lockCursor = true;
            _fpsController.m_MouseLook.UpdateCursorLock();
        }

    }

    private void doDamage(float damage)
    {
        if (!invencible)
        {
            playerCurrentHealth = _currentHealth - damage;
        };

        _lifeRectWidth = 250 - (250 - (250 * playerCurrentHealth / 100));
        _lifeRect.sizeDelta = new Vector2(_lifeRectWidth, _lifeRect.rect.height);
        _lifeRect.ForceUpdateRectTransforms();

        if (_cameraBloodEffect != null)
        {
            _cameraBloodEffect.minBloodAmount = 0;
            _cameraBloodEffect.bloodAmount = Mathf.Min((1.1f - 100 / 100.0f) + 0.3f, 1.0f);
        }

        if (playerCurrentHealth <= 0)
        {
            SceneManager.LoadScene(1);
        }
    }

    private void Update()
    {
        Ray ray;
        RaycastHit hit;
        RaycastHit[] hits;

        // PROCESS INTERACTIVE OBJECTS
        // Is the crosshair over a usuable item or descriptive item...first get ray from centre of screen
        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // Calculate Ray Length
        float rayLength = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(_camera.transform.forward, Vector3.up)));
        // Cast Ray and collect ALL hits

        hits = Physics.RaycastAll(ray, rayLength, _interactiveMask);

        // Process the hits for the one with the highest priorty
        if (hits.Length > 0)
        {
            // Used to record the index of the highest priorty
            int highestPriority = int.MinValue;
            InteractiveItem priorityObject = null;

            // Iterate through each hit
            for (int i = 0; i < hits.Length; i++)
            {
                // Process next hit
                hit = hits[i];

                // Fetch its InteractiveItem script from the database
                InteractiveItem interactiveObject = _gameSceneManager.GetInteractiveItem(hit.collider.GetInstanceID());

                // If this is the highest priority object so far then remember it
                if (interactiveObject != null && interactiveObject.priority > highestPriority)
                {
                    priorityObject = interactiveObject;
                    highestPriority = priorityObject.priority;
                }
            }

            // If we found an object then display its text and process any possible activation
            if (priorityObject != null)
            {
                if (_playerHUD) {
                    bool doDialog;
                    string text = priorityObject.GetText(out doDialog);
                    if (doDialog)
                    {
                        _playerHUD.SetDialogText(text);
                    } else {
                        _playerHUD.SetInteractionText(priorityObject.GetText());
                    }
                }

                if (Input.GetButtonDown("Use"))
                {
                    priorityObject.Activate(this);
                }
            }
        }
        else
        {
            if (_playerHUD)
                _playerHUD.SetInteractionText(null);
            _playerHUD.SetDialogText(null);
        }


        if (_fpsController || _soundEmitter != null)
        {
            float newRadius = Mathf.Max(_walkRadius, (100.0f - _health) / _bloodRadiusScale);
            if (!_fpsController.m_IsWalking)
            {
                newRadius = Mathf.Max(newRadius, _runRadius);
            }

            _soundEmitter.SetRadius(newRadius);
        }
    }

    public void setChoiceText(List<Choice> choices, DialogConfig config)
    {
        _playerHUD.setChoiceText(choices, config);
    }

    public void clearPlayerChoiceButtons()
    {
        _playerHUD.clearPlayerChoices();
    }
}
