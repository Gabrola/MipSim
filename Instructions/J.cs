using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipSim.Instructions
{
    class J : Instruction
    {
        int _address;
        public J(string instr, int instructionNumber, int address)
            : base(instr, instructionNumber)
        {
            _address = address;
            JumpData = new JumpData { Type = JumpType.Jump, Address = address, IsJumpTaken = false };
        }

        public override void Decode()
        {
            JumpData.Address = _address;
            JumpData.IsJumpTaken = true;
        }

        public override bool Execute()
        {
            return true;
        }

        public override void MemoryOp()
        {
            
        }

        public override void WriteBack()
        {
            
        }

        public override string GetDecode()
        {
            return string.Format("J Instruction: address => PC = {0}", JumpData.Address);
        }

        public override string GetExecute()
        {
            return "None";
        }

        public override string GetMem()
        {
            return "None";
        }

        public override string GetWriteback()
        {
            return "None";
        }

        public override string GetInstructionType()
        {
            return "J";
        }
    }
}
