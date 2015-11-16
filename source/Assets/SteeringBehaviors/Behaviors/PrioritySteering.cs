using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviors
{
    public class PrioritySteering : Steering
    {
        // holds a list of BlendedSteering instances, which in turn contain sets of behaviours 
        List<BlendedSteering> groups;
        
        // holds the epsilon value, which should be small
        public float epsilon;

        int lastUsedSteeringIndex; // index of the last behavior that was used

        public PrioritySteering(float _epsilon, params BlendedSteering[] _groups)
        {
            groups = new List<BlendedSteering>();
            Add(_groups);
            
            epsilon = _epsilon;
        }
        
        public SteeringOutput GetSteering()
        {
            SteeringOutput steering = new SteeringOutput();
            
            lastUsedSteeringIndex = 0;
            foreach(var group in groups)
            {
                steering = group.GetSteering();
                
                // check if we are above the epsilon threshold, if so return
                if( steering.linearVel.magnitude > epsilon )
                {
                    return steering;
                }

                ++lastUsedSteeringIndex;
            }

            // if we reached here it means that no group had a large enough acceleration,
            // so return the (small) acceleration of the last group
            return steering;
        }

        /// <summary>
        /// Returns the last used steering.
        /// </summary>
        public int lastUsedSteering
        {
            get { return lastUsedSteeringIndex; }
        }

        public void Add(params BlendedSteering[] _groups)
        {
            foreach(var group in _groups)
                groups.Add(group);
        }
        
        public void Remove(params BlendedSteering[] _groups)
        {
            foreach(var group in _groups)
                groups.Remove(group);
        }
        
        public void RemoveAll()
        {
            groups.Clear();
        }

        public List<BlendedSteering> Groups
        {
            get { return groups; }
        }
    }
}

