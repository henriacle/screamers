using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHumanRobot_Fire1 : AIHumanRobotState
{
    [SerializeField] [Range(0, 10)] float _speed = 0.0f;
    [SerializeField] float _stoppingDistance = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] float keepMinDistance = 5f;
    [SerializeField] float keepMaxDistance = 9f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] float _slerpSpeed = 5.0f;
    private OriginalWeaponSystem _weaponSystem = null;
    private Weapon _npcWeapon = null;
    [SerializeField] Transform chest;
    private Transform rightHand;
    private Transform leftHand;

    // Private Variables
    private float _currentLookAtWeight = 0.0f;
    public Vector3 Offset;

    public override AIStateType GetStateType()
    {
        return AIStateType.Fire;
    }

    public override void OnEnterState()
    {
        if (_humanRobotStateMachine == null)
            return;

        chest = _humanRobotStateMachine.animator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand   = _humanRobotStateMachine.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        leftHand    = _humanRobotStateMachine.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        _humanRobotStateMachine.NavAgentControl(true, false);
        _humanRobotStateMachine.speed = _speed;
        _humanRobotStateMachine.seeking = 0;
        _humanRobotStateMachine.firing = true;
        _humanRobotStateMachine.attackType = 0;
        _humanRobotStateMachine.reloading = false;
        _weaponSystem = gameObject.GetComponentInChildren<OriginalWeaponSystem>();
        _npcWeapon = _weaponSystem.weapons[_weaponSystem.weaponIndex].GetComponent<Weapon>();
        _npcWeapon.reloadAutomatically = true;
    }

    public override void OnExitState()
    {
        _humanRobotStateMachine.attackType = 0;
    }

    public void OnEasyWeaponsLaunch()
    {
        Debug.Log("easy weapons launch");
    }

    IEnumerator OnEasyWeaponsFire()
    {
        yield return new WaitForSeconds(1.5f);
        _npcWeapon.canFire = true;
    }

    IEnumerator OnEasyWeaponsReload()
    {
        _humanRobotStateMachine.reloading = true;
        yield return new WaitForSeconds(1.5f);
        _npcWeapon.currentAmmo = _npcWeapon.ammoCapacity;
        _humanRobotStateMachine.reloading = false;
        _npcWeapon.canFire = true;
    }

    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        if (Vector3.Distance(_humanRobotStateMachine.transform.position, _humanRobotStateMachine.targetPosition) < keepMinDistance)
        {
            _humanRobotStateMachine.speed = 4;
        } else if (Vector3.Distance(_humanRobotStateMachine.transform.position, _humanRobotStateMachine.targetPosition) > keepMaxDistance)
        {
            _humanRobotStateMachine.speed = _speed;
        } else
        {
            _humanRobotStateMachine.speed = 0;
        }


        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _humanRobotStateMachine.SetTarget(_stateMachine.VisualThreat);

            if (_humanRobotStateMachine.inMeleeRange)
            {
                return AIStateType.Attack;
            };

            if (!_humanRobotStateMachine.inFireRange)
            {
                return AIStateType.Pursuit;
            };

            if(!_humanRobotStateMachine.reloading) {
                _humanRobotStateMachine.firing = true;
            }

            targetPos = _humanRobotStateMachine.targetPosition;
            targetPos.y = _humanRobotStateMachine.transform.position.y - 20.0f;
            newRot = Quaternion.LookRotation(targetPos - _humanRobotStateMachine.transform.position);
            _humanRobotStateMachine.transform.rotation = Quaternion.Slerp(_humanRobotStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

            _humanRobotStateMachine.attackType = 0;

            if (_npcWeapon.currentAmmo > 0 && _npcWeapon.canFire)
            {
                _npcWeapon.AIFiring();
            }

            return AIStateType.Fire;
        } else
        {
            if(_humanRobotStateMachine.targetType == AITargetType.Visual_Player)
            {
                return AIStateType.Pursuit;
            }
        }

        return AIStateType.Alerted;
    }

    public override void LateUpdate()
    {
        if (!_humanRobotStateMachine) return;

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player && _humanRobotStateMachine.targetPosition != null && chest != null)
        {
            chest.LookAt(_humanRobotStateMachine.targetPosition);
            chest.rotation = chest.rotation * Quaternion.Euler(Offset);
        }
    }
}
