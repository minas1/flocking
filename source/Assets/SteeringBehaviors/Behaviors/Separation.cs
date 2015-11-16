using System;
using System.Collections.Generic;
using UnityEngine;

using Flocking;

namespace SteeringBehaviors
{
    public class Separation : BoidSteering
    {
        Flee flee;
        DummyEntity dummy;
        
        public bool useOldValues = false;
        Vector3 center, avgVel; // center and avgVel of the flock
        int neighborhoodCount;

        public Separation(Entity _character, Flock _flock, float _neighbourhoodMaxDistance, float _neighbourhoodMinDotProduct)
        : base(_character, _flock, _neighbourhoodMaxDistance, _neighbourhoodMinDotProduct)
        {
            dummy = new DummyEntity();
            flee = new Flee(_character, dummy);

            maxNeighborhoodSize = 5;
        }
        
        public override SteeringOutput GetSteering()
        {
            if( !useOldValues )
                neighborhoodCount = flock.GetNeighborhoodInfo(character, maxNeighborhoodSize, neighborhoodMaxDistance * neighborhoodMaxDistance, neighborhoodMinDotProduct, aknnApproxVal, out center, out avgVel);

            if (neighborhoodCount == 0)
                return SteeringOutput.None;

            // steer away from the center of the local neighborhood
            dummy.position = center;
            
            return flee.GetSteering();
        }
    }
}

