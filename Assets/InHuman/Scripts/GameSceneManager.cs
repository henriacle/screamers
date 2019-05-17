using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    //statics
    private static GameSceneManager _instance = null;
    public static GameSceneManager instance
    {
        get
        {
            if (_instance == null)
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
            return _instance;
        }
    }

    //private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();
    private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();

    //Public Methods
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if(!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if(_stateMachines.TryGetValue(key, out machine))
        {
            return machine;
        }
        return null;
    }

    // --------------------------------------------------------------------
    // Name	:	RegisterInteractiveItem
    // Desc	:	Stores the passed Interactive Item reference in the 
    //			dictionary with the supplied key (usually the instanceID of
    //			a collider)
    // --------------------------------------------------------------------
    public void RegisterInteractiveItem(int key, InteractiveItem script)
    {
        if (!_interactiveItems.ContainsKey(key))
        {
            _interactiveItems[key] = script;
        }
    }

    // --------------------------------------------------------------------
    // Name	:	GetInteractiveItem
    // Desc	:	Given a collider instance ID returns the
    //			Interactive Item_Base derived object attached to it.
    // --------------------------------------------------------------------
    public InteractiveItem GetInteractiveItem(int key)
    {
        InteractiveItem item = null;
        _interactiveItems.TryGetValue(key, out item);
        return item;
    }
}
