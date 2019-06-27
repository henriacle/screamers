using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITatuIdle : AITatuState
{
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);

    float _idleTime = 0.0f;
    float _timer = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_tatuStateMachine == null)
            return;

        _tatuStateMachine.headbutt = 1;

        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        _tatuStateMachine.NavAgentControl(true, false);

        _tatuStateMachine.speed = 0;
        _tatuStateMachine.seeking = 0;

        _tatuStateMachine.ClearTarget();
    }

    public override AIStateType OnUpdate()
    {
        if (_tatuStateMachine == null)
            return AIStateType.Idle;

        if (_tatuStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            Debug.Log("visual threat");
            _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        return AIStateType.Idle;
    }
}
