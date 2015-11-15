using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;
using Flocking;

using System.Threading;

public class FloatVertical : MultipleBirdState
{
    List<Collider> colliders;


    float t = 0f;

    int currentFrame;

    Vector3 flockCenter;

    public FloatVertical(List<Entry> _entries)
    {
        colliders = new List<Collider>();

        // copy entries given as our entries
        foreach (var e in _entries)
            Add(e.bird);
    }

    public FloatVertical()
    {
        colliders = new List<Collider>();
    }

    public override Entry Add(Bird b)
    {
        var entry = base.Add(b);

        var separation = new Separation(b, flock, 5f, -1f);
        var cohesion = new Cohesion(b, flock, 40f, -1f);

        separation.aknnApproxVal = 1f;
        cohesion.aknnApproxVal = 1f;

        var blended = new BlendedSteering[1];
        blended[0] = new BlendedSteering(b,
                                         new BehaviorAndWeight(separation, 2f)
                                         ,new BehaviorAndWeight(cohesion, 1f)
                                          );

        entry.behavior = new PrioritySteering(1f, blended);

        return entry;
    }

    // test performed when the flock must become flat
    bool testFlatFlock(Vector3 charPos, Vector3 charVel, Vector3 pos, Vector3 vel, float maxDistance)
    {
        return Vector3.Distance(charPos, pos) <= maxDistance && Mathf.Abs(charPos.y - pos.y) <= 5f;
    }

    public override void Update(float dt)
    {
        if (Time.frameCount == currentFrame)
            return;
        currentFrame = Time.frameCount;

        flock.RebuildKdTree();

        var center = Vector3.zero; // center of the flock
        var avgDist = Vector3.zero; // average distance from last known center
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue); // max (x,y,z) for the flock

        // update birds
        for (int i = 0; i < entries.Count; ++i)
        {
            var bird = entries[i].bird;

            UpdateSteering(dt, entries[i]);

            // calculate the center of the flock
            center += bird.position;

            // calculate average distance from last known center (of the flock)
            avgDist.x += Mathf.Abs(bird.position.x - flockCenter.x);
            avgDist.y += Mathf.Abs(bird.position.y - flockCenter.y);
            avgDist.z += Mathf.Abs(bird.position.z - flockCenter.z);

            if (bird.position.x > max.x) max.x = bird.position.x;
            if (bird.position.y > max.y) max.y = bird.position.y;
            if (bird.position.z > max.z) max.z = bird.position.z;
        }

        center /= entries.Count;
        flockCenter = center; // use the new center as the flock's center

        avgDist /= entries.Count;

        //Debug.Log(avgDist.x + ", " + avgDist.y);

        for(int i = 0; i < entries.Count; ++i)
        {
            var pos = entries[i].bird.position;

            float WIDTH = entries.Count / 5;
            float HEIGHT = Mathf.Pow(WIDTH, 0.85f);

            //Debug.Log(WIDTH + ", " + HEIGHT);

            // move towards the center of the flock
            if( Mathf.Abs(pos.x - flockCenter.x) > WIDTH || Mathf.Abs(pos.y - flockCenter.y) > HEIGHT )
            {
                var path = new Path();

                path.Add(flockCenter);
                
                var separation = new Separation(entries[i].bird, flock, 20.0f, 0f);
                var cohesion = new Cohesion(entries[i].bird, flock, 40, 0f);
                
                separation.aknnApproxVal = 2.0;
                cohesion.aknnApproxVal = 100.0;
                
                entries[i].behavior = new BlendedSteering(entries[i].bird, new BehaviorAndWeight(new PathFollow(entries[i].bird, path), 0.015f), new BehaviorAndWeight(separation, 2f), new BehaviorAndWeight(cohesion, 1f));
            }
            else
            {
                var separation = new Separation(entries[i].bird, flock, 7.5f, -1f);
                var cohesion = new Cohesion(entries[i].bird, flock, 25f, -1f);
                
                separation.aknnApproxVal = 2.0;
                cohesion.aknnApproxVal = (entries.Count  < 200 ? 2f : entries.Count * 0.04);

                var blended = new BlendedSteering[1];
                blended[0] = new BlendedSteering(entries[i].bird,
                                                 new BehaviorAndWeight(separation, 2f)
                                                 ,new BehaviorAndWeight(cohesion, 1f)
                                                 );
                
                entries[i].behavior = new PrioritySteering(1f, blended);
            }
        }

        // get colliders with tag "Starling" in a sphere around the center of the flock
        colliders.Clear();
        Util.GetCollidersInRange(colliders, "Starling", center, 40f);

        List<Collider> visibleGameObjects;
    
        // if there are no obstacles
        //if (bird.viewVolume2.NumColliders("Ground") == 0 && bird.viewVolume2.NumColliders("Untagged") == 0)
        {
            visibleGameObjects = colliders;
        }
        //else
        {
            //visibleGameObjects = SingleBirdState.getVisibleGameObjects(e.bird, new string[]{"Ground","Untagged"}, colliders);
        }
    
        var starlings = visibleGameObjects;

        // add other birds to the flock
        CheckAddBirdsToFlock(starlings);

        //t += dt;
        if( t >= 1f )
        {
            //var division = new FlockDivision(GetEntries());
            /*var division = new FlockWander(GetEntries());
            for(int i = 0; i < entries.Count; ++i)
            {
                entries[i].bird.state = division;
            }*/
        }
    }
    //private void Update(float dt, Entry e)
    //{
        //t += dt / entries.Count;
        //if( t >= 2f )
        //{
            //var division = new FlockDivision(GetEntries());
            //var division = new FlockPositionChange(GetEntries());
            //foreach(var entry in entries.Values)
            //    entry.bird.state = division;
            
            /*var behavior = (PrioritySteering)e.behavior;
            var groups = behavior.Groups;
            var behaviors = groups[0].Behaviors;

            var separation = (Separation)behaviors[0].behaviour;
            separation.neighbourhoodMaxDistance = 10f;

            var cohesion = (Cohesion)behaviors[1].behaviour;
            cohesion.neighbourhoodMaxDistance = 50f;*/
        //}
    //}

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

    // checks if starlings can be added to the flock and if yes, adds them
    void CheckAddBirdsToFlock(List<Collider> starlings)
    {
        if (starlings.Count != 0 )
        {
            foreach (var star in starlings)
            {
                var starling = star.GetComponent<Starling>();

                if( starling.state == this )
                    continue;

                if( starling.state.IsSingleBirdState && ((SingleBirdState)starling.state).canSwitchToMultipleState )
                {
                    starling.state = this;
                    Add(starling);
                }
                else // if this bird belongs in a flock, merge the flocks
                {
                    var otherFlockEntries = ((MultipleBirdState)starling.state).GetEntries();
                    foreach(var otherEntry in otherFlockEntries)
                    {
                        otherEntry.bird.state = this;
                        Add(otherEntry.bird);
                    }
                }
            }
        }
    }

    public override void GetFlockInfo(out Vector3 center, out Vector3 avgVel)
    {
        center = avgVel = Vector3.zero;
    }
}