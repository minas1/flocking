namespace SteeringBehaviors
{
    public class BehaviorAndWeight
    {
        public BehaviorAndWeight(Steering _behaviour, float _weight)
        {
            behaviour = _behaviour;
            weight = _weight;
        }
        

        public Steering behaviour;
        public float weight;
    }
}
