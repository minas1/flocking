using System.Collections.Generic;
using UnityEngine;

using Flocking;

namespace SteeringBehaviors
{
    public class VelocityMatch : BoidSteering
    {
        public bool useOldValues = false;
        Vector3 center, avgVel; // center and avgVel of the flock
        int neighborhoodCount;

        // constructor
        public VelocityMatch(Entity _character, Flock _flock, float _maxDistance, float _minDotProduct)
        : base(_character, _flock, _maxDistance, _minDotProduct)
        {
            character = _character;
            flock = _flock;
            

            maxNeighborhoodSize = 10;
        }
        
        public override SteeringOutput GetSteering()
        {
            var steering = new SteeringOutput();

            if( !useOldValues )
                neighborhoodCount = flock.GetNeighborhoodInfo(character, maxNeighborhoodSize, neighborhoodMaxDistance * neighborhoodMaxDistance, neighborhoodMinDotProduct, aknnApproxVal, out center, out avgVel);

            // if there are no neighbors, return early
            if (neighborhoodCount == 0)
                return SteeringOutput.None;

            steering.linearVel = avgVel - character.velocity;
            if( steering.linearVel.magnitude > character.maxSpeed )
            {
                steering.linearVel.Normalize();
                steering.linearVel *= character.maxSpeed;
            }

            return steering;
        }
    }
}

