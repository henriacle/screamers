using UnityEngine;
using System.Collections;

public class AIHumanRobot_alerted1 : AIHumanRobotState
{
    // Inspector Assigned
    [SerializeField] [Range(1, 60)] float _maxDuration = 10.0f;
    [SerializeField] float _waypointAngleThreshold = 90.0f;
    [SerializeField] float _threatAngleThreshold = 10.0f;
    [SerializeField] float _directionChangeTime = 1.5f;
    [SerializeField] private Material _eyeColor = null;
    [SerializeField] private GameObject leftEye = null;
    [SerializeField] private GameObject rightEye = null;
    Material leftEyeMaterial = null;
    Material rightEyeMaterial = null;
    private float _eyeIntensity = 5.0f;

    // Private Fields
    float _timer = 0.0f;
    float _directionChangeTimer = 0.0f;

    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public void setEyeColor(Color color, float intensity)
    {
        if (leftEyeMaterial != null && rightEyeMaterial != null)
        {
            leftEyeMaterial.SetColor("_EmissionColor", color * _eyeIntensity);
            rightEyeMaterial.SetColor("_EmissionColor", color * _eyeIntensity);
        }
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes a timer and configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_humanRobotStateMachine == null)
            return;

        // Configure State Machine
        _humanRobotStateMachine.NavAgentControl(true, false);
        _humanRobotStateMachine.speed = 0;
        _humanRobotStateMachine.seeking = 0;
        _humanRobotStateMachine.firing = false;
        _humanRobotStateMachine.reloading = false;
        _humanRobotStateMachine.attackType = 0;

        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
    }

    // ---------------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Reduce Timer
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        // Transition into a patrol state if available
        if (_timer <= 0.0f)
        {
            if(_humanRobotStateMachine.navAgent.enabled)
            {
                _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(false));
                _humanRobotStateMachine.navAgent.isStopped = false;
            }
            _timer = _maxDuration;
        }

        // Do we have a visual threat that is the player. These take priority over audio threats
        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_humanRobotStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            _timer = _maxDuration;
        }

        if (_humanRobotStateMachine.AudioThreat.type == AITargetType.None &&
            _humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Food &&
            _humanRobotStateMachine.targetType == AITargetType.None)
        {
            _humanRobotStateMachine.SetTarget(_stateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        float angle;

        if ((_humanRobotStateMachine.targetType == AITargetType.Audio || _humanRobotStateMachine.targetType == AITargetType.Visual_Light) && !_humanRobotStateMachine.isTargetReached)
        {
            angle = AIState.FindSignedAngle(_humanRobotStateMachine.transform.forward,
                                            _humanRobotStateMachine.targetPosition - _humanRobotStateMachine.transform.position);

            if (_humanRobotStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < _threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }

            if (_directionChangeTimer > _directionChangeTime)
            {
                setEyeColor(Color.yellow, _eyeIntensity);
                if (Random.value < _humanRobotStateMachine.intelligence)
                {
                    _humanRobotStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _humanRobotStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }

                _directionChangeTimer = 0.0f;
            }
        }

        else

        if (_humanRobotStateMachine.targetType == AITargetType.Waypoint && !_humanRobotStateMachine.navAgent.pathPending)
        {
            angle = AIState.FindSignedAngle(_humanRobotStateMachine.transform.forward,
                                            _humanRobotStateMachine.navAgent.steeringTarget - _humanRobotStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;


            if (_directionChangeTimer > _directionChangeTime)
            {
                _humanRobotStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0.0f;
            }
        } else
        {
            if (_directionChangeTimer > _directionChangeTime)
            {
                setEyeColor(Color.yellow, _eyeIntensity);
                _humanRobotStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0.0f;       
            }
        }

        if(_directionChangeTimer > _directionChangeTime - 0.50f)
        {
            setEyeColor(Color.blue, _eyeIntensity);
            _humanRobotStateMachine.seeking = 0;
        }

        return AIStateType.Alerted;
    }
}
