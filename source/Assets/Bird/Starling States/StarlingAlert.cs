using System.Collections.Generic;
using Flocking;
using UnityEngine;
using SteeringBehaviors;

public class StarlingAlert : SingleBirdState
{
	List<Collider> colliders;
	string[] enemyTags;
	
	public StarlingAlert(Bird bird, string[] _enemyTags) : base(bird)
	{
		//behavior = new SteeringBehaviors.Wander(bird, 20f, 5f, 2f);
		colliders = new List<Collider>();
		enemyTags = _enemyTags;
	}
	
    public override void Update(float dt, Bird bird)
	{
		colliders.Clear();
		
		if( hasVisionOf(bird, enemyTags[0], new string[]{"Ground", "Untagged"}, colliders) )
		{
			bird.state = new StarlingHunt(bird, enemyTags);
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

