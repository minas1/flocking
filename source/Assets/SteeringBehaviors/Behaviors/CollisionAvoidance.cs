using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    public class CollisionAvoidance : Steering
    {
        public Entity character;
        public List<Entity> targets;
        
        public float maxAcceleration;
        public float radius; // collision radius
        
        public CollisionAvoidance(Entity _character, List<Entity> _targets, float _maxAcceleration, float _radius)
        {
            character = _character;
            targets = _targets;
            maxAcceleration = _maxAcceleration;
            radius = _radius;
        }
        
        public SteeringOutput GetSteering()
        {
            // find the target that's closest to collision
            
            // store the first collision time
            float shortestTime = float.PositiveInfinity;
            
            // store the target that collides then, and other data
            // that we will need and can avoid re-calculating
            Entity firstTarget = null;
            float firstMinSeparation = float.NaN, firstDistance = float.NaN;
            Vector3 firstRelativePos = Vector3.zero, firstRelativeVel = Vector3.zero;
            
            // loop through each target
            foreach(var target in targets)
            {
                // calculate the time to collision
                var relativePos = -(target.transform.position - character.transform.position);
                var relativeVel = (target.velocity - character.velocity);
                var relativeSpeed = relativeVel.magnitude;
                var timeToCollision = Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);
                
                // check if it is going to be a collision at all
                var distance = relativePos.magnitude;
                var minSeparation = distance - relativeSpeed * timeToCollision;
                
                Debug.Log("t = " + timeToCollision + " dist = " + relativePos.magnitude);
                
                if( minSeparation > 2 * radius )
                    continue;
                
                // check if it is the shortest
                if( timeToCollision > 0f && timeToCollision < shortestTime )
                {
                    // store the time, target and other data
                    shortestTime = timeToCollision;
                    firstTarget = target;
                    firstMinSeparation = minSeparation;
                    firstDistance = distance;
                    firstRelativePos = relativePos;
                    firstRelativeVel = relativeVel;
                }
            }
            
            // calculate the steering
            
            // if we have no target, then exit
            if( firstTarget == null )
                return SteeringOutput.None;
            
            Vector3 relPos;
            
            // if we're going to hit exactly, or if we're already colliding, then do the steering based on current position
            if( firstMinSeparation <= 0f || firstDistance < 2 * radius )
                relPos = firstTarget.transform.position - character.transform.position;
            else
                relPos = firstRelativePos + firstRelativeVel * shortestTime;
            
            
            var steering = new SteeringOutput();
            
            // avoid the target
            relPos.Normalize();
            steering.linearVel = -relPos * maxAcceleration;
            
            // face in the direction we want to move
            character.transform.LookAt(character.transform.position + character.velocity);
            
            return steering;
        }

        public void Destroy()
        {
        }
    }
}

