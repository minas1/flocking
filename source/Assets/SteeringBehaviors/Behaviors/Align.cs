using System;
using UnityEngine;

namespace SteeringBehaviors
{
	public class Align : Steering
	{
		public Entity character;
		public Entity target;
		
		// constructor
		public Align(Entity _character, Entity _target)
		{
			character = _character;
			target = _target;
		}
		
		public SteeringOutput GetSteering()
		{
			SteeringOutput steering = new SteeringOutput();
			
			steering.linearVel = Vector3.zero;
			//steering.rotation = Vector3.zero;
			
			// face in the direction that the target is facing
			character.transform.LookAt(character.transform.position + target.transform.forward);
			
			return steering;
		}

        public void Destroy()
        {
        }
	}
}