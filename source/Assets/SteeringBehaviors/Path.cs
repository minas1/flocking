using System;
using System.Collections.Generic;

using UnityEngine;

namespace Flocking
{
    public class Path
    {
        public Entity character;
        public List<Vector3> path; // list of world coordinates that define the path
        
        private bool pathCompleted;
        private bool loop = true; // loop through the path?
        
        public Path()
        {
            path = new List<Vector3>();
        }

        public void Add(Vector3 pos)
        {
            path.Add(pos);
        }
        
        /// <summary>
        /// This function returns the next index of the next point on the path
        /// </summary>
        /// <param name='position'>
        /// Position of the character.
        /// </param>
        /// <param name='param'>
        /// The current index. If it's the first time it's called, it must be equal to -1
        /// </param>
        /// <param name='minDistance'>
        /// The distance at which we consider we have reached the target. If the character moves at a very high velocity,
        /// increase this value.
        /// </param>
        public int GetParam(Vector3 position, int param, float minDistance)
        {
            if( param == -1 )
            {
                // find the closest point of the path to the player
                float min = float.MaxValue;
                for(int i = 0; i < path.Count; ++i)
                {
                    if( (position - path[i]).magnitude < min )
                    {
                        param = i;
                        min = (position - path[i]).magnitude;
                    }
                }
            }
            else
            {
                // if we have reached the point, go for the next one
                if( (position - path[param]).magnitude < minDistance )
                {
                    if( loop )
                        param = (param + 1) % path.Count;
                    else
                        param = Mathf.Clamp(param + 1, 0, path.Count - 1);
                }
            }
            
            return param;
        }
        
        public void SetLoop(bool val)
        {
            loop = val;
        }
        
        public Vector3 GetPosition(int param)
        {
            return path[param];
        }
        
        /// <summary>
        /// Returns true when the path is completed. Applies only to path that do not loop
        /// </summary>
        public bool PathCompleted(int param)
        {
            return !loop && param == path.Count - 1;
        }
    }

}