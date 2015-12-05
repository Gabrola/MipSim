using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipSim
{
    public class ExecutionRecordList : List<ExecutionRecord>
    {
        protected bool Equals(ExecutionRecordList other)
        {
            return other.SequenceEqual(this);
        }

        public override int GetHashCode()
        {
            return this.Distinct().Aggregate(0, (x, y) => x.GetHashCode() ^ y.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ExecutionRecordList && Equals((ExecutionRecordList) obj);
        }
    }
}
