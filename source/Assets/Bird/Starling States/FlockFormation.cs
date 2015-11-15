using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;
using Flocking;

using System.Threading;

public class FlockFormation : MultipleBirdState
{
    System.Diagnostics.Stopwatch updateTimer;
    int frameCount = 0;

    int currentFrame = 0;

    public Entity anchor;
  
    FormationManager generalManager; // the general formation manager used

    Dictionary<Entity, FormationManager> formationManagers;

    Flock anchorFlock;

    public FlockFormation(List<Vector3> vertices)
    {
        formationManagers = new Dictionary<Entity, FormationManager>();

        updateTimer = new System.Diagnostics.Stopwatch();

        var pattern = new ModelFormation(null, vertices);
        var formationManager = new FormationManager(pattern);
        generalManager = formationManager;
    }

    /// <summary>
    /// Re-calculates the path for each bird
    /// </summary>
    public override void Init()
    {
        if (anchor == null)
            anchor = GameObject.Find("Player").GetComponent<Starling>();

        anchorFlock = new Flock(anchor);
        flock.Add(anchor);

        generalManager.pattern.anchor = anchor;
        //anchor.maxSpeed = 0f;

        //////
        formationManagers.Clear();
        foreach (var entry in entries)
        {
            generalManager.AddCharacter(entry.bird);
            formationManagers.Add(entry.bird, generalManager);
            entry.behavior = getDefaultSteering(entry, generalManager, flock);
        }
    }

    public override void Update(float dt)
    {
        if (Time.frameCount == currentFrame) return;
        currentFrame = Time.frameCount;

        ++frameCount;

        flock.RebuildKdTree();
        anchorFlock.RebuildKdTree();

        updateTimer.Start();

        // update birds and calculate the center of the flock
        for (int i = 0; i < entries.Count; ++i)
            UpdateSteering(dt, entries[i]);

        updateTimer.Stop();
        //Debug.Log("Time to update: " + updateTimer.ElapsedMilliseconds / (float)frameCount);
    }

    Steering getDefaultSteering(Entry e, FormationManager generalManager, Flock flock)
    {
        var pattSteering = new PatternSteering(e.bird, generalManager);
        var separation = new Separation(e.bird, flock, 5.0f, -1f);
        separation.aknnApproxVal = 1.0;
        var obstacleAvoidance = new ObstacleAvoidance(e.bird, 20f, new string[]{"Ground"});
        
        var blended = new BlendedSteering[3];
        
        blended[0] = new BlendedSteering(e.bird, new BehaviorAndWeight(obstacleAvoidance, 1f));
        blended[1] = new BlendedSteering(e.bird, new BehaviorAndWeight(separation, 1f));
        blended[2] = new BlendedSteering(e.bird, new BehaviorAndWeight(pattSteering, 1f));

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
}
