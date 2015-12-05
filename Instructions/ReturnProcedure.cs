namespace MipSim.Instructions
{
    public class ReturnProcedure : Instruction
    {
        public ReturnProcedure(string instr, int instructionNumber)
            : base (instr, instructionNumber)
        {
            JumpData = new JumpData { Type = JumpType.JumpDirect, IsJumpTaken = false };
        }

        public override void Decode()
        {
            JumpData.Address = CPU.Instance.StackPop();
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
            return "RP";
        }

        public override string GetDecodeFields()
        {
            return string.Format("");
        }
    }
}
