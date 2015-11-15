using System.Collections.Generic;
using System;
using UnityEngine;
using SteeringBehaviors;

public class CrowWander : SingleBirdState
{
	List<Collider> colliders;
	string[] enemyTags;

	public CrowWander(Bird bird, string[] _enemyTags) : base(bird)
	{
		colliders = new List<Collider>();

		float lookaheadDistance = 20f;
	
		// wander state combines obstacle avoidance and wander behaviors

		var blended = new BlendedSteering[2];
		blended[0] = new BlendedSteering(bird, new BehaviorAndWeight(new ObstacleAvoidance (bird, lookaheadDistance, new string[]{"Untagged", "Ground"}), 2f));
		//blended[1] = new BlendedSteering(maxAccel, new BehaviorAndWeight(new SteeringBehaviors.Wander(bird, maxAccel), 1f));
		behavior = new PrioritySteering (1f, blended);

		enemyTags = _enemyTags;
	}

    public override void Update(float dt, Bird bird)
	{
		colliders.Clear();

		if (hasVisionOf (bird, enemyTags[0], new string[]{"Ground", "Untagged"}, colliders))
		{
			bird.state = new CrowHunt(bird, enemyTags);
		}

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