using System;
using UnityEngine;

namespace SteeringBehaviors
{
	public class ObstacleAvoidance : Steering
	{
		// holds the minimum distance to a wall (i.e how far to avoid collision). Should be greater than
		// the radius of the character.
		float avoidDistance = 20f;
		
		// holds the distance to look ahead for a collision (i.e the length of the collision ray)
        float lookahead;
		
		public Entity character;
		
		Seek seek;
		DummyEntity dummy;
		
		string[] tagsToAvoid; // list of tags that the behavior should avoid
		
        SteeringOutput _lastSteering;

		public ObstacleAvoidance(Entity _character, float _lookahead, string[] _tagsToAvoid)
		{
			character = _character;
			lookahead = _lookahead;
			tagsToAvoid = _tagsToAvoid;
			
			dummy = new DummyEntity();
			seek = new Seek(character, dummy);
		}
		
		public SteeringOutput GetSteering()
		{
			var hitInfoForward = new RaycastHit();

			var layerMask = (1 << 8); // bit shift by the index of the layer we want (NoCollision layer)
			layerMask = ~layerMask; // this will force to test only for collisions other than the above

			bool collisionForward = Physics.Raycast(new Ray(character.transform.position, character.transform.forward), out hitInfoForward, lookahead, layerMask);

			bool canMoveForward = !HitFound(collisionForward, hitInfoForward);

            if( !canMoveForward )
            {
                dummy.position = hitInfoForward.point + hitInfoForward.normal * avoidDistance;
            }
            else
            {
                var hitInfoVertical = new RaycastHit();
                bool collisionVertical = (character.maxSpeed > 0f ? Physics.Raycast(new Ray(character.transform.position, Vector3.down), out hitInfoVertical, character.maxSpeed * 0.25f, layerMask) : false);
                bool canMoveVertical = !HitFound(collisionVertical, hitInfoVertical);

                if( !canMoveVertical )
                    dummy.position = character.position + character.transform.forward * 10f + Vector3.up * 10f;
                else
                    return (_lastSteering = SteeringOutput.None);
            }

			return (_lastSteering = seek.GetSteering());
		}

		bool HitFound(bool collision, RaycastHit hitInfo)
		{
			return collision && Array.Exists(tagsToAvoid, x => x == hitInfo.collider.gameObject.tag);
		}

        /// <summary>
        /// Returns the last steering
        /// </summary>
        public SteeringOutput lastSteering
        {
            get { return _lastSteering; }
        }

        /// <summary>
        /// Returns the position that entity has to seek to in order to avoid obstacles
        /// </summary>
        public Vector3 targetPosition
        {
            get { return dummy.position; }
        }
	}
}

