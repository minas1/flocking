using System.Collections.Generic;
using UnityEngine;

using SteeringBehaviors;
using Flocking;

public class StarlingAttack : SingleBirdState
{
	public readonly static float ATTACK_DISTANCE = 30f; // distance in which a bird can attack a target
	readonly static float ATTACK_TIME = 5f; // time in attack state
	
	readonly static float SPEED_MULTIPLIER = 4f; // how many times faster the bird flies while attacking
	
	Path path;
	PathFollow pathFollow;
	float timeAttacking = ATTACK_TIME;
	
	Entity target;
	
	List<Collider> colliders;
	string[] enemyTags;

	LineRenderer lineRenderer;

	public StarlingAttack(Bird bird, Entity _target, string[] _enemyTags) : base(bird)
	{
		target = _target;
		bird.maxSpeed *= SPEED_MULTIPLIER;
		
		// calculate where the target will be in the near future
		Vector3 targetPosition = target.transform.position + target.velocity * 1f;
		
		// the path-follow behaviour is used here to move to the position the target was
		path = new Path();
		path.path.Add(targetPosition);
		path.path.Add(targetPosition + (targetPosition - bird.transform.position) * 100f);
		
		behavior = pathFollow = new PathFollow(bird, path);
		
		// calculate how much time we'll need to travel to the target position
		var timeToReach = (targetPosition - bird.transform.position).magnitude / bird.maxSpeed;

		timeToReach *= 3; //  multiply by a value keep moving in that direction for a longer period, as it seems more natural
		timeAttacking = timeToReach;
		
		colliders = new List<Collider>();
		lineRenderer = bird.GetComponent<LineRenderer>();
		lineRenderer.SetPosition(1, path.GetPosition(1));
		lineRenderer.enabled = true;

		enemyTags = _enemyTags;
	}
	
    public override void Update(float dt, Bird bird)
	{
		lineRenderer.SetPosition(0, bird.transform.position + bird.transform.forward * 1f);

		timeAttacking -= dt;
		if( timeAttacking <= 0f )
		{
			DoneAttacking();
		}
		
		colliders.Clear();
		
		UpdateSteering(dt);
	}
	
	public override void FixedUpdate()
	{
	}

	/// <summary>
	/// Called when attacking is done and the state must be changed
	/// </summary>
	private void DoneAttacking()
	{
		bird.maxSpeed /= SPEED_MULTIPLIER; // restore the bird's max speed
		bird.state = new StarlingAlert(bird, enemyTags);

		var lineRenderer = bird.GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
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

