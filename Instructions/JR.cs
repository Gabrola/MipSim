namespace MipSim.Instructions
{
    public class JR : Instruction
    {
        private readonly int _rs;

        public JR (string instr, int instructionNumber, int rs)
            : base (instr, instructionNumber)
        {
            _rs = rs;
            JumpData = new JumpData { Type = JumpType.JumpDirect, IsJumpTaken = false };
        }

        public override bool Decode()
        {
            JumpData.Address = CPU.Instance.RegRead(_rs);

            if (!CPU.Instance.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.Instance.IsRegisterForwarded(_rs))
                    JumpData.Address = CPU.Instance.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }

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
            return "JR";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rs = ${0}", _rs);
        }
    }
}
