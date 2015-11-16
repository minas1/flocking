using System.Collections.Generic;
using UnityEngine;
using System;

namespace Flocking
{
    public class ModelFormation : FormationPattern
    {
        Entity _anchor;
        List<Vector3> positions;

        List<Vector3> vertices;

        public ModelFormation(Entity _anchor, List<Vector3> vertices)
        {
            anchor = _anchor;
            positions = new List<Vector3>(vertices);
        }

        public override void Update(float dt)
        {
        }
        
        public override void Add(FormationManager.SlotAssignment slotAssignment)
        {
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

            if (slotNumber >= positions.Count)
                return anchor.position + positions[UnityEngine.Random.Range(0, positions.Count)];

            return anchor.position + positions[slotNumber];
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

