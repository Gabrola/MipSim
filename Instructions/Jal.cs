namespace MipSim.Instructions
{
    class Jal : Instruction
    {
        private readonly int _address;

        public Jal(string instr, int instructionNumber, int address)
            : base(instr, instructionNumber)
        {
            JumpData = new JumpData { Type = JumpType.Jump, IsJumpTaken = false };
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
            CPU.RegWrite(31, _address<<2);

            //At this point we have written the value to the register in first half of
            //the clock cycle so it should available from the register file directly
            ClearAwaits();
        }

        public override string GetDecode()
        {
            return string.Format("Jal Instruction: address => {0}", _address<<2);
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
            return "Jal";
        }
    }
}
