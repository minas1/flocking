using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
	public class Flock
	{
		List<Entity> boids;
        alglib.kdtree kdt;

        double[] point;
        double[,] results, points;

        List<Entity> boidsX, boidsY, boidsZ; // boids sorted by x, y, z coordinate

		public Flock(params Entity[] _boids)
		{
			boids = new List<Entity>();
            point = new double[3];

            results = new double[0, 0];
            points = new double[100, 6]; // set an arbitrary size. will be changed if it's not enough

            boidsX = new List<Entity>();
            boidsY = new List<Entity>();
            boidsZ = new List<Entity>();

			Add(_boids);  
		}
		
		public List<Entity> Boids
		{
			get { return boids; }
		}

        public void RebuildKdTree()
        {
            for (int i = 0; i < boids.Count; ++i)
            {
                var pos = boids[i].position;
                var vel = boids[i].velocity;

                points[i, 0] = pos.x;
                points[i, 1] = pos.y;
                points[i, 2] = pos.z;
                
                points[i, 3] = vel.x;
                points[i, 4] = vel.y;
                points[i, 5] = vel.z;
            }

            // build the k-d tree
            alglib.kdtreebuild(points, boids.Count, 3, 3, 2, out kdt);
        }

        /// <summary>
        /// Gets the K nearest neighbors of entity. If only M < K neighbors exist, returns M neighbors.
        /// </summary>
        public List<Entity> KNearestNeighbors(Entity entity, int k, double aknnApproxVal)
        {
            var neighbors = new List<Entity>();

            var entityPositions = new Dictionary<Vector3, Entity>();
            foreach (var e in boids)
                entityPositions.Add(e.position, e);

            point[0] = entity.position.x;
            point[1] = entity.position.y;
            point[2] = entity.position.z;

            var results = new double[0, 0]; // the array containing the results (points of the k nearest neighbors)

            // k+1 because we will ignore ourself
            int neighborCount = alglib.kdtreequeryaknn(kdt, point, k+1, aknnApproxVal);

            // get the results
            alglib.kdtreequeryresultsxy(kdt, ref results);

            for (int i = 0; i < neighborCount; ++i)
            {
                var pos = new Vector3((float)results[i, 0], (float)results[i, 1], (float)results[i, 2]);
                var vel = new Vector3((float)results[i, 3], (float)results[i, 4], (float)results[i, 5]);

                if (pos == entity.position)
                    continue;

                neighbors.Add(entityPositions[pos]);
            }

            return neighbors;
        }

        /// <summary>
        /// Gets K boids that are around the given point
        /// </summary>
        public List<Entity> KBoidsAroundPoint(Vector3 p, int k, double aknnApproxVal)
        {
            var neighbors = new List<Entity>();
            
            var entityPositions = new Dictionary<Vector3, Entity>();
            foreach (var e in boids)
                entityPositions.Add(e.position, e);
            
            var point = new double[3]{p.x, p.y, p.z};
            
            var results = new double[0, 0]; // the array containing the results (points of the k nearest neighbors)

            int neighborCount = alglib.kdtreequeryaknn(kdt, point, k, aknnApproxVal);
            
            // get the results
            alglib.kdtreequeryresultsxy(kdt, ref results);
            
            for (int i = 0; i < neighborCount; ++i)
            {
                var pos = new Vector3((float)results[i, 0], (float)results[i, 1], (float)results[i, 2]);
                var vel = new Vector3((float)results[i, 3], (float)results[i, 4], (float)results[i, 5]);
                
                neighbors.Add(entityPositions[pos]);
            }
            
            return neighbors;
        }

        public int GetNeighborhoodInfo(Entity character, int maxNeighbors, float maxDistanceSquared, float minDotProduct, double aknnApproxVal, out Vector3 center, out Vector3 avgVeL) 
        {
            point[0] = character.position.x;
            point[1] = character.position.y;
            point[2] = character.position.z;

            center = avgVeL = Vector3.zero;

            if( maxNeighbors == 0 )
                return 0;

            int neighbors = alglib.kdtreequeryaknn(kdt, point, maxNeighbors, false, aknnApproxVal);

            // get the results
            alglib.kdtreequeryresultsxy(kdt, ref results);
            
            // where is the character looking
            var look = character.transform.forward;

            int count = 0;
            Vector3 pos;
            for (int i = 0; i < neighbors; ++i)
            {
                pos.x = (float)results[i, 0];
                pos.y = (float)results[i, 1];
                pos.z = (float)results[i, 2];

                if( Util.DistanceSquared(character.position, pos) <= maxDistanceSquared )
                {
                    // check for angle
                    if( minDotProduct > -1.0f )
                    {
                        var offset = pos - character.position;
                        if( Vector3.Dot(look, offset.normalized) < minDotProduct )
                            continue;
                    }
                    
                    center += pos;

                    avgVeL.x += (float)results[i, 3];
                    avgVeL.y += (float)results[i, 4];
                    avgVeL.z += (float)results[i, 5];

                    ++count;
                }
            }

            center /= count;
            avgVeL /= count;

    
            return count;
        }

        public int GetNeighborhoodInfo2(Entity character, int maxNeighbors, float maxDistance, float minDotProduct, double aknnApproxVal, out Vector3 center, out Vector3 avgVel) 
        {
            center = avgVel = Vector3.zero;
            
            if( maxNeighbors == 0 )
                return 0;

            int count = 0;

            int indexX = Util.BinarySearchClosestEntity(boidsX, character);

            int i = (boidsX[indexX] == character ? indexX - 1 : indexX);
            int j = indexX + 1;

            int neighbors = 0;
            while (neighbors < maxNeighbors)
            {
                if (i >= 0 && j < boidsX.Count)
                {
                    if (Vector3.Distance(boidsX[i].position, character.position) > maxDistance && Vector3.Distance(boidsX[j].position, character.position) > maxDistance)
                        break;

                    if (Vector3.Distance(boidsX[i].position, character.position) < Vector3.Distance(boidsX[j].position, character.position))
                        --i;
                    else
                        ++j;
                }
                else if (i >= 0)
                {
                    if (Vector3.Distance(boidsX[i].position, character.position) > maxDistance)
                        break;

                    --i;
                }
                else if (j < boidsX.Count)
                {
                    if (Vector3.Distance(boidsX[j].position, character.position) > maxDistance)
                        break;

                    ++j;
                }
                else
                    break;

                ++neighbors;
            }
            for (i = 0; i < neighbors; ++i)
            {
                if (Vector3.Distance(boidsX[i].position, character.position) <= maxDistance)
                {
                    center += boidsX[i].position;
                    avgVel += boidsX[i].velocity;
                    ++count;
                }
            }
            // where is the character looking
            var look = character.transform.forward;

            center /= count;
            avgVel /= count;

            return count;
        }

        public static int GetNeighborhoodInfo(List<Entity> boids, out Vector3 center, out Vector3 avgVel)
        {
            center = Vector3.zero;
            avgVel = Vector3.zero;
            
            foreach (var boid in boids)
            {
                center += boid.position;
                avgVel += boid.velocity;
            }

            center /= boids.Count;
            avgVel /= boids.Count;
            
            return boids.Count;
        }

		/// <summary>
		/// Gets the neighbourhood center.
		/// </summary>
		public Vector3 GetNeighbourhoodCenter(List<Entity> neighbourhood)
		{
			var center = Vector3.zero;
			if( neighbourhood.Count == 0 )
				return center;
			
			foreach(var boid in neighbourhood)
				center += boid.transform.position;
			
			return center / neighbourhood.Count;
		}
		
		public Vector3 GetNeighbourhoodAverageVelocity(List<Entity> neighbourhood)
		{
			var vel = Vector3.zero;
			if( neighbourhood.Count == 0 )
				return vel;
			
			foreach (var boid in neighbourhood)
				vel += boid.velocity;
			
			return vel / neighbourhood.Count;
		}
		
		/// <summary>
		/// Adds a variable number of boids into the flock.
		/// </summary>
		public void Add(params Entity[] _boids)
		{
            if (boids.Count + _boids.Length >= points.Length / 6)
                points = new double[(boids.Count + _boids.Length) * 2, 6];

			foreach (var boid in _boids)
            {
                boids.Add(boid);

                boidsX.Add(boid);
                boidsY.Add(boid);
                boidsZ.Add(boid);
            }
		}
		
		/// <summary>
		/// Removes a variable number of boids from the flock.
		/// </summary>
		public void Remove(params Entity[] _boids)
		{
			foreach(var boid in _boids)
                boids.Remove(boid);
		}
		
		/// <summary>
		/// Removes all boids from the flock.
		/// </summary>
		public void RemoveAll()
		{
			boids.Clear();
		}

        public void Merge(params Flock[] flocks)
        {
            foreach (var f in flocks)
            {
                foreach (var boid in f.boids)
                {
                    if( !boids.Exists(x => x == boid) )
                        boids.Add(boid);
                }
            }
        }

		/// <summary>
		/// Merges flocks and returns a new one
		/// </summary>
		public static Flock MergeFlocks(params Flock[] flocks)
		{
			Flock newFlock = new Flock();

            foreach (var f in flocks)
            {
                foreach (var boid in f.boids)
                {
                    if( !newFlock.boids.Exists(x => x == boid) )
                        newFlock.Add(boid);
                }
            }

			return newFlock;
		}


	}
}

