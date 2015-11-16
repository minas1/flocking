using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    /// <summary>
    /// A simple V formation.
    /// </summary>
    public class VFormation : FormationPattern
    {
        Entity _anchor;
        public float angle;
        public float distanceFromFrontEntity; // distance from the entity in front
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Flocking.VFormation"/> class.
        /// </summary>
        /// <param name='_anchor'>
        /// The entity that acts as the formation's anchor point.
        /// </param>
        /// <param name='_angle'>
        /// The angle from the angle at which the other entities are positioned.
        /// </param>
        public VFormation(Entity a, float _angle, float _distanceFromFrontEntity)
        {
            _anchor = a;
            angle = _angle;
            distanceFromFrontEntity = _distanceFromFrontEntity;
        }

        public VFormation()
        {
        }

        public override void Update(float dt)
        {
        }

        public override void Add(FormationManager.SlotAssignment slotAssignment)
        {
        }
        
        public override void Remove(FormationManager.SlotAssignment slotAssignment)
        {
        }

        public override int GetDriftOffset(List<FormationManager.SlotAssignment> slotAssignments)
        {
            return 0;
        }
        
        public override Vector3 GetSlotPosition(int slotNumber)
        {
            var dir = Vector3.zero;
            if( slotNumber % 2 == 0 )
                dir = Quaternion.AngleAxis(-90f - angle, new Vector3(0f, 1f, 0f)) * anchor.velocity;
            else
                dir = Quaternion.AngleAxis(90f + angle, new Vector3(0f, 1f, 0f)) * anchor.velocity;
            dir.Normalize();


            return anchor.position + dir * (slotNumber + 1) * distanceFromFrontEntity;
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

