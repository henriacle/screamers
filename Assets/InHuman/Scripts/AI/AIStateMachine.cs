using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// Public Enums of the AI System
public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Fire, Feeding, Pursuit, Dead, Scream }
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType { Enter, Stay, Exit }

// ----------------------------------------------------------------------
// Class	:	AITarget
// Desc		:	Describes a potential target to the AI System
// ----------------------------------------------------------------------
public struct AITarget
{
    private AITargetType _type;         // The type of target
    private Collider _collider;     // The collider
    private Vector3 _position;      // Current position in the world
    private float _distance;        // Distance from player
    private Quaternion _rotation;        // angle between target
    private float _time;            // Time the target was last ping'd

    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position { get { return _position; } }
    public Quaternion rotation { get { return _rotation; } }
    public float distance { get { return _distance; } set { _distance = value; } }
    public float time { get { return _time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }

    public void Set(AITargetType t, Collider c, Vector3 p, float d, Quaternion r)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _rotation = r;
        _time = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        _time = 0.0f;
        _distance = Mathf.Infinity;
    }
}

// ----------------------------------------------------------------------
// Class	:	AIStateMachine
// Desc		:	Base class for all AI State Machines
// ----------------------------------------------------------------------
public abstract class AIStateMachine : MonoBehaviour
{
    // Public
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    // Protected
    protected AIState           _currentState = null;
    protected Dictionary<AIStateType, AIState> _states  = new Dictionary<AIStateType, AIState>();
    protected AITarget          _target                          = new AITarget();
    protected int               _rootPositionRefCount   = 0;
    protected int               _rootRotationRefCount   = 0;
    protected bool              _isTargetReached        = false;
    protected List<Rigidbody>   _bodyParts              = new List<Rigidbody>();
    protected bool              _isFireRange            = false;
    protected bool              _isMeleeRange           = false;
    protected int               _aiBodyPartLayer        = -1;

    // Protected Inspector Assigned
    [SerializeField] protected AIStateType          _currentStateType   = AIStateType.Idle;
    [SerializeField] Transform _rootBone                                = null;
    [SerializeField] protected SphereCollider       _targetTrigger      = null;
    [SerializeField] protected SphereCollider       _sensorTrigger      = null;
    [SerializeField] protected AIWaypointNetwork    _waypointNetwork    = null;
    [SerializeField] protected bool                 _randomPatrol       = false;
    [SerializeField] protected int                  _currentWaypoint    = -1;
    [SerializeField] [Range(0, 15)] protected float _stoppingDistance   = 1.0f;

    // Component Cache
    protected Animator _animator        = null;
    protected NavMeshAgent _navAgent    = null;
    protected Collider _collider        = null;
    protected Transform _transform      = null;

	// Animation Layer Manager
	protected Dictionary<string, bool>	_animLayersActive			= new Dictionary<string, bool>();

	// Layered Audio Control
	protected ILayeredAudioSource						_layeredAudioSource	=	null;


    // Public Properties
    public bool isTargetReached         { get { return _isTargetReached; } }
    public bool inMeleeRange { get { return _isMeleeRange; } set { _isMeleeRange = value; } }
    public AIWaypointNetwork waypointNetwork { get { return _waypointNetwork; } }
    public bool inFireRange             { get { return _isFireRange; }      set { _isFireRange = value; } }
    public Animator animator            { get { return _animator; } }
    public NavMeshAgent navAgent        { get { return _navAgent; } }
    public Vector3 sensorPosition
    {
        get
        {
            if (_sensorTrigger == null) return Vector3.zero;
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float sensorRadius
    {
        get
        {
            if (_sensorTrigger == null) return 0.0f;
            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x,
                                        _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);

            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }

    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
    public AITargetType targetType { get { return _target.type; } }
    public Vector3 targetPosition { get { return _target.position; } }
    public Quaternion rotationAngle { get { return _target.rotation; } }
    public int targetColliderID
    {
        get
        {
            if (_target.collider)
                return _target.collider.GetInstanceID();
            else
                return -1;
        }
    }

    // -----------------------------------------------------------------
    // Name	:	Awake
    // Desc	:	Cache Components
    // -----------------------------------------------------------------
    protected virtual void Awake()
    {
        // Cache all frequently accessed components
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        //get body part layer
        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        // Do we have a valid Game Scene Manager
        if (GameSceneManager.instance != null)
        {
            // Register State Machines with Scene Database
            if (_collider) GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }

        if (_rootBone != null)
        {
            Rigidbody[] bodies = _rootBone.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody bodypart in bodies)
            {
                if(bodypart != null && bodypart.gameObject.layer == _aiBodyPartLayer)
                {
                    _bodyParts.Add(bodypart);
                }
            }
        }
    }

    // -----------------------------------------------------------------
    // Name	:	Start
    // Desc	:	Called by Unity prior to first update to setup the object
    // -----------------------------------------------------------------
    protected virtual void Start()
    {
        // Set the sensor trigger's parent to this state machine
        if (_sensorTrigger != null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();
            if (script != null)
            {
                script.parentStateMachine = this;
            }
        }


        // Fetch all states on this game object
        AIState[] states = GetComponents<AIState>();

        // Loop through all states and add them to the state dictionary
        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
            {
                // Add this state to the state dictionary
                _states[state.GetStateType()] = state;

                // And set the parent state machine of this state
                state.SetStateMachine(this);
            }
        }

        // Set the current state
        if (_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }

        // Fetch all AIStateMachineLink derived behaviours from the animator
        // and set their State Machine references to this state machine
        if (_animator)
        {
            AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();
            foreach (AIStateMachineLink script in scripts)
            {
                script.stateMachine = this;
            }
        }
    }


    // -----------------------------------------------------------------------------
    // Name	:	GetWaypointPosition
    // Desc	:	Fetched the world space position of the state machine's currently
    //			set waypoint with optional increment
    // -----------------------------------------------------------------------------
    public Vector3 GetWaypointPosition(bool increment)
    {
        if (waypointNetwork == null)
            return Vector3.zero;

        if (_currentWaypoint == -1)
        {
            if (_randomPatrol)
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            else
                _currentWaypoint = 0;
        }
        else if (increment)
            NextWaypoint();

        // Fetch the new waypoint from the waypoint list
        if (_waypointNetwork.Waypoints[_currentWaypoint] != null)
        {
            Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint];

            // This is our new target position
            SetTarget(AITargetType.Waypoint,
                        null,
                        newWaypoint.position,
                        Vector3.Distance(newWaypoint.position, transform.position));

            return newWaypoint.position;
        }

        return Vector3.zero;
    }

    // -------------------------------------------------------------------------
    // Name	:	NextWaypoint
    // Desc	:	Called to select a new waypoint. Either randomly selects a new
    //			waypoint from the waypoint network or increments the current
    //			waypoint index (with wrap-around) to visit the waypoints in
    //			the network in sequence. Sets the new waypoint as the the
    //			target and generates a nav agent path for it
    // -------------------------------------------------------------------------
    private void NextWaypoint()
    {
        // Increase the current waypoint with wrap-around to zero (or choose a random waypoint)
        if (_randomPatrol && _waypointNetwork.Waypoints.Count > 1)
        {
            // Keep generating random waypoint until we find one that isn't the current one
            // NOTE: Very important that waypoint networks do not only have one waypoint :)
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint)
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            }
        }
        else
            _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1;


    }
    // -------------------------------------------------------------------
    // Name :	SetTarget (Overload)
    // Desc	:	Sets the current target and configures the target trigger
    // -------------------------------------------------------------------
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        // Set the target info
        _target.Set(t, c, p, d);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    // --------------------------------------------------------------------
    // Name :	SetTarget (Overload)
    // Desc	:	Sets the current target and configures the target trigger.
    //			This method allows for specifying a custom stopping
    //			distance.
    // --------------------------------------------------------------------
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        // Set the target Data
        _target.Set(t, c, p, d);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    // -------------------------------------------------------------------
    // Name :	SetTarget (Overload)
    // Desc	:	Sets the current target and configures the target trigger
    // -------------------------------------------------------------------
    public void SetTarget(AITarget t)
    {
        // Assign the new target
        _target = t;

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = t.position;
            _targetTrigger.enabled = true;
        }
    }

    public void ClearTarget()
    {
        _target.Clear();
        if (_targetTrigger != null)
        {
            _targetTrigger.enabled = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }

        _isTargetReached = false;
    }

    protected virtual void Update()
    {
        if (_currentState == null) return;

        AIStateType newStateType = _currentState.OnUpdate();
        if (newStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else
            if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) {
            return;
        };

        _isTargetReached = true;

        // Notify Child State
        if (_currentState)
            _currentState.OnDestinationReached(true);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) {
            return;
        };

        _isTargetReached = true;
    }

    protected void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other) return;

        _isTargetReached = false;

        if (_currentState != null)
            _currentState.OnDestinationReached(false);
    }

    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (_currentState != null)
            _currentState.OnTriggerEvent(type, other);
    }

    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null)
            _currentState.OnAnimatorUpdated();
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
            _currentState.OnAnimatorIKUpdated();
    }

    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

	public void SetLayerActive (string layerName, bool active )
	{
		 _animLayersActive[layerName] = active;
		if (active==false && _layeredAudioSource!=null)
		 	_layeredAudioSource.Stop( _animator.GetLayerIndex(layerName) );
	} 

	public bool IsLayerActive( string layerName ) 
	{
		bool result;
		if (_animLayersActive.TryGetValue(layerName, out result)) 
		{
			return result;
		}
		return false;
	}

	public bool PlayAudio(AudioCollection clipPool, int bank, int layer, bool looping=true )
	{
		if (_layeredAudioSource==null) return false;
		return _layeredAudioSource.Play( clipPool, bank, layer, looping );
	}

	public void StopAudio( int layer )
	{
		if (_layeredAudioSource!=null)
			_layeredAudioSource.Stop( layer );
	}

	public void MuteAudio( bool mute )
	{
		if (_layeredAudioSource!=null)
			_layeredAudioSource.Mute( mute );
	}    

    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }

    public virtual void takeDamage(Vector3 position, float force, float damage, Rigidbody bodyPart, int hitDirection = 0)
    {

    }
}
