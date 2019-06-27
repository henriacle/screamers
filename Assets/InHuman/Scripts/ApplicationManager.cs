using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
	public string Key	=	null;
	public string Value	=	null;
}

[System.Serializable]
public class DialogConfig {
    public bool triggerPlayerChoice = false;
    public List<Choice> Choices     = null;
    public bool WaitingForAnswer    = false;
    public int Answer               = -1;
}

[System.Serializable]
public class Choice
{
    public string Text = null;
    public int jumpToDialogIndex = 0;
    public bool updateDatabase = false;
    public List<GameState> newStates;
}

[System.Serializable]
public class DialogState
{
    public List<AnimatorParameter> AnimatorParams = new List<AnimatorParameter>();
    public string           Text                    = null;
    public bool             IncrementDialog         = true;
    public bool             resetDialog             = true;
    public DialogConfig     Value                   = null;
    public bool             FinishDialog            = false;
    public bool             WaitForVariableToChange = false;
    public List<GameState>  GameStateToCheck        = null;
    public bool             AreGameStatesSet        = false;
    public List<GameState>  GameStateToSet          = null;
    public int              jumpToDialogIndex       = 0;
    public int              lastAnswer              = 0;
}

[System.Serializable]
public class Choices
{
    public string text = null;
    public bool status = false;
}

public class ApplicationManager : MonoBehaviour 
{
	
	// Inspector Assigned
	// This holds any states you wish set at game startup
	[SerializeField] private List<GameState>	_startingGameStates		= new List<GameState>();

	// Used to store the key/values pairs in the above list in a more efficient dictionary for runtime lookup
	private Dictionary<string, string>			_gameStateDictionary		= new Dictionary<string, string>();

	// Singleton Design
	private static ApplicationManager _Instance		= null;
	public static ApplicationManager instance
	{
		get { 
			// If we don't an instance yet find it in the scene hierarchy
			if (_Instance==null) { _Instance = (ApplicationManager)FindObjectOfType(typeof(ApplicationManager)); }
			
			// Return the instance
			return _Instance;
		}
	}

	void Awake()
	{

		// This object must live for the entire application
		DontDestroyOnLoad(gameObject);

		// Copy starting game states into game state dictionary
		for (int i=0; i<_startingGameStates.Count;i++)
		{
			GameState gs = _startingGameStates[i];
			_gameStateDictionary[gs.Key] = gs.Value;
		}
	}

	// ----------------------------------------------------------------------------------------------
	// Name	:	GetGameState
	// Desc	:	Returns the value of a game state
	// ----------------------------------------------------------------------------------------------
	public string GetGameState( string key )
	{
		string result = null;
		_gameStateDictionary.TryGetValue( key, out result);
		return result;
	}

	// ----------------------------------------------------------------------------------------------
	// Name	:	SetGameState
	// Desc	:	Sets a Game State
	// ----------------------------------------------------------------------------------------------
	public bool SetGameState( string key, string value )
	{
		if (key==null || value==null) return false;

		_gameStateDictionary[key] = value;

	
		return true;
	}
	
}
