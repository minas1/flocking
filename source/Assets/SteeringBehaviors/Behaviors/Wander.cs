using System;
using UnityEngine;

namespace SteeringBehaviors
{
	public class Wander : Steering
	{
		public Entity character;
		public float maxAcceleration;
		
		float circleRadius = 20f;
		
		public float maxAngle; // max angle to rotate
		public float theta = 0f; // the angle we rotating left/right (in degrees)

        public float maxYSpeed; // maximum speed/second for Y axis
        public float customYSpeed; // custom value added to target's Y position

		DummyEntity target;
		Seek seek;
		
        public Bounds bbox; // bounding box where the bird is allowed to fly in

		// constructor
		public Wander(Entity _character, float _maxAcceleration, float _maxAngle, float _maxYSpeed, Bounds _bbox)
		{
			character = _character;
			maxAcceleration = _maxAcceleration;
			maxAngle = _maxAngle;
			maxYSpeed = _maxYSpeed;
            bbox = _bbox;

            target = new DummyEntity();
			seek = new Seek(character, target);
		}
		
		// constructor with default arguments
		public Wander(Entity _character, float _maxAcceleration, Bounds _bbox)
			: this(_character, _maxAcceleration, 5f, 2f, _bbox)
		{
		}
		
        public SteeringOutput GetSteering()
		{
			theta += Util.RandomBinomial() * maxAngle;

            var pos = character.transform.position;

            // check if we are inside the bounding box
            if( !bbox.Contains(character.position) )
            {
                pos.y = bbox.center.y;
                LookAt(bbox.center);
            }

			pos.x += circleRadius * Mathf.Cos(theta * Mathf.Deg2Rad);
			pos.y += maxYSpeed * Util.RandomBinomial() * Time.deltaTime;
            pos.y += customYSpeed * Time.deltaTime;
            pos.z += circleRadius * Mathf.Sin(theta * Mathf.Deg2Rad);

            target.position = pos;

            return seek.GetSteering();
		}

        /// <summary>
        /// Makes the bird look at the given position
        public void LookAt(Vector3 position)
        {
            var u = position - character.position;
            u.Normalize();
            
            var referenceForward = Vector3.right;
            var referenceRight = Vector3.Cross(Vector3.up, referenceForward);
            
            var angle = Vector3.Angle(u, referenceForward);
            float sign = Mathf.Sign(Vector3.Dot(u, referenceRight));
            
            angle *= -sign;
            theta = angle;

            customYSpeed = 0f;
        }

        /// <summary>
        /// Gets the position where the entity is heading
        /// </summary>
        public Vector3 targetPosition
        {
            get { return target.position; }
        }
	}
}

