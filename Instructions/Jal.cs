namespace MipSim.Instructions
{
    public class Jal : Instruction
    {
        private int _returnAddress;

        public Jal(string instr, int instructionNumber, int address)
            : base(instr, instructionNumber)
        {
            JumpData = new JumpData { Type = JumpType.Jump, IsJumpTaken = false, Address = address };
        }

        public override bool Decode()
        {
            JumpData.IsJumpTaken = true;

            _returnAddress = CPU.Instance.GetPC();

            return true;
        }

        public override bool Execute()
        {
            WriteAwaiting = 15;
            ForwardedRegister = _returnAddress;

            return true;
        }

        public override void MemoryOp()
        {
        }

        public override void WriteBack()
        {
            CPU.Instance.RegWrite(15, _returnAddress);
            ClearAwaiting = true;
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
            return string.Format("Register $15 <= {0}", _returnAddress);
        }

        public override string GetInstructionType()
        {
            return "Jal";
        }

        public override string GetDecodeFields()
        {
            return string.Format("imm = {0}", JumpData.Address);
        }
    }
}
