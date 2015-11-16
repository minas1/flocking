using System;
using UnityEngine;
using System.Collections.Generic;

public class DummyEntity : Entity
{
    public Vector3 _position;
    public Vector3 _velocity;

    public DummyEntity()
    {
    }

    public void Destroy()
    {
    }
    
    public Vector3 velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    public Vector3 position
    {
        get { return _position; }
        set { _position = value; }
    }

    public float maxSpeed
    {
        get { return float.NaN; }
        set { Debug.Log("DummyEntity.maxSpeed [set] is not defined"); }
    }

    public string tag
    {
        get { return "DummyEntity"; }
    }

    public string name
    {
        get { return "DummyEntity"; }
    }
    
    public Transform transform
    {
        get { return null; }
    }
}

