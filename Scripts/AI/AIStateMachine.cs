using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum AIStateType { None, Idle, Alerted, Firing, Attack, Pursuit, Flying, Dead}
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Robot, Visual_Blade, Audio}
public enum AITriggerEventType { Enter, Stay, Exit }

public struct AITarget
{
    private AITargetType    _type;      // The type of the target
    private Collider        _collider;  // The collider
    private Vector3         _position;  // Current Position in the world
    private float           _distance;  // Distance from the player
    private float           _time;      // Tempo para mudar de ação

    public AITargetType     type        { get { return _type; } }
    public Collider         collider    { get { return _collider; } }
    public Vector3          position    { get { return _position; } }
    public float            distance    { get { return _distance; } set { _distance = value; } }
    public float            time        { get { return _time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _type       = t;
        _collider   = c;
        _position   = p;
        _distance   = d;
        _time       = Time.time;
    }

    public void Clear()
    {
        _type       = AITargetType.None;
        _collider   = null;
        _position   = Vector3.zero;
        _time       = 0.0f;
        _distance   = Mathf.Infinity;
    }
}

public abstract class AIStateMachine : MonoBehaviour
{
    //public 
    public AITarget VisualThreat    = new AITarget();
    public AITarget AudioThreat     = new AITarget();

    //protected
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states  = new Dictionary<AIStateType, AIState>();
    protected AITarget _target  = new AITarget();

    [SerializeField]    protected AIStateType       _currentStateType   = AIStateType.Idle;
    [SerializeField]    protected SphereCollider    _targetTrigger      = null;
    [SerializeField]    protected SphereCollider    _sensorTrigger      = null;

    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f;

    protected Animator      _animator   = null;
    protected NavMeshAgent  _navAgent    = null;
    protected Collider      _collider   = null;
    protected Transform     _transform  = null;

    //Public Properties
    public Animator     animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }
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

    protected virtual void Awake()
    {
        _transform  = transform;
        _animator   = GetComponent<Animator>();
        _navAgent   = GetComponent<NavMeshAgent>();
        _collider   = GetComponent<Collider>();

        if(GameSceneManager.instance != null)
        {
            if (_collider) GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }

        //register state machines with scene database
    }

    // ----------------------------------------------------------------
    // Name : Start
    // Descrição: Setup the ai state machine
    // ----------------------------------------------------------------
    protected virtual void Start()
    {
        if(_sensorTrigger!=null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();
            if(script != null)
            {
                script.parentStateMachine = this;
            }
        }

        AIState[] states = GetComponents<AIState>();
        foreach(AIState state in states)
        {
            if(state!=null && !_states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
        }

        if(_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        } else
        {
            _currentState = null;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        _target.Set(t, c, p, d);

        if(_targetTrigger!=null)
        {
            _targetTrigger.radius               = _stoppingDistance;
            _targetTrigger.transform.position   = _target.position;
            _targetTrigger.enabled              = true;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        _target.Set(t, c, p, d);

        if (_targetTrigger != null)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        _target = t;

        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void Clear()
    {
        _target.Clear();
        if(_targetTrigger)
        {
            _targetTrigger.enabled = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if(_target.type!=AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }
    }

    protected virtual void Update()
    {
        if (_currentState == null) return;

        AIStateType newStateType = _currentState.OnUpdate();
        if(newStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    //Name: OnTriggerEnter
    //Desc: Called by the Physics system when the AI's main collider enters it's trigger, and notify children about
    //      the last player position
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        if(_currentState)
        {
            _currentState.OnDestinationReached(true);
        }
    }

    //Name: OnTriggerEnter
    //Desc: Called by the Physics system when the AI's main collider exits it's trigger, and notify children about
    //      the last player position
    public void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other) return;

        if (_currentState!=null)
            _currentState.OnDestinationReached(true);
    }

    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if(_currentState != null)
            _currentState.OnTriggerEvent(type, other);
    }

    protected virtual void OnAnimatorMove()
    {
        if(_currentState!=null)
            _currentState.OnAnimatorUpdated();
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
            _currentState.OnAnimatorIKUpdated();
    }

    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if(_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }
}
