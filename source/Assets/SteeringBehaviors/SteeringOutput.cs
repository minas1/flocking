using System;
using UnityEngine;

public struct SteeringOutput
{
	public Vector3 linearVel; // linear velocity
	//public Vector3 rotation; // rotation in euler angles

	private bool none; // a boolean that indicates "no steering"
	
	public static SteeringOutput None
	{
		get
		{
			SteeringOutput steering = new SteeringOutput();
			steering.none = true;
			return steering; 
		}
	}
	
	/// <summary>
	/// Determines whether this instance is no steering.
	/// </summary>
	public bool IsNoSteering()
	{
		return none;
	}
}

