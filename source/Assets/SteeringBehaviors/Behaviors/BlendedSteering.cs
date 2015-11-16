using System.Collections.Generic;
using UnityEngine;

using Flocking;

namespace SteeringBehaviors
{
    public class BlendedSteering : Steering
    {
        List<BehaviorAndWeight> behaviors; // the list of behaviours
        List<float> lastSpeed; // list of the speeds of the behaviors multiplied by their weight. this value is updated in GetSteering()

        Entity character;
        
        public BlendedSteering(Entity _character, params BehaviorAndWeight[] _behaviours)
        {
            behaviors = new List<BehaviorAndWeight>();
            lastSpeed = new List<float>();

            Add(_behaviours);
            character = _character;
        }
        
        public SteeringOutput GetSteering()
        {
            var steering = new SteeringOutput();
            
            int i = 0;
            foreach(var behavior in behaviors)
            {
                var tempSteering = behavior.behaviour.GetSteering();

                steering.linearVel += behavior.weight * tempSteering.linearVel;

                lastSpeed[i] = behavior.weight * tempSteering.linearVel.magnitude;

                ++i;
            }

            // crop the result and return
            steering.linearVel = Vector3.ClampMagnitude(steering.linearVel, character.maxSpeed);
            
            return steering;
        }

        public void Add(params BehaviorAndWeight[] _behaviours)
        {
            foreach(var behaviour in _behaviours)
            {
                behaviors.Add(behaviour);
                lastSpeed.Add(0f);
            }
        }

        public List<BehaviorAndWeight> Behaviors
        {
            get { return behaviors; }
        }

        public List<float> LastSpeed
        {
            get { return lastSpeed; }
        }
    }
}

