using System;
using UnityEngine;

using Flocking;

namespace SteeringBehaviors
{
    public class PathFollow : Steering
    {
        public Path path;
        int param = -1;
        
        Seek seek;
        
        public Entity character;
        
        DummyEntity dummy; // dummy entity we use to get a transform for Seek's target
        
        public PathFollow(Entity _character, Path _path)
        {
            character = _character;
            path = _path;
            
            dummy = new DummyEntity();
            seek = new Seek(character, dummy);
        }
    
        public SteeringOutput GetSteering()
        {
            param = path.GetParam(character.transform.position, param, 5f);
        
            dummy.position = path.GetPosition(param);
            
            return seek.GetSteering();
        }

        public void Destroy()
        {
            dummy.Destroy();
        }
        
        /// <summary>
        /// Returns the position in the path. Return -1 when no point has been reached yet
        /// </summary>
        public int Index()
        {
            return param;
        }

    }
}

