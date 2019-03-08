using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum AIStateType { None, Idle, Alerted, Firing, Attack, Pursuit, Flying, Dead}
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Robot, Visual_Blade, Audio}

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
    protected Dictionary<AIStateType, AIState> _states  = new Dictionary<AIStateType, AIState>();
    protected AITarget _target  = new AITarget();

    [SerializeField] protected SphereCollider   _targetTrigger  = null;
    [SerializeField] protected SphereCollider   _sensorTrigger  = null;

    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f;

    protected Animator      _animator   = null;
    protected NavMeshAgent  _navAgent    = null;
    protected Collider      _collider   = null;
    protected Transform     _transform  = null;

    //Public Properties
    public Animator     animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }

    // ----------------------------------------------------------------
    // Name : Start
    // Descrição: Setup the ai state machine
    // ----------------------------------------------------------------
    protected virtual void Start()
    {
        AIState[] states = GetComponents<AIState>();
        foreach(AIState state in states)
        {
            if(state!=null && !_states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state;
            }
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
    }
}
