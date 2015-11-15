using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
	public abstract class FormationPattern
	{
		// holds the number of slots currently in the pattern. This is updated in the
		// GetDriftOffset method. It may be a fixed value.
		protected int numberOfSlots;
		
		public abstract int GetDriftOffset(List<FormationManager.SlotAssignment> slotAssignments);
		
		/// <summary>
		/// Gets the location of the given slot index.
		/// </summary>
		public abstract Vector3 GetSlotPosition(int slotNumber);
		
        public abstract void Update(float dt);

		/// <summary>
		/// Returns true if the pattern can support the given number of slots.
		/// </summary>
		public abstract bool SupportsSlots(int slotCount);
		
        public abstract void Add(FormationManager.SlotAssignment slotAssignment);
        public abstract void Remove(FormationManager.SlotAssignment slotAssignment);

        public abstract Entity anchor
        {
            get;
            set;
        }
	}
}

