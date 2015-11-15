using System;
using System.Collections.Generic;
using UnityEngine;
//using Leap;

public static class Util
{
    public static void Swap<T>(ref T a, ref T b)
    {
        var temp = a;
        a = b;
        b = temp;
    }

	/// <summary>
	/// Normalizes the angle.
	/// </summary>
	/// <returns>
	/// The angle in [-180,180]
	/// </returns>
	/// <param name='angle'>
	/// Angle from [0,360]
	/// </param>
	public static float NormalizeAngle(float angle)
	{
		if( angle >= 180f )
			return angle - 360f;
		
		return angle;
	}
	
	public static bool isInRange(float val, float min, float max)
	{
		return val >= min && val <= max;
	}
	
	/// <summary>
	/// Returns a random number (-1, 1) with higher chance to be close to 0
	/// </summary>
	public static float RandomBinomial()
	{
		return UnityEngine.Random.Range(0f, 1f) - UnityEngine.Random.Range(0f, 1f);
	}
	
	/// <summary>
	/// Returns true if the character at p0 is moving towards p1 with the given velocity
	/// </summary>
	public static bool MovesTowards(Vector3 p0, Vector3 p1, Vector3 velocity)
	{
		float d0 = (p1 - p0).magnitude;
		
		p0 += velocity;
		
		float d1 = (p1 - p0).magnitude;
		
		return d1 < d0;
	}
	
	/// <summary>
	/// Keeps in the "colliders" list only the colliders that have tag equal to one of the tags in "tags" list
	/// </summary>
	public static void filterSelectedTags(List<Collider> colliders, string[] tags)
	{
		bool changes = false;
		do
		{
			changes = false;
			foreach(var collider in colliders)
			{
				if( Array.Find(tags, str => str == collider.tag) == null )
				{
					colliders.Remove(collider);
					changes = true;
					break;
				}
			}
			
		} while(changes == true);
	}

    public static void GetCollidersInRange(List<Collider> colliders, string tag, Vector3 p, float distance)
    {
        var gameObjects = GameObject.FindGameObjectsWithTag(tag);

        foreach (var g in gameObjects)
        {
            if( Vector3.Distance(g.transform.position, p) <= distance )
            {
                colliders.Add(g.GetComponent<Collider>());
            }
        }
    }

    public static int BinarySearchClosestEntity(List<Entity> list, Entity e)
    {
        if( list.Count == 0 )
            return -1;
        else if( list.Count == 1 )
            return 0;

        int left, right, mid;
        int closestIndex;
        
        left = 0;
        right = list.Count;
        closestIndex = (left + right) / 2;
        
        while( left <= right )
        {
            mid = (left + right) / 2;
         
            // if the value was found, stop here
            if( list[mid] == e )
            {
                closestIndex = mid;
                break;
            }
            
            if( Mathf.Abs(list[mid].position.x - e.position.x) < Mathf.Abs(list[closestIndex].position.x - e.position.x) )
                closestIndex = mid;
            
            var valueLeft = (mid - 1 >= 0 ? list[mid - 1].position.x : float.MaxValue);
            var valueRight = (mid + 1 < list.Count ? list[mid + 1].position.x : float.MaxValue);
            
            if( Math.Abs(valueLeft - e.position.x) < Math.Abs(valueRight - e.position.x) )
            {
                // go left
                right = mid - 1;
            }
            else
            {
                // go right
                left = mid + 1;
            }
        }
        
        return closestIndex;
    }

    /// <summary>
    /// Returns the squared distance of two vectors
    /// </summary>
    public static float DistanceSquared(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
    }


    public static List<Vector3> LoadModel(string filename)
    {
        var points = new List<Vector3>();
        
        string line;
        var file = new System.IO.StreamReader(filename);
        while( (line = file.ReadLine()) != null )
        {
            var strArray = line.Split(' ');
            var v = new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
            
            points.Add(v);
        }
        
        file.Close();

        return points;
    }

    /// <summary>
    /// Simplifies the points by merging points whose distance is <= mergeDistance
    /// </summary>
    public static void SimplifyPoints(List<Vector3> vertices, float mergeDistance)
    {
        var point = new double[3];
        var points = new double[vertices.Count, 3];
        var results = new double[100, 3];

        bool stop = false;
        
        do
        {
            alglib.kdtree kdt;
            for(int i = 0; i < vertices.Count; ++i)
            {
                points[i, 0] = vertices[i].x;
                points[i, 1] = vertices[i].y;
                points[i, 2] = vertices[i].z;
            }
            
            // build the k-d tree
            alglib.kdtreebuild(points, vertices.Count, 3, 0, 2, out kdt);
            
            bool merged = false;
            for(int i = 0; i < vertices.Count; ++i)
            {
                point[0] = vertices[i].x;
                point[1] = vertices[i].y;
                point[2] = vertices[i].z;
                
                int neighbors = alglib.kdtreequeryrnn(kdt, point, mergeDistance, true); // last parameter is true, we want self match
                
                if( neighbors > 1 )
                    merged = true;

                // get the results
                alglib.kdtreequeryresultsx(kdt, ref results);
                
                // find the median point
                var sum = new Vector3();
                for(int j = 0; j < neighbors; ++j)
                {
                    var v = new Vector3((float)results[j, 0], (float)results[j, 1], (float)results[j, 2]);
                    sum += v;
                    
                    vertices.Remove(v);
                }
                
                vertices.Insert(0, sum / neighbors);
            }
            
            stop = !merged;
            
        } while( !stop );

        
    }

    public static void NormalizeAndMultiplyPoints(List<Vector3> points)
    {
        float maxLength = float.MinValue;

        var sum = Vector3.zero;

        foreach(var p in points)
        {
            sum += p;
            if( p.magnitude > maxLength )
                maxLength = p.magnitude;
        }
    }
}
