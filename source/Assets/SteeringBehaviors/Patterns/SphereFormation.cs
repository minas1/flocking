using System.Collections.Generic;
using UnityEngine;
using System;

namespace Flocking
{
	/// <summary>
	/// A simple V formation.
	/// </summary>
	public class SphereFormation : FormationPattern
	{
		Entity _anchor;
		public float maxDistanceFromFrontEntity; // max distance from the anchor
        public float minDistanceFromAnchor;

        List<Vector3> positions; // [slotNumber, position]

        float t = 2f;

        public SphereFormation(Entity _anchor, float _maxDistanceFromFrontEntity, float _minDistanceFromAnchor)
        {
            positions = new List<Vector3>();

            anchor = _anchor;
            maxDistanceFromFrontEntity = _maxDistanceFromFrontEntity;
            minDistanceFromAnchor = _minDistanceFromAnchor;
        }

        public override void Update(float dt)
        {
            t -= dt;

            if (t <= 0f)
            {
                t = UnityEngine.Random.Range(0.5f, 2.5f);

                // do % changes
                int count = UnityEngine.Random.Range(0, Mathf.CeilToInt(positions.Count * 0.05f));

                for(int k = 0; k < count; ++k)
                {
                    int i = UnityEngine.Random.Range(0, positions.Count);

                    positions[i] = getRandomVector();
                }
            }
        }

        public override void Add(FormationManager.SlotAssignment slotAssignment)
        {
            var v = getRandomVector();

            positions.Add(v);

            /*string str = "pattern: ";
            for(int i = 0; i < positions.Count; ++i)
                str += positions[i] + " ";
            Debug.Log(str);*/
        }

        Vector3 getRandomVector()
        {
            float theta = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
            float phi = UnityEngine.Random.Range(0f, Mathf.PI);

            Vector3 v;

            float distance = minDistanceFromAnchor + UnityEngine.Random.Range(0, maxDistanceFromFrontEntity - minDistanceFromAnchor);

            v.x = distance * Mathf.Cos(theta) * Mathf.Sin(phi);
            v.y = distance * Mathf.Sin(theta) * Mathf.Sin(phi);
            v.z = distance * Mathf.Cos(phi);

            return v;
        }
        
        public override void Remove(FormationManager.SlotAssignment slotAssignment)
        {
            positions.RemoveAt(slotAssignment.slotNumber);

            /*Debug.Log("removing " + slotAssignment.slotNumber);
            string str = "pattern: ";
            for(int i = 0; i < positions.Count; ++i)
                str += positions[i] + " ";
            Debug.Log(str);*/
        }

		public override int GetDriftOffset(List<FormationManager.SlotAssignment> slotAssignments)
		{
			return 0;
		}
		
		public override Vector3 GetSlotPosition(int slotNumber)
		{
            if (positions.Count == 0)
                return new Vector3(float.NaN, float.NaN, float.NaN);

            var pos = anchor.position + positions[slotNumber];

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

        public void setVector(int index, Vector3 vec)
        {
            positions[index] = vec;
        }

        public Vector3 getVector(int index)
        {
            return positions[index];
        }
	}
}

