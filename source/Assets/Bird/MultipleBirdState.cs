using System;
using System.Collections.Generic;
using UnityEngine;
using Flocking;

public abstract class MultipleBirdState : BirdState
{
    public class Entry
    {
        public Bird bird;
        public Steering behavior;
    }

    protected Flock flock;
    protected List<Entry> entries;

    protected static int count;
    protected int id;

    public MultipleBirdState()
    {
        entries = new List<Entry>();
        flock = new Flock();

        id = count++;
    }

    /// <summary>
    /// Adds a bird to this state and to its enclosing flock.
    /// </summary>
    public virtual Entry Add(Bird b)
    {
        var entry = new Entry();
        entry.bird = b;
        entries.Add(entry);

        flock.Add(b.GetComponent<Starling>());

        return entry;
    }

    public virtual void Init()
    {
    }

    public virtual void Remove(Bird b)
    {
        entries.RemoveAll(x => x.bird == b);
    }

    public List<Entry> GetEntries()
    {
        return entries;
    }

    public void RemoveAllEntries()
    {
        entries.Clear();
    }
    
    public abstract void Update(float dt, Bird bird);

    public abstract void Update(float dt);

    public abstract void FixedUpdate();

    public abstract void onCollisionEnter(Collision collision);
    public abstract void onCollisionStay(Collision collision);
    public abstract void onCollisionExit(Collision collision);

    /// <summary>
    /// Updates the velocity/rotation of all birds
    /// </summary>
    protected void UpdateSteering(float dt, Entry e)
    {
        SingleBirdState.UpdateSteering(e.behavior, e.bird, dt);
    }

    public abstract void GetFlockInfo(out Vector3 center, out Vector3 avgVel);

    public bool IsSingleBirdState
    {
        get { return false; }
    }
}


