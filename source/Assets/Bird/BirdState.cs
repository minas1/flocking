using System;
using System.Collections.Generic;
using UnityEngine;

public interface BirdState
{
	void Update(float dt, Bird bird);
	void FixedUpdate();

	void onCollisionEnter(Collision collision);
	void onCollisionStay(Collision collision);
	void onCollisionExit(Collision collision);

    bool IsSingleBirdState
    {
        get;
    }
}


