using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Flocking;

public class Starling : Bird
{
    readonly float ANIMATION_SPEED = 0.75f;

    Animation anim;

	// Use this for initialization
	public override void Start()
	{
        anim = GetComponent<Animation>();
        foreach(AnimationState animState in anim)
        {
            animState.wrapMode = WrapMode.PingPong;
            animState.speed = ANIMATION_SPEED;

            animState.time = Random.value * animState.length;
        }

        maxSpeed = 20f;
	}

	// Update is called once per frame
	public override void Update()
	{
        if (state != null)
        {
            if (state.IsSingleBirdState)
                state.Update(Time.deltaTime, this);
            else
                ((MultipleBirdState)state).Update(Time.deltaTime);
        }

        if (velocity.normalized.y < -0.6f)
        {
            foreach (AnimationState animState in anim)
                animState.speed = 0f;
        }
        else
        {
            foreach (AnimationState animState in anim)
                animState.speed = ANIMATION_SPEED;
        }
    }
    
    public override void FixedUpdate()
	{
		if (state != null)
            state.FixedUpdate();
	}
}
