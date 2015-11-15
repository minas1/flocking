using System;
using UnityEngine;

namespace SteeringBehaviors
{
	public class Seek : Steering
	{
		public Entity character;
		public Entity target;
		
		// constructor
		public Seek(Entity _character, Entity _target)
		{
			character = _character;
			target = _target;
		}
		
		public virtual SteeringOutput GetSteering()
		{
			var steering = new SteeringOutput();
			
			// get the direction to the target
			steering.linearVel = target.position - character.position;
			
			// the velocity is along this direction, at full speed
			steering.linearVel.Normalize();
			steering.linearVel *= character.maxSpeed;

			return steering;
		}
	}
}
