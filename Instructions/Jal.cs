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

        public override void Decode()
        {
            JumpData.IsJumpTaken = true;

            _returnAddress = CPU.Instance.GetPC();
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
            CPU.Instance.RegWrite(15, _returnAddress);

            //At this point we have written the value to the register in first half of
            //the clock cycle so it should available from the register file directly
            ClearAwaits();
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
