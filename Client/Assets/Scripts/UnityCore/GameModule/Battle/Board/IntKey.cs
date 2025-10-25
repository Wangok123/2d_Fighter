namespace UnityCore.GameModule.Battle.Board
{
    public class IntKey : LatKey<int>
    {
        public override int Unbox(in LatValue value)
        {
            return value.intVal;
        }

        public override LatValue Box(int value)
        {
            return new LatValue { intVal = value };
        }

        public override bool Equals(LatKey<int> other)
        {
            return other != null && Id == other.Id && Name == other.Name;
        }

        public override int CompareTo(LatKey<int> other)
        {
            throw new System.NotImplementedException();
        }
    }
}