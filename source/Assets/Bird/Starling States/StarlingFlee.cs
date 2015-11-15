using System.Collections.Generic;
using UnityEngine;

using SteeringBehaviors;
using Flocking;

public class StarlingFlee : SingleBirdState
{
	Entity targetToRunFrom;
	
	List<Collider> colliders;


	public StarlingFlee(Bird bird, Entity _targetToRunFrom) : base(bird)
	{
		targetToRunFrom = _targetToRunFrom;
		colliders = new List<Collider> ();

		behavior = new Flee(bird, targetToRunFrom);
	}
	
    public override void Update(float dt, Bird bird)
	{
		colliders.Clear();

		UpdateSteering(dt);
	}
	
	public override void FixedUpdate()
	{
	}

	public override void onCollisionEnter(Collision other)
	{
	}
	
	public override void onCollisionStay(Collision other)
	{
	}
	
	public override void onCollisionExit(Collision other)
	{
	}
}

