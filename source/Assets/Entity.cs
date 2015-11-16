using UnityEngine;
using System;

public interface Entity
{
    Vector3 velocity
    {
        get;
        set;
    }

    float maxSpeed
    {
        get;
        set;
    }

    Vector3 position
    {
        get;
        set;
    }

    Transform transform
    {
        get;
    }
    
    string tag
    {
        get;
    }

    string name
    {
        get;
    }
}