namespace MipSim.Instructions
{
    public class Slt : Instruction
    {
        private readonly int _rd;
        private readonly int _rs;
        private readonly int _rt;

        private int _op1;
        private int _op2;

        private int _result;

        public Slt(string instr, int instructionNumber, int rd, int rs, int rt)
            : base(instr, instructionNumber)
        {
            _rd = rd;
            _rs = rs;
            _rt = rt;
        }

        public override bool Decode()
        {
            _op1 = CPU.Instance.RegRead(_rs);
            _op2 = CPU.Instance.RegRead(_rt);

            return true;
        }

        public override bool Execute()
        {
            WriteAwaiting = _rd;

            //Some previous instruction has not written value to register yet
            if (!CPU.Instance.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.Instance.IsRegisterForwarded(_rs))
                    _op1 = CPU.Instance.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }

            if (!CPU.Instance.IsRegisterReady(_rt))
            {
                if (CPU.Instance.IsRegisterForwarded(_rt))
                    _op2 = CPU.Instance.GetForwardedRegister(_rt);
                else
                    return false; //Stall
            }

            if (_op1 < _op2)
                _result = 1;
            else _result = 0;

            return true;
        }

        public override void MemoryOp()
        {
            //Forwarded data is available only AFTER the execute stage
            ForwardedRegister = _result;
        }

        public override void WriteBack()
        {
            CPU.Instance.RegWrite(_rd, _result);
            ClearAwaiting = true;
        }

        public override string GetExecute()
        {
            return string.Format("Slt {0} < {1} = {2}", _op1, _op2, _result);
        }

        public override string GetMem()
        {
            return "None";
        }

        public override string GetWriteback()
        {
            return string.Format("Register ${0} <= {1}", _rd, _result);
        }

        public override string GetInstructionType()
        {
            return "Slt";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rd = ${0}, rs = ${1}, rt = ${2}", _rd, _rs, _rt);
        }
    }
}
