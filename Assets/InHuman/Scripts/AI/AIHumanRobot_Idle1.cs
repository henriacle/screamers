﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHumanRobot_Idle1 : AIHumanRobotState
{
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);

    float _idleTime = 0.0f;
    float _timer    = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_humanRobotStateMachine == null)
            return;

        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        _humanRobotStateMachine.NavAgentControl(true, false);

        _humanRobotStateMachine.speed   = 0;
        _humanRobotStateMachine.seeking = 0;
        _humanRobotStateMachine.firing = false;
        _humanRobotStateMachine.reloading = false;
        _humanRobotStateMachine.crouching = false;

        _humanRobotStateMachine.ClearTarget();
    }

    public override AIStateType OnUpdate()
    {
        if (_humanRobotStateMachine == null)
            return AIStateType.Idle;

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if(_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        if(_humanRobotStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        if(_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        _timer += Time.deltaTime;
        if(_timer > _idleTime)
        {
            return AIStateType.Patrol;
        }

        return AIStateType.Idle;
    }
}