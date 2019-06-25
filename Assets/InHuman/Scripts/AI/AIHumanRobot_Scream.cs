using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHumanRobot_Scream : AIHumanRobotState
{
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);
    [SerializeField] AudioSource _scream    = null;

    float _idleTime = 0.0f;
    float _timer    = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Scream;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_humanRobotStateMachine == null)
            return;


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

        StartCoroutine(Scream());

        return AIStateType.Patrol;
    }

    IEnumerator Scream()
    {
        _humanRobotStateMachine.animator.SetBool("Scream", true);
        yield return new WaitForSeconds(1.3f);
        _humanRobotStateMachine.animator.SetBool("Scream", false);
    }
}
