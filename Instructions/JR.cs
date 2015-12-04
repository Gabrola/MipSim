namespace MipSim.Instructions
{
    public class JR : Instruction
    {
        private readonly int _rs;

        public JR (string instr, int instructionNumber, int rs)
            : base (instr, instructionNumber)
        {
            _rs = rs;
            JumpData = new JumpData { Type = JumpType.Jump, IsJumpTaken = false };
        }

        public override void Decode()
        {
            JumpData.Address = CPU.RegRead(_rs);
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
            return string.Format("JR Instruction: rs => ${0} = {1}", _rs, JumpData.Address);
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
            return "JR";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rs = ${0}", _rs);
        }
    }
}
