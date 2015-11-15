using System.Collections.Generic;
using UnityEngine;
using SteeringBehaviors;

public class CrowHunt : SingleBirdState
{
	List<Collider> colliders;
	string[] enemyTags;
	
	public CrowHunt(Bird bird, string[] _enemyTags) : base(bird)
	{
		colliders = new List<Collider>();
		enemyTags = _enemyTags;
	}
	
	public override void Update(float dt, Bird bird)
	{
		colliders.Clear();
		
		if( hasVisionOf(bird, enemyTags[0], new string[]{"Ground", "Untagged"}, colliders) )
		{
			List<Collider> colls = colliders.FindAll(x => x.tag == enemyTags[0]);
			Bird birdToAttack = null;
			if( colls[0].tag == "Starling" )
				birdToAttack = colls[0].GetComponent<Starling>();
			else if( colls[0].tag == "Crow" )
				birdToAttack = colls[0].GetComponent<Crow>();

			Debug.Log("TODO: CrowHunt [fix] for all tags");

			if( behavior == null )
			{
				/*var blended = new BlendedSteering[2];
				blended[0] = new BlendedSteering(25f, new BehaviorAndWeight(new ObstacleAvoidance(bird, 50f, 25f), 1f));
				blended[1] = new BlendedSteering(25f, new BehaviorAndWeight(new Seek(bird, player, 25f), 1f));
				behaviour = new PrioritySteering(1f, blended);*/
				
				behavior = new Seek(bird, birdToAttack);
			}
			
			// if the player is within attack distance, attack him
			float distance = Vector3.Distance(birdToAttack.transform.position, bird.transform.position);
			if( distance <= CrowAttack.ATTACK_DISTANCE )
				bird.state = new CrowAttack(bird, birdToAttack, enemyTags);
			
		}
		else
		{
			bird.state = new CrowAlert(bird, enemyTags);
		}
		
		UpdateSteering(dt);
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

