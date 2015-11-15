using System.Collections.Generic;
using UnityEngine;
using SteeringBehaviors;

namespace Flocking
{
    /// <summary>
    /// Processes a formation pattern and generates targets for the characters occupying its slots.
    /// </summary>
    public class FormationManager
    {
        // holds the assignment of a single character to a slot 
        public class SlotAssignment
        {
            public Entity character;
            public int slotNumber;
        }
        
        static int count;
        int id;
        
        // holds a list of slot assignments
        Dictionary<Entity, SlotAssignment> slotAssignments;

        public FormationPattern pattern;

        Arrive arrive;
        DummyEntity dummy;
        
        public FormationManager(FormationPattern _pattern)
        {
            pattern = _pattern;
            slotAssignments = new Dictionary<Entity, SlotAssignment>();

            arrive = new Arrive(null, null, 3f, 5f);
            dummy = new DummyEntity();
            
            id = count++;
        }
        
        public void Update(float dt)
        {
            pattern.Update(dt);
        }
        
        /// <summary>
        /// Updates the assignment of characters to slots.
        /// </summary>
        public void UpdateSlotAssignments()
        {
            // simply go through each assignment in the list and assign
            // sequential slot numbers
            int count = 0;

            var tempDict = new Dictionary<Entity, SlotAssignment>();

            foreach(var val in slotAssignments.Values)
            {
                var slot = val;
                slot.slotNumber = count++;

                tempDict.Add(slot.character, slot);
            }

            slotAssignments = tempDict;
        }
        
        /// <summary>
        /// Adds a new character to the first available slot. Returns false if no more slots are available.
        /// </summary>
        public bool AddCharacter(Entity character)
        {
            // find out how many slots we have occupied
            int occupiedSlots = slotAssignments.Count;
            
            // check if the pattern supports more slots
            if( !pattern.SupportsSlots(occupiedSlots + 1) )
                return false;
            
            // add a new slot assignment
            var slotAssignment = new SlotAssignment();
            slotAssignment.character = character;

            slotAssignments.Remove(character);
            slotAssignments.Add(character, slotAssignment);

            // update the slot assignents and return success
            UpdateSlotAssignments();

            pattern.Add(slotAssignment);

            return true;
        }
        
        /// <summary>
        /// Removes a character from its slot.
        /// </summary>
        public void RemoveCharacter(Entity character)
        {
            if( !slotAssignments.ContainsKey(character) )
                return;
            
            var slot = slotAssignments[character];
            
            pattern.Remove(slot);
            slotAssignments.Remove(character);

            UpdateSlotAssignments();
        }
        
        public SteeringOutput GetSteering(Entity character)
        {
            var slot = slotAssignments[character];
            
            var position = pattern.GetSlotPosition(slot.slotNumber);
            
            // set the dummy entity's position
            dummy.position = position;
            
            // set the seek characters
            arrive.character = character;
            arrive.target = dummy;
            
            return arrive.GetSteering();
        }
        
        public int Id
        {
            get { return id; }
        }
        
        public int Count
        {
            get { return slotAssignments.Count; }
        }

        /// <returns>The slot number that is assigned to this entity</returns>
        public int SlotNumber(Entity e)
        {
            return slotAssignments[e].slotNumber;
        }
    }
}