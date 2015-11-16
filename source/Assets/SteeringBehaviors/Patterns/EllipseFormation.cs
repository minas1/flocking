using System.Collections.Generic;
using UnityEngine;
using System;

namespace Flocking
{
    /// <summary>
    /// A simple V formation.
    /// </summary>
    public class EllipseFormation : FormationPattern
    {
        Entity _anchor;
        List<Vector3> positions;

        float t = 0f;
        public float a, b, c; // parameters for the ellipse

        public EllipseFormation(Entity _anchor, int slotCount, float _a, float _b, float _c)
        {
            anchor = _anchor;

            a = _a;
            b = _b;
            c = _c;

            positions = new List<Vector3>();
        }

        /// <summary>
        /// Generates a point inside the ellipse with center 0,0,0 and parameters a, b, c
        /// </summary>
        Vector3 genPointInEclipse(float a, float b, float c)
        {
            var u = UnityEngine.Random.Range(Mathf.PI * -0.5f, Mathf.PI * 0.5f);
            var v = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
            
            var pos = Vector3.zero;
            
            pos.x = a * Mathf.Cos(u) * Mathf.Cos(v);
            pos.y = b * Mathf.Cos(u) * Mathf.Sin(v);
            pos.z = c * Mathf.Sin(u);



            /*a = 40.1f;
            b = 30.5f;
            c = 5.1f;
            var theta = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            v = UnityEngine.Random.Range(-1f, 1f);

            pos.x = (float)(a * Math.Cosh(v) * Math.Cos(theta));
            pos.y = (float)(b * Math.Cosh(v) * Math.Sin(theta));
            pos.z = (float)(c * Math.Sinh(v));*/

            return pos;
        }

        public override void Update(float dt)
        {
            t += dt;

            if (t >= 0.5f)
            {
                // do % changes
                int count = Mathf.CeilToInt(positions.Count * 0.05f);

                t = 0f;

                for(int k = 0; k < count; ++k)
                {
                    int i = UnityEngine.Random.Range(0, positions.Count);

                    var pos = anchor.position + genPointInEclipse(a, b, c);
                    positions[i] = pos - anchor.position;
                }
            }
        }

        public override void Add(FormationManager.SlotAssignment slotAssignment)
        {
            var pos = anchor.position + genPointInEclipse(a, b, c);
            positions.Add(pos - anchor.position);
        }

        public override void Remove(FormationManager.SlotAssignment slotAssignment)
        {
            positions.RemoveAt(slotAssignment.slotNumber);
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
    }
}

