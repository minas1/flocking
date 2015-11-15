using System.Collections.Generic;
using UnityEngine;

using Flocking;

namespace SteeringBehaviors
{
	public class Cohesion : BoidSteering
	{
		Seek seek;
		DummyEntity dummy;

        public bool useOldValues = false;
        Vector3 center, avgVel; // center and avgVel of the flock
        int neighborhoodCount;

		public Cohesion(Entity _character, Flock _flock, float _neighbourhoodMaxDistance, float _neighbourhoodMinDotProduct)
            : base(_character, _flock, _neighbourhoodMaxDistance, _neighbourhoodMinDotProduct)
		{
			dummy = new DummyEntity();
			seek = new Seek(character, dummy);

            maxNeighborhoodSize = 30;
		}
		
		public override SteeringOutput GetSteering()
		{
            if( !useOldValues )
                neighborhoodCount = flock.GetNeighborhoodInfo(character, maxNeighborhoodSize, neighborhoodMaxDistance * neighborhoodMaxDistance, neighborhoodMinDotProduct, aknnApproxVal, out center, out avgVel);

            if( neighborhoodCount == 0 )
                return SteeringOutput.None;

			dummy.position = center;
			
			return seek.GetSteering();
		}
	}

}

