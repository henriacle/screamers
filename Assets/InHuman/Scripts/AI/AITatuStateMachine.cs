using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITatuStateMachine : AIStateMachine
{
    //Inspector custom parameters
    [SerializeField] [Range(10.0f, 360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _agression = 0.5f;
    [SerializeField] [Range(0, 100)] float _health = 100.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 1.0f;

    private int     _seeking = 0;
    private float   _speed = 0;
    private GameSceneManager _gameSceneManager = null;

    //public properties
    public float fov { get { return _fov; } }
    public float sight { get { return _sight; } }
    public float hearing { get { return _hearing; } }
    public float intelligence { get { return _intelligence; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float health { get { return _health; } set { _health = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }
}
