using System.Collections.Generic;
using System;
using UnityEngine;
using SteeringBehaviors;

using System.Threading;

public class StarlingWander : SingleBirdState
{
	string[] enemyTags;

    const float MIN_TIME_CHANGE_DIRECTION = 5f;
    const float MAX_TIME_CHANGE_DIRECTION = 30f;

    float t;

    const float LOOKAHEAD_DISTANCE = 20f;

	public StarlingWander(Bird bird, string[] _enemyTags, Bounds bbox) : base(bird)
	{
        var blended = new BlendedSteering[2];
        blended[0] = new BlendedSteering(bird, new BehaviorAndWeight(new ObstacleAvoidance (bird, LOOKAHEAD_DISTANCE, new string[]{"Ground"}), 1f));
        blended[1] = new BlendedSteering(bird, new BehaviorAndWeight(new SteeringBehaviors.Wander(bird, bird.maxSpeed, 5f, 5f, bbox), 1f));

        behavior = new PrioritySteering(1f, blended);

		enemyTags = _enemyTags;

        t = UnityEngine.Random.Range(MIN_TIME_CHANGE_DIRECTION, MAX_TIME_CHANGE_DIRECTION);
	}

    public Bounds BoundingBox
    {
        get
        {
            return wanderSteering.bbox;
        }
        set
        {
            wanderSteering.bbox = value;
        }
    }

    public Wander wanderSteering
    {
        get
        {
            return (behavior as PrioritySteering).Groups[1].Behaviors[0].behaviour as Wander;
        }
    }

    public ObstacleAvoidance obstacleAvoidanceSteering
    {
        get
        {
            return (behavior as PrioritySteering).Groups[0].Behaviors[0].behaviour as ObstacleAvoidance;
        }
    }

    public override void Update(float dt, Bird bird)
	{
        var prioritySteering = (PrioritySteering)behavior;
        var avoidance = prioritySteering.Groups[0].Behaviors[0].behaviour as ObstacleAvoidance;
        var wander = prioritySteering.Groups[1].Behaviors[0].behaviour as Wander;

		UpdateSteering(dt);

        // make wander behavior's angle same as the target position of the avoidance behavior,
        // so that when wander will be used it will have the correct (current) angle of the bird
        // and not its old one
        if( prioritySteering.lastUsedSteering == 0 ) // obstacle avoidance
        {
            wander.LookAt(avoidance.targetPosition);
            wander.customYSpeed = 0f;
        }
	}

	public override void FixedUpdate()
	{
	}
	
	public override void onCollisionEnter(Collision other)
	{
	}
	
	public override void onCollisionStay(Collision other)
	{
	}
	
	public override void onCollisionExit(Collision other)
	{
	}
}