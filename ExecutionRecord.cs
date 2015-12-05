namespace MipSim
{
    public struct ExecutionRecord
    {
        public readonly ExecutionType Type;
        public readonly string Value;
        public readonly int ExecutionNumber;

        public ExecutionRecord(ExecutionType type, string value, int executionNumber)
        {
            Type = type;
            Value = value;
            ExecutionNumber = executionNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ExecutionRecord && Equals((ExecutionRecord) obj);
        }
        public bool Equals(ExecutionRecord other)
        {
            return Type == other.Type && string.Equals(Value, other.Value) && ExecutionNumber == other.ExecutionNumber;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ExecutionNumber;
                return hashCode;
            }
        }
    }
}
