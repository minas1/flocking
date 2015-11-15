using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Flocking;

public abstract class Bird : MonoBehaviour, Entity
{
	private float _maxSpeed = 7f;

    public BirdState state;
	
	// Use this for initialization
	public abstract void Start();
	
	// Update is called once per frame
	public abstract void Update();
	
	public abstract void FixedUpdate();
	
	public Transform Transform() { return transform; }

	public Vector3 velocity
	{
		get { return GetComponent<Rigidbody>().velocity; }
		set { GetComponent<Rigidbody>().velocity = value; }
	}

    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

	public Vector3 Velocity() { return GetComponent<Rigidbody>().velocity; }
	
	void OnTriggerEnter(Collider other)
	{
        //rigidbody.angularVelocity = Vector3.zero;
		Debug.Log("parent enter");
	}
	
	void OnTriggerStay(Collider other)
	{
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		//Debug.Log("parent stay");
	}
	
	void OnTriggerExit(Collider other)
	{
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		//Debug.Log("parent exit");	
	}

	void OnCollisionEnter(Collision other)
	{
	}

	void OnCollisionStay(Collision other)
	{

		//state.onCollisionStay(other);
	}

	void OnCollisionExit(Collision other)
	{

		//state.onCollisionExit(other);
	}

    public float maxSpeed
    {
        get { return _maxSpeed; }
        set { _maxSpeed = value; }
    }
}
