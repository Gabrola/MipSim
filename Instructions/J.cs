namespace MipSim.Instructions
{
    public class J : Instruction
    {
        public J(string instr, int instructionNumber, int address)
            : base(instr, instructionNumber)
        {
            JumpData = new JumpData { Type = JumpType.Jump, Address = address, IsJumpTaken = false };
        }

        public override void Decode()
        {
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

        public override string GetDecodeFields()
        {
            return string.Format("imm = {0}", JumpData.Address);
        }
    }
}
