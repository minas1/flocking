using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;
using Flocking;

using System.Threading;

public class FlockWander : MultipleBirdState
{
    int currentFrame = 0;

    public Entity anchor;
    Flock anchorFlock;

    float _cohesionToAnchorWeight = 0.25f;
    float _separationFromAnchorWeight = 0f;
    float _velMatchToAnchorWeight = 0f;

    // separation options
    float _separationWeight = 2f;
    float _separationAknnVal = 1f;
    float _separationDistance = 5f;
    int _separationK = 5;

    // cohesion options
    float _cohesionWeight = 1f;
    float _cohesionAknnVal = 1f;
    float _cohesionDistance = 30f;
    int _cohesionK = 10;

    // velocity match options
    float _velocityMatchWeight = 0.25f;
    float _velocityMatchAknnVal = 25f;
    float _velocityMatchDistance = 20f;
    int _velocityMatchK = 5;

    float _cohesionMinDotProduct = -0.5f;
    float _separationMinDotProduct = -0.5f;
    float _velMatchMinDotProduct = -0.5f;

    public float percentageOfBoidsToUpdate = 0.6f; // percentage of boids to update every frame

    System.Diagnostics.Stopwatch kdBuildTimer, updateTimer;
    int frameCount = 0;

    int current;

    public FlockWander()
    {
        kdBuildTimer = new System.Diagnostics.Stopwatch();
        updateTimer = new System.Diagnostics.Stopwatch();
    }

    /// <summary>
    /// Re-calculates the path for each bird
    /// </summary>
    public override void Init()
    {
        if (anchor == null)
            anchor = GameObject.Find("Player").GetComponent<Starling>();

        anchorFlock = new Flock(anchor);

        foreach (var entry in entries)
            entry.behavior = getDefaultSteering(entry, flock);
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
            (((entries[i].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[0].behaviour as Separation).useOldValues = true;
            (((entries[i].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[1].behaviour as Cohesion).useOldValues = true;
            (((entries[i].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[2].behaviour as VelocityMatch).useOldValues = true;
        }

        int boidsToUpdate = Mathf.CeilToInt(entries.Count * percentageOfBoidsToUpdate); // make sure at least one boid is updated
        int limit = (current + boidsToUpdate < entries.Count ? current + boidsToUpdate : entries.Count);
        for(; current < limit; ++current)
        {
            (((entries[current].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[0].behaviour as Separation).useOldValues = false;
            (((entries[current].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[1].behaviour as Cohesion).useOldValues = false;
            (((entries[current].behavior as PrioritySteering).Groups[1] as BlendedSteering).Behaviors[2].behaviour as VelocityMatch).useOldValues = false;
        }

        if( current == entries.Count )
            current = 0;

        // update birds and calculate the center of the flock
        for (int i = 0; i < entries.Count; ++i)
            UpdateSteering(dt, entries[i]);

        updateTimer.Stop();
        ++frameCount;
        //Debug.Log("Time to build K-D tree: " + kdBuildTimer.ElapsedMilliseconds / (float)frameCount + ", Time to update: " + updateTimer.ElapsedMilliseconds / (float)frameCount);
    }

    Steering getDefaultSteering(Entry e, Flock flock)
    {
        var separation = new Separation(e.bird, flock, _separationDistance, -0.5f);
        var cohesion = new Cohesion(e.bird, flock, _cohesionDistance, -0.5f);
        var velMatch = new VelocityMatch(e.bird, flock, _velocityMatchDistance, -0.5f);

        var anchorCohesion = new Cohesion(e.bird, anchorFlock, float.MaxValue, -1f); // move towards the anchor
        var anchorSeparation = new Separation(e.bird, anchorFlock, 25f, -1f);
        var anchorVelocityMatch = new VelocityMatch(e.bird, anchorFlock, float.MaxValue, -1f);

        separation.aknnApproxVal = _separationAknnVal;
        cohesion.aknnApproxVal = _cohesionAknnVal;
        velMatch.aknnApproxVal = _velocityMatchAknnVal;

        separation.maxNeighborhoodSize = separationK;
        cohesion.maxNeighborhoodSize = cohesionK;
        velMatch.maxNeighborhoodSize = velocityMatchK;

        var obstacleAvoidance = new ObstacleAvoidance(e.bird, 20f, new string[]{"Ground"});
        
        var blended = new BlendedSteering[2];
        
        blended[0] = new BlendedSteering(e.bird, new BehaviorAndWeight(obstacleAvoidance, 1f));
        blended[1] = new BlendedSteering(e.bird, new BehaviorAndWeight(separation, _separationWeight), // 0
                                                 new BehaviorAndWeight(cohesion, _cohesionWeight), // 1
                                                 new BehaviorAndWeight(velMatch, _velocityMatchWeight), // 2
                                                 new BehaviorAndWeight(anchorCohesion, _cohesionToAnchorWeight), // 3
                                                 new BehaviorAndWeight(anchorSeparation, _separationFromAnchorWeight), // 4
                                                 new BehaviorAndWeight(anchorVelocityMatch, _velMatchToAnchorWeight) // 5
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


    /// <summary>
    /// Gets or sets the cohesion to anchor weight.
    /// </summary>
    public float cohesionToAnchorWeight
    {
        get { return _cohesionToAnchorWeight; }

        set
        {
            _cohesionToAnchorWeight = value;

            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                blendedSteerings.Behaviors[3].weight = _cohesionToAnchorWeight;
            }
        }
    }

    public float separationFromAnchorWeight
    {
        get { return _separationFromAnchorWeight; }
        
        set
        {
            _separationFromAnchorWeight = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                blendedSteerings.Behaviors[4].weight = _separationFromAnchorWeight;
            }
        }
    }

    public float velMatchToAnchorWeight
    {
        get { return _velMatchToAnchorWeight; }
        
        set
        {
            _velMatchToAnchorWeight = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                blendedSteerings.Behaviors[5].weight = _velMatchToAnchorWeight;
            }
        }
    }

    /// <summary>
    /// Gets or sets the separation to the flock weight.
    /// </summary>
    public float separationWeight
    {
        get { return _separationWeight; }
        
        set
        {
            _separationWeight = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                blendedSteerings.Behaviors[0].weight = _separationWeight;
            }
        }
    }

    /// <summary>
    /// Gets or sets the cohesion to the flock weight.
    /// </summary>
    public float cohesionWeight
    {
        get { return _cohesionWeight; }
        
        set
        {
            _cohesionWeight = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                blendedSteerings.Behaviors[1].weight = _cohesionWeight;
            }
        }
    }

    /// <summary>
    /// Gets or sets the velocityMatch to the flock weight.
    /// </summary>
    public float velocityMatchWeight
    {
        get { return _velocityMatchWeight; }
        
        set
        {
            _velocityMatchWeight = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                blendedSteerings.Behaviors[2].weight = _cohesionWeight;
            }
        }
    }

    public float separationAknnVal
    {
        get { return _separationAknnVal; }
        set
        {
            _separationAknnVal = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[0].behaviour as Separation).aknnApproxVal = _separationAknnVal;
            }
        }
    }

    public float separationDistance
    {
        get { return _separationDistance; }
        set
        {
            _separationDistance = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[0].behaviour as Separation).neighborhoodMaxDistance = _separationDistance;
            }
        }
    }

    public float cohesionDistance
    {
        get { return _cohesionDistance; }
        set
        {
            _cohesionDistance = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[1].behaviour as Cohesion).neighborhoodMaxDistance = _cohesionDistance;
            }
        }
    }

    public float velocityMatchDistance
    {
        get { return _velocityMatchDistance; }
        set
        {
            _velocityMatchDistance = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[2].behaviour as VelocityMatch).neighborhoodMaxDistance = _velocityMatchDistance;
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of neighbors that 'Separation' takes into account
    /// </summary>
    public int separationK
    {
        get { return _separationK; }
        set
        {
            _separationK = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[0].behaviour as Separation).maxNeighborhoodSize = _separationK;
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of neighbors that 'Cohesion' takes into account
    /// </summary>
    public int cohesionK
    {
        get { return _cohesionK; }
        set
        {
            _cohesionK = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[1].behaviour as Cohesion).maxNeighborhoodSize = _cohesionK;
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of neighbors that 'Velocity Match' takes into account
    /// </summary>
    public int velocityMatchK
    {
        get { return _velocityMatchK; }
        set
        {
            _velocityMatchK = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[2].behaviour as VelocityMatch).maxNeighborhoodSize = _velocityMatchK;
            }
        }
    }

    public float cohesionAknnVal
    {
        get { return _cohesionAknnVal; }
        set
        {
            _cohesionAknnVal = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[1].behaviour as Cohesion).aknnApproxVal = _cohesionAknnVal;
            }
        }
    }

    public float velocityMatchAknnVal
    {
        get { return _velocityMatchAknnVal; }
        set
        {
            _velocityMatchAknnVal = value;
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[2].behaviour as VelocityMatch).aknnApproxVal = _velocityMatchAknnVal;
            }
        }
    }

    public float CohesionAngle
    {
        get { return Mathf.Acos(_cohesionMinDotProduct) * Mathf.Rad2Deg * 2; }
        set
        {
            _cohesionMinDotProduct = Mathf.Cos((value * 0.5f) * Mathf.Deg2Rad);
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[1].behaviour as Cohesion).neighborhoodMinDotProduct = _cohesionMinDotProduct;
            }
        }
    }

    public float SeparationAngle
    {
        get { return Mathf.Acos(_separationMinDotProduct) * Mathf.Rad2Deg * 2; }
        set
        {
            _separationMinDotProduct = Mathf.Cos((value * 0.5f) * Mathf.Deg2Rad);
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[0].behaviour as Separation).neighborhoodMinDotProduct = _separationMinDotProduct;
            }
        }
    }

    public float VelocityMatchAngle
    {
        get { return Mathf.Acos(_velMatchMinDotProduct) * Mathf.Rad2Deg * 2; }
        set
        {
            _velMatchMinDotProduct = Mathf.Cos((value * 0.5f) * Mathf.Deg2Rad);
            
            foreach(var entry in entries)
            {
                var prioritySteering = entry.behavior as PrioritySteering;
                var blendedSteerings = prioritySteering.Groups[1] as BlendedSteering;
                (blendedSteerings.Behaviors[2].behaviour as VelocityMatch).neighborhoodMinDotProduct = _velMatchMinDotProduct;
            }
        }
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
