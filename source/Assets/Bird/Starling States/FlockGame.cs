using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;
using Flocking;

using System.Threading;

public class FlockGame : MultipleBirdState
{
    int currentFrame = 0;

    public Entity anchor;

    public float percentageOfBoidsToUpdate = 0.2f; // percentage of boids to update every frame

    System.Diagnostics.Stopwatch kdBuildTimer, updateTimer;
    int frameCount = 0;

    int current;

    Flock anchorFlock;

    enum BoidSteeringType
    {
        Free,
        FollowingAnchor
    }

    BoidSteeringType[] boidSteeringType;

    int _score;
    float t = 60f;

    public FlockGame()
    {
        kdBuildTimer = new System.Diagnostics.Stopwatch();
        updateTimer = new System.Diagnostics.Stopwatch();

        anchorFlock = new Flock();
    }

    /// <summary>
    /// Re-calculates the path for each bird
    /// </summary>
    public override void Init()
    {
        anchorFlock.Add(anchor);
        boidSteeringType = new BoidSteeringType[entries.Count];

        for(int i = 0; i < entries.Count; ++i)
        {
            entries[i].behavior = getDefaultSteering(entries[i]);
            boidSteeringType[i] = BoidSteeringType.Free;
        }
    }

    public override void Update(float dt)
    {
        if (Time.frameCount == currentFrame) return;
        currentFrame = Time.frameCount;

        //------------
        kdBuildTimer.Start();

        flock.RebuildKdTree();
        anchorFlock.RebuildKdTree();

        kdBuildTimer.Stop();
        //------------

        updateTimer.Start();

        // reset values
        for(int i = 0; i < entries.Count; ++i)
        {
            if( boidSteeringType[i] == 0 )
            {
                (((entries[i].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[0].behaviour as Separation).useOldValues = true;
                (((entries[i].behavior as PrioritySteering).Groups[2] as BlendedSteering).Behaviors[1].behaviour as Cohesion).useOldValues = true;
                (((entries[i].behavior as PrioritySteering).Groups[2] as BlendedSteering).Behaviors[2].behaviour as VelocityMatch).useOldValues = true;
            }
        }


        int boidsToUpdate = Mathf.CeilToInt(entries.Count * percentageOfBoidsToUpdate); // make sure at least one boid is updated
        int limit = (current + boidsToUpdate < entries.Count ? current + boidsToUpdate : entries.Count);
        for(; current < limit; ++current)
        {
            if( boidSteeringType[current] == 0 )
            {
                (((entries[current].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[0].behaviour as Separation).useOldValues = false;
                (((entries[current].behavior as PrioritySteering).Groups[2] as BlendedSteering).Behaviors[1].behaviour as Cohesion).useOldValues = false;
                (((entries[current].behavior as PrioritySteering).Groups[2] as BlendedSteering).Behaviors[2].behaviour as VelocityMatch).useOldValues = false;
            }
        }

        if( current == entries.Count )
            current = 0;

        // update birds and add close boids to the player's flock
        for (int i = 0; i < entries.Count; ++i)
        {
            UpdateSteering(dt, entries[i]);

            var distance = Vector3.Distance(entries[i].bird.position, anchor.position);

            if( distance <= 40f && boidSteeringType[i] == BoidSteeringType.Free )
            {
                entries[i].behavior = getDefaultSteering2(entries[i]);
                boidSteeringType[i] = BoidSteeringType.FollowingAnchor;
            }
        }

        t -= dt;
        if( t < 0f )
            t = 0f;

        // count how many boids are following the player
        if( t > 0 )
        {
            _score = 0;
            for(int i = 0; i < boidSteeringType.Length; ++i)
                if( boidSteeringType[i] == BoidSteeringType.FollowingAnchor )
                    ++_score;
        }


        updateTimer.Stop();
        ++frameCount;
        //Debug.Log("Time to build K-D tree: " + kdBuildTimer.ElapsedMilliseconds / (float)frameCount + ", Time to update: " + updateTimer.ElapsedMilliseconds / (float)frameCount);
    }

    public int Score
    {
        get
        {
            return _score;
        }
    }

    public float TimePassed
    {
        get
        {
            return t;
        }
    }

    Steering getDefaultSteering(Entry e)
    {
        e.bird.maxSpeed = 7.5f;

        (e.bird as Starling).transform.Find("Sphere").gameObject.SetActive(true);

        var obstacleAvoidance = new ObstacleAvoidance(e.bird, 20f, new string[]{"Ground"});
        var separation = new Separation(e.bird, flock, 5f, 0f);
        var cohesion = new Cohesion(e.bird, flock, 75f, 0f); cohesion.aknnApproxVal = 25f; cohesion.maxNeighborhoodSize = 15;
        var velMatch = new VelocityMatch(e.bird, flock, 20f, 0.0f); velMatch.aknnApproxVal = 50f;


        var bbox = new Bounds(new Vector3(1162, 113, 770), new Vector3(500, 150, 500));
        var wander = new SteeringBehaviors.Wander(e.bird, e.bird.maxSpeed, 30f, 50f, bbox);
        wander.theta = UnityEngine.Random.Range(0f, 360f);

        var blended = new BlendedSteering[3];

        blended[0] = new BlendedSteering(e.bird, new BehaviorAndWeight(obstacleAvoidance, 1f));
        blended[1] = new BlendedSteering(e.bird,
                                         new BehaviorAndWeight(separation, 1f)
                                         );
        blended[2] = new BlendedSteering(e.bird, new BehaviorAndWeight(wander, 0.85f), // 0
                                                 new BehaviorAndWeight(cohesion, 0.90f),
                                                 new BehaviorAndWeight(velMatch, 0.50f)
                                         );

        return new PrioritySteering(1f, blended);
    }

    Steering getDefaultSteering2(Entry e)
    {
        e.bird.maxSpeed = 30f;

        (e.bird as Starling).transform.Find("Sphere").gameObject.SetActive(false);

        var separation = new Separation(e.bird, flock, 5f, -0.5f);
        var cohesion = new Cohesion(e.bird, flock, 30f, -0.5f);
        var velMatch = new VelocityMatch(e.bird, flock, 20f, -0.5f);

        var seek = new Cohesion(e.bird, anchorFlock, float.MaxValue, -1f); // move towards the anchor
        var flee = new Separation(e.bird, anchorFlock, 10f, -1f); // move towards the anchor

        separation.aknnApproxVal = 1.0;
        cohesion.aknnApproxVal = 1.0;
        velMatch.aknnApproxVal = 25.0;
        
        separation.maxNeighborhoodSize = 5;
        cohesion.maxNeighborhoodSize = 10;
        velMatch.maxNeighborhoodSize = 5;
        
        
        var obstacleAvoidance = new ObstacleAvoidance(e.bird, 20f, new string[]{"Ground"});
        
        var blended = new BlendedSteering[2];
        
        blended[0] = new BlendedSteering(e.bird, new BehaviorAndWeight(obstacleAvoidance, 1f));
        blended[1] = new BlendedSteering(e.bird, new BehaviorAndWeight(separation, 2f), // 0
                                         new BehaviorAndWeight(cohesion, 1f), // 1
                                         new BehaviorAndWeight(velMatch, 0.5f), // 2
                                         new BehaviorAndWeight(seek, 1.25f), // 3
                                         new BehaviorAndWeight(flee, 2f)
                                         );
        
        return new PrioritySteering(1f, blended);
    }

    public override void Update(float dt, Bird bird)
    {
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

    public override void GetFlockInfo(out Vector3 center, out Vector3 avgVel)
    {
        Flock.GetNeighborhoodInfo(flock.Boids, out center, out avgVel);
    }

    public float boidMaxSpeed
    {
        set
        {
            foreach(var entry in entries)
                entry.bird.maxSpeed = value;
        }
        get
        {
            if( entries.Count == 0 )
                return float.NaN;

            return entries[0].bird.maxSpeed;
        }
    }

}
