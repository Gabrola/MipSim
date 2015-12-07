using System;

namespace MipSim.CPUComponents
{
    public class ProgramCounter
    {
        private int _counter;

        public int ArrayCounter
        {
            get { return _counter >> 2; }
        }

        public int RealCounter
        {
            get { return _counter; }
        }

        public ProgramCounter()
        {
            _counter = 0;
        }

        public void Advance()
        {
            _counter = _counter + 4;
        }

        public void Jump(JumpData jumpData)
        {
            switch (jumpData.Type)
            {
                case JumpType.Jump:
                    _counter = (int)(_counter & 0xF0000000) | (jumpData.Address << 2);
                    break;
                case JumpType.JumpDirect:
                    _counter = jumpData.Address;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
        }
    }
}
