using System;
using UnityEngine;

public class Flee : Steering
{
	public Entity character;
	public Entity target;
    public float radius;

	// constructor
	public Flee(Entity _character, Entity _target, float _radius)
	{
		character = _character;
		target = _target;
        radius = _radius;
	}

    // constructor
    public Flee(Entity _character, Entity _target)
       : this(_character, _target, float.MaxValue)
    {
    }
	
	public virtual SteeringOutput GetSteering()
	{
		SteeringOutput steering = new SteeringOutput();
		
        if( Vector3.Distance(character.position, target.position) <= radius )
        {
    		// get the direction to the target
    		steering.linearVel = character.position - target.position;
    		
    		// the velocity is along this direction, at full speed
    		steering.linearVel.Normalize();
    		steering.linearVel *= character.maxSpeed;
    		
    		return steering;
        }

        return SteeringOutput.None;
	}

    public void Destroy()
    {
    }
}
