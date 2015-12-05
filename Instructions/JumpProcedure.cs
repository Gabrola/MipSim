namespace MipSim.Instructions
{
    public class JumpProcedure : Instruction
    {
        public JumpProcedure(string instr, int instructionNumber, int address)
            : base (instr, instructionNumber)
        {
            JumpData = new JumpData { Type = JumpType.Jump, Address = address, IsJumpTaken = false };
        }

        public override bool Decode()
        {
            CPU.Instance.StackPush(CPU.Instance.GetPC());
            JumpData.IsJumpTaken = true;

            return true;
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
            return "JP";
        }

        public override string GetDecodeFields()
        {
            return string.Format("imm = {0}", JumpData.Address);
        }
    }
}
