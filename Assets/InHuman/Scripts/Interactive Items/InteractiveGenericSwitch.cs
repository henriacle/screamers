﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimatorParameterType { Trigger, Bool, Int, Float, String }

[System.Serializable]
public class AnimatorParameter
{
    public AnimatorParameterType Type = AnimatorParameterType.Bool;
    public string Name = null;
    public string Value = null;
    public float timeToReset = 0.0f;
}

[System.Serializable]
public class AnimatorConfigurator
{
    [SerializeField] public Animator Animator = null;
    [SerializeField] public List<AnimatorParameter> AnimatorParams = new List<AnimatorParameter>();
}

public class InteractiveGenericSwitch : InteractiveItem
{
    // Key Values that
    [Header("Game State Management")]
    [SerializeField] protected List<GameState> _requiredStates = new List<GameState>();
    [SerializeField] protected List<GameState> _activateStates = new List<GameState>();
    [SerializeField] protected List<GameState> _deactivateStates = new List<GameState>();

    [Header("Dialog")]
    [SerializeField] protected bool _incrementDialog = true;
    [SerializeField] protected int _jumpToIndex = 0;
    [SerializeField] protected bool _doDialog = false;
    [SerializeField] protected bool _resetDialog = false;
    [SerializeField] protected CharacterManager _characterManager = null;
    [SerializeField] protected int _currentDialog = 0;
    [SerializeField] public AnimatorConfigurator _dialogAnimator = null;
    [SerializeField] protected int _currentDialogIndex = 0;
    [SerializeField] protected List<DialogState> _dialogLines = new List<DialogState>();

    [Header("Message")]
    [TextArea(3, 10)]
    [SerializeField] protected string _stateNotSetText = "";
    [TextArea(3, 10)]
    [SerializeField] protected string _stateSetText = "";
    [TextArea(3, 10)]
    [SerializeField] protected string _ObjectActiveText = "";

    // Activation Sound
    [Header("Activation Parameters")]
    [SerializeField] protected float _activationDelay = 1.0f;
    [SerializeField] protected float _deactivationDelay = 1.0f;
    [SerializeField] protected AudioCollection _activationSounds = null;
    [SerializeField] protected AudioSource _audioSource = null;

    // Operation
    [Header("Operating Mode")]
    [SerializeField] protected bool _startActivated = false;
    [SerializeField] protected bool _canToggle = false;

    [Header("Configurable Entities")]
    // Inspector Assigned Animators that need to be affected by the trigger
    [SerializeField] protected List<AnimatorConfigurator> _animations = new List<AnimatorConfigurator>();

    // Materials that need to have their emissive properties effected by the trigger
    [SerializeField] protected List<MaterialController> _materialControllers = new List<MaterialController>();

    // GameObjects that need to be activated or deactivated by this trigger	 
    [SerializeField] protected List<GameObject> _objectActivators = new List<GameObject>();
    [SerializeField] protected List<GameObject> _objectDeactivators = new List<GameObject>();

    // Private stuff
    protected IEnumerator _coroutine = null;
    protected bool _activated = false;
    protected bool _firstUse = false;
    protected int _previouslyDialogIndex = 0;

    // ---------------------------------------------------------------------------
    // Name	: Start
    // Desc	: Register this objects collider with the Activation Database
    // ---------------------------------------------------------------------------
    protected override void Start()
    {
        // Call the base class (VERY IMPORTANT) as this registers the scene with the app database
        base.Start();

        // Activate Material Controller
        for (int i = 0; i < _materialControllers.Count; i++)
        {

            if (_materialControllers[i] != null)
            {
                _materialControllers[i].OnStart();
            }
        }

        // Turn off all objects that should be activated
        for (int i = 0; i < _objectActivators.Count; i++)
        {
            if (_objectActivators[i] != null)
                _objectActivators[i].SetActive(false);
        }

        for (int i = 0; i < _objectDeactivators.Count; i++)
        {
            if (_objectDeactivators[i] != null)
                _objectDeactivators[i].SetActive(true);
        }

        if (_startActivated)
        {
            Activate(null);
            _firstUse = false;
        }
    }

    public override string GetText()
    {
        // If we have no application database or this switch is disabled then return null
        if (!enabled) return string.Empty;

        // If its already activated then just return the activated text
        if (_activated)
        {
            return _ObjectActiveText;
        }

        // We need to test all the states that need to be set to see if this item can be activated
        // as that determines the text we send back
        bool requiredStates = AreRequiredStatesSet();

        // Return the desired text to reflect whether ot not we can use the object yet
        if (!requiredStates)
        {
            return _stateNotSetText;
        }
        else
        {
            return _stateSetText;
        }
    }

    IEnumerator resetTrigger(float timeToWait, string paramName)
    {
        yield return new WaitForSeconds(timeToWait);
        _dialogAnimator.Animator.ResetTrigger(paramName);
    }

    public override string GetText(out bool doDialog)
    {
        // If we have no application database or this switch is disabled then return null
        doDialog = _doDialog;
        if (!_doDialog) return null;
        if (!enabled) return string.Empty;

        // If its already activated then just return the activated text
        _resetDialog = _dialogLines[_currentDialogIndex].resetDialog;
        if (_activated && _doDialog)
        {


                if (_dialogAnimator != null)
                {

                foreach (AnimatorParameter parameter in _dialogLines[_currentDialogIndex].AnimatorParams)
                {
                    switch (parameter.Type)
                    {
                        case AnimatorParameterType.Bool:
                            bool boolean = bool.Parse(parameter.Value);
                            _dialogAnimator.Animator.SetBool(parameter.Name, boolean);
                            break;
                        case AnimatorParameterType.Trigger:
                            if(parameter.Value.Length == 0)
                            {
                                _dialogAnimator.Animator.SetTrigger(parameter.Name);
                                parameter.Value = "run";
                            }
                            break;
                    }
                }
            }

            if (_dialogLines[_currentDialogIndex].FinishDialog == false)
            {
                if (_dialogLines[_currentDialogIndex].Value.Answer > 0)
                {
                    _characterManager.clearPlayerChoiceButtons();
                    _characterManager.freezeFPSController(false);
                    _previouslyDialogIndex = _currentDialogIndex;
                    _currentDialogIndex = _dialogLines[_currentDialogIndex].Value.Answer - 1;
                    _dialogLines[_previouslyDialogIndex].Value.WaitingForAnswer = false;
                    _dialogLines[_previouslyDialogIndex].Value.Answer = 0;
                    _resetDialog = _dialogLines[_currentDialogIndex].resetDialog;
                }

                if (_dialogLines[_currentDialogIndex].WaitForVariableToChange)
                {
                    _activated = true;
                    ApplicationManager appDatabase = ApplicationManager.instance;
                    bool allStatesTrue = false;

                    if (!_dialogLines[_currentDialogIndex].AreGameStatesSet)
                    {
                        foreach (GameState state in _dialogLines[_currentDialogIndex].GameStateToSet)
                        {
                            appDatabase.SetGameState(state.Key, state.Value);
                        }
                    }

                    _dialogLines[_currentDialogIndex].AreGameStatesSet = true;

                    foreach (GameState state in _dialogLines[_currentDialogIndex].GameStateToCheck)
                    {
                        allStatesTrue = state.Value == appDatabase.GetGameState(state.Key);
                    }

                    if (allStatesTrue)
                    {
                        _currentDialogIndex = _dialogLines[_currentDialogIndex].jumpToDialogIndex - 1;
                        _jumpToIndex = _dialogLines[_currentDialogIndex].jumpToDialogIndex;
                        _stateSetText = _dialogLines[_currentDialogIndex].Text;
                        return _stateSetText;
                    }

                    _ObjectActiveText = _dialogLines[_currentDialogIndex].Text;
                    return _ObjectActiveText;
                }
                else
                {
                    if (_dialogLines[_currentDialogIndex].Value.triggerPlayerChoice == true)
                    {
                        _dialogLines[_currentDialogIndex].Value.WaitingForAnswer = true;
                        _characterManager.freezeFPSController(true);
                        _characterManager.setChoiceText(_dialogLines[_currentDialogIndex].Value.Choices, _dialogLines[_currentDialogIndex].Value);
                    }

                    if (_dialogLines[_currentDialogIndex].Value.WaitingForAnswer)
                    {
                        _stateSetText = _dialogLines[_currentDialogIndex].Text;
                        _incrementDialog = _dialogLines[_currentDialogIndex].IncrementDialog;
                        return _stateSetText;
                    }

                    if (_dialogLines[_currentDialogIndex].Value.Answer > -1)
                    {
                        _dialogLines[_currentDialogIndex].Value.WaitingForAnswer = false;
                        _characterManager.freezeFPSController(false);
                    }

                    if (_currentDialogIndex > _dialogLines.Count - 1)
                    {
                        _activated = true;
                        _incrementDialog = _dialogLines[_currentDialogIndex].IncrementDialog;
                        return _ObjectActiveText;
                    }
                }

                if (_activated)
                {
                    string text = _dialogLines[_currentDialogIndex].Text;
                    _stateSetText = text;
                }

                _incrementDialog = _dialogLines[_currentDialogIndex].IncrementDialog;
                return _stateSetText;
            }

            _ObjectActiveText = _dialogLines[_currentDialogIndex].Text;
            return _ObjectActiveText;
        }

        // If we have no application database or this switch is disabled then return null
        if (!enabled) return string.Empty;

        // If its already activated then just return the activated text
        if (_activated)
        {
            return _ObjectActiveText;
        }

        // We need to test all the states that need to be set to see if this item can be activated
        // as that determines the text we send back
        bool requiredStates = AreRequiredStatesSet();

        // Return the desired text to reflect whether ot not we can use the object yet
        if (!requiredStates)
        {
            return _stateNotSetText;
        }
        else
        {
            return _stateSetText;
        }
    }

    protected bool AreRequiredStatesSet()
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null) return false;

        // Assume the states are all set and then loop to find a state to disprove this

        for (int i = 0; i < _requiredStates.Count; i++)
        {
            GameState state = _requiredStates[i];

            // Does the current state exist in the app dictionary?
            string result = appManager.GetGameState(state.Key);
            if (string.IsNullOrEmpty(result) || !result.Equals(state.Value)) return false;
        }

        return true;
    }

    protected void SetActivationStates()
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null) return;

        if (_activated)
        {
            foreach (GameState state in _activateStates)
            {
                appManager.SetGameState(state.Key, state.Value);
            }
        }
        else
        {
            foreach (GameState state in _deactivateStates)
            {
                appManager.SetGameState(state.Key, state.Value);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Name	:	Activate
    // Desc	:	Activates the object
    // -------------------------------------------------------------------------
    public override void Activate(CharacterManager characterManager)
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null) return;

        // If we are already in a different state to the starting state and
        // we are not in toggle mode then this item has been switched on/ switched off
        // and can not longer be altered
        if (_doDialog)
        {

            if (_incrementDialog)
            {
                _currentDialogIndex++;
            }

            if (_jumpToIndex > 0)
            {
                _currentDialogIndex = _jumpToIndex - 1;
            }

            if (_resetDialog)
            {
                _currentDialogIndex = 0;
            }
        }

        if (_firstUse && !_canToggle) return;

        if (!_activated)
        {
            bool requiredStates = AreRequiredStatesSet();
            if (!requiredStates) return;
        }

        // Object state has been switched
        _activated = !_activated;
        _firstUse = true;

        // play the activation Sound Effect
        if (_activationSounds != null && _activated)
        {
            AudioClip clipToPlay = _activationSounds[0];
            if (clipToPlay == null) return;

            // If an audio source has been specified then use it. This is good for playing looping sounds
            // or sounds that need to happen nowhere near the tigger source
            if (_audioSource != null && AudioManager.instance)
            {
                Debug.Log(_activationSounds[0]);
                _audioSource.clip = clipToPlay;
                _audioSource.volume = _activationSounds.volume;
                _audioSource.spatialBlend = _activationSounds.spatialBlend;
                _audioSource.priority = _activationSounds.priority;
                _audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName(_activationSounds.audioGroup);
                _audioSource.Play();
            }
        }

        // Get the coroutine to perform the delayed activation and if its playing stop it
        if (_coroutine != null) StopCoroutine(_coroutine);

        // Start a new corotuine to do the activation
        _coroutine = DoDelayedActivation();
        StartCoroutine(_coroutine);

    }



    protected virtual IEnumerator DoDelayedActivation()
    {
        // Now perform the derived class animator stuff
        foreach (AnimatorConfigurator configurator in _animations)
        {
            if (configurator != null)
            {
                foreach (AnimatorParameter param in configurator.AnimatorParams)
                {
                    // TODO: Support other types	
                    switch (param.Type)
                    {
                        case AnimatorParameterType.Bool:
                            bool boolean = bool.Parse(param.Value);
                            configurator.Animator.SetBool(param.Name, _activated ? boolean : !boolean);
                            break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(_activated ? _activationDelay : _deactivationDelay);

        // Set the states that should be set when activating / deactivating
        SetActivationStates();

        if (_activationSounds != null && !_activated)
        {
            AudioClip clipToPlay = _activationSounds[1];

            // If an audio source has been specified then use it. This is good for playing looping sounds
            // or sounds that need to happen nowhere near the tigger source
            if (_audioSource != null && clipToPlay && AudioManager.instance)
            {
                _audioSource.clip = clipToPlay;
                _audioSource.volume = _activationSounds.volume;
                _audioSource.spatialBlend = _activationSounds.spatialBlend;

                _audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName(_activationSounds.audioGroup);
                _audioSource.Play();
            }
        }

        // If we get here then we are allow to enable this object
        // so first turn on any game objects that should be made active by this 
        // action
        if (_objectActivators.Count > 0)
        {
            for (int i = 0; i < _objectActivators.Count; i++)
            {
                if (_objectActivators[i]) _objectActivators[i].SetActive(_activated);
            }
        }

        // Turn off any game objects that should be disabled by this action
        if (_objectDeactivators.Count > 0)
        {
            for (int i = 0; i < _objectDeactivators.Count; i++)
            {
                if (_objectDeactivators[i]) _objectDeactivators[i].SetActive(!_activated);
            }
        }

        // Activate Material Controller
        for (int i = 0; i < _materialControllers.Count; i++)
        {

            if (_materialControllers[i] != null)
            {
                _materialControllers[i].Activate(_activated);
            }
        }
    }


}
