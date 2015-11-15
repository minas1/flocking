using System;
using UnityEngine;

namespace SteeringBehaviors
{
	public class Pursue : Seek
	{
		public float maxPrediction; // holds the maximum prediction time
		
		// constructor
		public Pursue(Entity _character, Entity _explicitTarget, float _maxPrediction)
			: base(_character, _explicitTarget)
		{
			maxPrediction = _maxPrediction;
		}
		
		public override SteeringOutput GetSteering()
		{
			// work out the distance to the target
			var direction = target.position - character.transform.position;
			float distance = direction.magnitude;

			// get current speed
			float speed = character.velocity.magnitude;
			
			float prediction = float.NaN;
			if( speed < distance / maxPrediction )
				prediction = maxPrediction;
			else
				prediction = distance / speed;
			
			// put the target together
			var oldTargetPosition = target.position;
			target.position += target.velocity * prediction;
			
			var steering = base.GetSteering();
			
			target.position = oldTargetPosition;
			return steering;
		}
	}
}