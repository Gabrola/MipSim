namespace MipSim.Instructions
{
    public class LW : Instruction
    {
        private readonly int _rs;
        private readonly int _rt;
        private readonly int _offset;

        private int _base;

        private int _result;

        public LW(string instr, int instructionNumber, int rt, int offset, int rs) 
            : base(instr, instructionNumber)
        {
            _rs = rs;
            _rt = rt;
            _offset = offset;
        }

        public override void Decode()
        {
            _base = CPU.RegRead(_rs);
        }

        public override bool Execute()
        {
            WriteAwaiting = _rt;

            if (!CPU.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.IsRegisterForwarded(_rs))
                    _base = CPU.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }

            return true;
        }

        public override void MemoryOp()
        {
            _result = CPU.Load(_base + _offset);
        }

        public override void WriteBack()
        {
            ForwardedRegister = _result;

            CPU.RegWrite(_rt, _result);
        }

        public override string GetDecode()
        {
            return string.Format("LW Instruction: rt => ${0}, offset => {1}, rs => ${2}", _rt, _offset, _rs);
        }

        public override string GetExecute()
        {
            return string.Format("LW Address -> {0} + Offset -> {1}", _base, _offset);
        }

        public override string GetMem()
        {
            return string.Format("Memory access result = {0}", _result);
        }

        public override string GetWriteback()
        {
            return string.Format("Register ${0} <= {1}", _rt, _result);
        }

        public override string GetInstructionType()
        {
            return "LW";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rs = ${0}, rt = ${1}, imm = {2}", _rs, _rt, _offset);
        }
    }
}
