using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Flocking;

public class Crow : Bird
{
	// Use this for initialization
	public override void Start()
	{
		state = new CrowWander(this, new string[]{"Starling", "Player"});
	}
	
	// Update is called once per frame
	public override void Update()
	{
		/*var lineRenderer = GetComponent<LineRenderer> ();
		lineRenderer.enabled = true;

		lineRenderer.SetPosition(0, transform.position + transform.forward * 1f);
		lineRenderer.SetPosition (1, transform.position + (Quaternion.Euler (0f, 0f, 0f) * transform.forward).normalized * 20f);*/

		//Debug.Log("bird state = " + state.GetType().ToString());
		
		state.Update(Time.deltaTime, this);

	}
	
	public override void FixedUpdate()
	{
		state.FixedUpdate();
	}
}
