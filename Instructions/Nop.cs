namespace MipSim.Instructions
{
    public class Nop : Instruction
    {
        public Nop(string instr, int instructionNumber)
            : base(instr, instructionNumber)
        {
        }

        public override void Decode()
        {
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
            return "";
        }

        public override string GetMem()
        {
            return "";
        }

        public override string GetWriteback()
        {
            return "";
        }

        public override string GetInstructionType()
        {
            return "Nop";
        }

        public override string GetDecodeFields()
        {
            return "";
        }
    }
}
