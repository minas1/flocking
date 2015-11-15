using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
	public class PatternSteering : Steering
	{
		Entity character;
		FormationManager manager;
		
		public PatternSteering(Entity _character, FormationManager _manager)
		{
			character = _character;
			manager = _manager;
		}
		
		public SteeringOutput GetSteering()
        {
			return manager.GetSteering(character);
		}
	}
}

