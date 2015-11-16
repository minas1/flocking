using System;
using UnityEngine;

namespace SteeringBehaviors
{
    public class Arrive : Steering
    {
        public Entity character;
        public Entity target;
        
        public float satisfactionRadius; // the radius to stop when inside
        public float slowRadius; // the radius at which to slow down
        
        // constructor
        public Arrive(Entity _character, Entity _target, float _satisfactionRadius, float _slowRadius)
        {
            character = _character;
            target = _target;
            
            satisfactionRadius = _satisfactionRadius;
            slowRadius = _slowRadius;
        }
        
        public virtual SteeringOutput GetSteering()
        {
            SteeringOutput steering = new SteeringOutput();
            
            // get the direction to the target
            var direction = target.position - character.position;
            float distance = direction.magnitude;
            
            // if we are within the satisfaction radius, return no steering
            if( distance <= satisfactionRadius )
                return SteeringOutput.None;
            
            float targetSpeed = float.NaN;
            
            // if we are outside the slow radius, go full speed
            if( distance > slowRadius )
                targetSpeed = character.maxSpeed;
            else // otherwise calculate a scaled speed
                targetSpeed = character.maxSpeed * distance / slowRadius;
            
            // the target velocity combines speed and direction
            var targetVelocity = direction;
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;
            
            // acceleration tries to get to the target velocity
            steering.linearVel = targetVelocity - character.transform.forward;
            steering.linearVel /= 0.1f;
            
            if( steering.linearVel.magnitude > character.maxSpeed )
            {
                steering.linearVel.Normalize();
                steering.linearVel *= character.maxSpeed;
            }
            
            // face in the direction we want to move
            character.transform.LookAt(character.transform.position + direction);
            
            return steering;
        }
    }

}