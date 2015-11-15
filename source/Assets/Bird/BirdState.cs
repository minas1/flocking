using System;
using System.Collections.Generic;
using UnityEngine;

public interface BirdState
{
	void Update(float dt, Bird bird);
	void FixedUpdate();

    bool IsSingleBirdState
    {
        get;
    }
}