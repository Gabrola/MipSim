using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MipSim.CPUComponents
{
    public class BTB
    {
        private readonly BTBEntry[] _entries;

        public BTB()
        {
            _entries = new BTBEntry[10];
        }

        public bool PredictBranch(int PC, out int PredictedPC)
        {
            int index = (PC >> 2) % 10;

            PredictedPC = 0;

            if (!_entries[index].Valid || _entries[index].BranchPC != PC)
                return false;

            PredictedPC = _entries[index].PredictedPC;

            return _entries[index].PredictionState;
        }

        public void UpdatePrediction(int PC, int PredictedPC, bool BranchTaken)
        {
            int index = (PC >> 2) % 10;

            if (!BranchTaken && (!_entries[index].Valid || _entries[index].BranchPC != PC))
                return;

            _entries[index].Valid = true;
            _entries[index].BranchPC = PC;
            _entries[index].PredictedPC = PredictedPC;
            _entries[index].PredictionState = BranchTaken;
        }

        public struct BTBEntry
        {
            public int BranchPC;
            public int PredictedPC;
            public bool PredictionState;
            public bool Valid;
        }
    }
}
