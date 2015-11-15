using System.Collections.Generic;
using System;
using UnityEngine;

public class CrowIdle : SingleBirdState
{
	List<Collider> colliders;
	string[] enemyTags;
	
	public CrowIdle(Bird bird, string[] _enemyTags) : base(bird)
	{
		colliders = new List<Collider>();
		enemyTags = _enemyTags;
	}
	
    public override void Update(float dt, Bird bird)
	{
		colliders.Clear();
	
		if( hasVisionOf(bird, enemyTags[0], new string[] {"Ground", "Untagged"}, colliders) )
			bird.state = new CrowHunt(bird, enemyTags);
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

