using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public abstract class SingleBirdState : BirdState
{
	public Steering behavior; // the behaviour of the bird
	protected Bird bird;
	
    public bool canSwitchToMultipleState = true; // if true this bird will join flocks. you might want to disable if you want a bird to be an anchor of a flock

    public SingleBirdState(Bird _bird)
	{
		bird = _bird;
	}
	
	public abstract void Update(float dt, Bird bird);
	
	public abstract void FixedUpdate();

	/// <summary>
	/// Updates the velocity/rotation of the Bird using the behaviour it has
	/// </summary>
	protected void UpdateSteering(float dt)
	{
        UpdateSteering(behavior, bird, dt);
	}

    public static void UpdateSteering(Steering behavior, Bird bird, float dt)
    {
        if (behavior == null)
            return;
        
        var steering = behavior.GetSteering();

        if (!steering.IsNoSteering())
        {
            // update position
            bird.velocity += steering.linearVel * dt;

            bird.velocity = Vector3.ClampMagnitude(bird.velocity, bird.maxSpeed);

            if (bird.maxSpeed != 0f)
            {
                var rotation = Quaternion.LookRotation(bird.velocity);
                bird.transform.rotation = Quaternion.Slerp(bird.transform.rotation, rotation, 1f);
            }
        }
        else
            bird.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
	
	protected bool hasVisionOf(Entity entity, String targetTag, string[] obstacleTags, List<Collider> collidersInViewVolume)
	{
		bool obstaclesExist = collidersInViewVolume.Exists(x => Array.Exists(obstacleTags, y => x.tag == y));

		// if there are obstacles in the view volume, check if one of them blocks vision of the target
		if (obstaclesExist)
		{
			var targetColliders = collidersInViewVolume.FindAll(x => x.tag == targetTag);
			foreach (var targetCollider in targetColliders)
			{
				var gameObject = targetCollider.gameObject;
				var layerMask = (1 << 8); // bit shift by the index of the layer we want (NoCollision layer)
				layerMask = ~layerMask; // this will force to test only for collisions other than the above
				
				RaycastHit hitInfo;
				bool hit = Physics.Raycast(entity.transform.position, gameObject.transform.position - entity.transform.position, out hitInfo, Vector3.Distance(entity.transform.position, gameObject.transform.position) * 1.5f, layerMask);

				if (hit && hitInfo.collider.gameObject.tag == targetTag)
					return true;
			}
		}
        // if there are no obstacles, if there is a collider with the target tag in the view volume, return true
        else if (collidersInViewVolume.Exists(x => x.tag == targetTag))
            return true;
		
		return false;
	}

	public static List<Collider> getVisibleGameObjects(Entity entity, string[] obstacleTags, List<Collider> collidersInViewVolume)
	{
        var stopwatch = new Stopwatch();
        stopwatch.Start();

		var visibleEntities = new List<Collider>();

        stopwatch.Stop();
        stopwatch.Reset();
        stopwatch.Start();

		bool obstaclesExist = collidersInViewVolume.Exists(x => Array.Exists(obstacleTags, y => x.tag == y));

        stopwatch.Stop();

		// if there are obstacles in the view volume, check if one of them blocks vision of the target
		if (obstaclesExist)
		{
			var colliders = collidersInViewVolume.FindAll(x => !Array.Exists(obstacleTags, y => y == x.tag));
			foreach (var collider in colliders)
			{
				var gameObject = collider.gameObject;
				var layerMask = (1 << 8); // bit shift by the index of the layer we want (NoCollision layer)
				layerMask = ~layerMask; // this will force to test only for collisions other than the above

				RaycastHit hitInfo;
				bool hit = Physics.Raycast(entity.transform.position, gameObject.transform.position - entity.transform.position, out hitInfo, Vector3.Distance(entity.transform.position, gameObject.transform.position), layerMask);

				if (hit && !Array.Exists(obstacleTags, x => x == hitInfo.collider.gameObject.tag))
                    visibleEntities.Add(collider);
			}
		}
		// if there are no obstacles
		else
		{
			foreach (var collider in collidersInViewVolume)
                visibleEntities.Add(collider);
		}
        	
		return visibleEntities;
	}

    public bool IsSingleBirdState
    {
        get { return true; }
    }
}


