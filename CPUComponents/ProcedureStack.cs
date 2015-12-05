using System;

namespace MipSim.CPUComponents
{
    class ProcedureStack
    {
        private readonly int[] _stack;

        private int _current;

        public ProcedureStack()
        {
            _stack = new int[4];
            _current = 0;
        }

        public void Push (int address)
        {
            if (_current == 3)
                throw new InvalidOperationException("Stack is full");

            _stack[_current++] = address;
        }

        public int Pop()
        {
            if(_current == 0)
                throw new InvalidOperationException("Stack is empty");

            return _stack[--_current];
        }

        public int Peek()
        {
            if (_current == 0)
                throw new InvalidOperationException("Stack is empty");

            return _stack[_current - 1];
        }
    }
}
