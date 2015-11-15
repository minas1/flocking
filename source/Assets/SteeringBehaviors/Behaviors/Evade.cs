using System;
using UnityEngine;

public class Evade : Flee
{
	public Entity explicitTarget;
	
	public float maxPrediction; // holds the maximum prediction time
	
	// constructor
	public Evade(Entity _character, Entity _explicitTarget, float _maxPrediction)
		: base(_character, _explicitTarget)
	{
		explicitTarget = _explicitTarget;
		maxPrediction = _maxPrediction;
	}
	
	public override SteeringOutput GetSteering()
	{
		// work out the distance to the target
		var direction = target.position - character.transform.position;
		float distance = direction.magnitude;
		
		// get our current speed
		float speed = character.velocity.magnitude;
		
		float prediction = float.NaN;
		if( speed < distance / maxPrediction )
			prediction = maxPrediction;
		else
			prediction = distance / speed;
		
		// put the target together
		var oldTargetPosition = target.position;
		target.position += target.velocity * prediction;
		
		SteeringOutput steering = base.GetSteering();
		
		target.position = oldTargetPosition;
		return steering;
	}
}

