using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHumanRobot_Fire1 : AIHumanRobotState
{
    [SerializeField] [Range(0, 10)] float _speed = 0.0f;
    [SerializeField] float _stoppingDistance = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] float _slerpSpeed = 5.0f;
    private OriginalWeaponSystem _weaponSystem = null;
    private Weapon _npcWeapon = null;

    // Private Variables
    private float _currentLookAtWeight = 0.0f;


    public override AIStateType GetStateType()
    {
        return AIStateType.Fire;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Fire State");
        if (_humanRobotStateMachine == null)
            return;

        _humanRobotStateMachine.NavAgentControl(true, false);
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

    public void gameObjectHit(HitInfo hit)
    {
        _humanRobotStateMachine.takeDamage(hit.hit.point, 5.0f, hit.damage, hit.hit.rigidbody);
    }

    IEnumerator OnEasyWeaponsFire()
    {
        Debug.Log("fire !");
        yield return new WaitForSeconds(1.5f);
        _npcWeapon.canFire = true;
    }

    IEnumerator OnEasyWeaponsReload()
    {
        Debug.Log("reload");
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

        if (Vector3.Distance(_humanRobotStateMachine.transform.position, _humanRobotStateMachine.targetPosition) < _stoppingDistance)
        {
            _humanRobotStateMachine.speed = 0;
        }
        else
        {
            _humanRobotStateMachine.speed = _speed;
        }


        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _humanRobotStateMachine.SetTarget(_stateMachine.VisualThreat);

            if (_humanRobotStateMachine.inMeleeRange)
            {
                return AIStateType.Attack;
            };

            Debug.Log(_humanRobotStateMachine.inFireRange);
            if (!_humanRobotStateMachine.inFireRange)
            {
                return AIStateType.Pursuit;
            };

            if(!_humanRobotStateMachine.reloading) {
                _humanRobotStateMachine.firing = true;
            }

            if (!_humanRobotStateMachine.useRootRotation)
            {
                targetPos = _humanRobotStateMachine.targetPosition;
                targetPos.y = _humanRobotStateMachine.transform.position.y;
                newRot = Quaternion.LookRotation(targetPos - _humanRobotStateMachine.transform.position);
                _humanRobotStateMachine.transform.rotation = Quaternion.Slerp(_humanRobotStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

                _humanRobotStateMachine.attackType = 0;

                if (_npcWeapon.currentAmmo > 0 && _npcWeapon.canFire)
                {
                    _npcWeapon.RemoteFire();
                }

                return AIStateType.Fire;
            }
        }

        return AIStateType.Alerted;
    }

}
