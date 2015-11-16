using System.Collections.Generic;
using UnityEngine;

using Flocking;

namespace SteeringBehaviors
{
    public abstract class BoidSteering : Steering
    {
        public Entity character;
        public Flock flock; // the flock this boid belongs to
        
        public float neighborhoodMaxDistance;
        public float neighborhoodMinDotProduct;

        public int maxNeighborhoodSize; // max numbers of neighbors to consider

        public double aknnApproxVal = 0f; // approximation value used for k-nearest neighbors algorithm (default is no approximation)

        public BoidSteering(Entity _character, Flock _flock, float _neighbourhoodMaxDistance, float _neighbourhoodMinDotProduct)
        {
            character = _character;
            flock = _flock;
            neighborhoodMaxDistance = _neighbourhoodMaxDistance;
            neighborhoodMinDotProduct = _neighbourhoodMinDotProduct;
        }
        
        public abstract SteeringOutput GetSteering();
    }
}