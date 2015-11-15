using System.Collections.Generic;
using UnityEngine;
using System;

namespace Flocking
{
	/// <summary>
	/// A simple V formation.
	/// </summary>
    public class CrossFormation : FormationPattern
	{
		Entity _anchor;
        public float length; // length of each side

        List<float> distances; // [slotNumber, distance]

        public CrossFormation(Entity _anchor, float _length)
        {
            distances = new List<float>();

            anchor = _anchor;
            length = _length;
        }

        public override void Update(float dt)
        {
        }

        public override void Add(FormationManager.SlotAssignment slotAssignment)
        {
            float distance = UnityEngine.Random.Range(0f, length);
            distances.Add(distance);
        }

        public override void Remove(FormationManager.SlotAssignment slotAssignment)
        {
            distances.RemoveAt(slotAssignment.slotNumber);
        }

		public override int GetDriftOffset(List<FormationManager.SlotAssignment> slotAssignments)
		{
			return 0;
		}
		
		public override Vector3 GetSlotPosition(int slotNumber)
		{
            if (distances.Count == 0)
                return new Vector3(float.NaN, float.NaN, float.NaN);

            Vector3 pos;

            var v = anchor.velocity;
            v.y = 0f;
            v.Normalize();


            if( slotNumber % 4 == 0 )
                pos = anchor.position + Quaternion.AngleAxis(90f, Vector3.up) * v * distances[slotNumber];
            else if( slotNumber % 4 == 1 )
                pos = anchor.position + Quaternion.AngleAxis(270f, Vector3.up) * v * distances[slotNumber];
            else if( slotNumber % 4 == 2 )
                pos = anchor.position + Vector3.down * distances[slotNumber];
            else
                pos = anchor.position + Vector3.up * distances[slotNumber];

            return pos;
		}
		
		public override bool SupportsSlots(int slotCount)
		{
            return true;
		}
		
        public override Entity anchor
        {
            get { return _anchor; }
            set { _anchor = value; }
        }
	}
}

