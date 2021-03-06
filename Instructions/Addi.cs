namespace MipSim.Instructions
{
    public class Addi : Instruction
    {
        private readonly int _rt;
        private readonly int _rs;
        private readonly int _immediate;

        private int _op1;

        private int _result;

        public Addi(string instr, int instructionNumber, int rt, int rs, int immediate)
            : base(instr, instructionNumber)
        {
            _rt = rt;
            _rs = rs;
            _immediate = immediate;
        }

        public override bool Decode()
        {
            _op1 = CPU.Instance.RegRead(_rs);

            return true;
        }

        public override bool Execute()
        {
            WriteAwaiting = _rt;

            //Some previous instruction has not written value to register yet
            if (!CPU.Instance.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.Instance.IsRegisterForwarded(_rs))
                    _op1 = CPU.Instance.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }
            
            _result = _op1 + _immediate;

            return true;
        }

        public override void MemoryOp()
        {
            //Forwarded data is available only AFTER the execute stage
            ForwardedRegister = _result;
        }

        public override void WriteBack()
        {
            CPU.Instance.RegWrite(_rt, _result);
            ClearAwaiting = true;
        }

        public override string GetExecute()
        {
            return string.Format("Add {0} + {1} = {2}", _op1, _immediate, _result);
        }

        public override string GetMem()
        {
            return "None";
        }

        public override string GetWriteback()
        {
            return string.Format("Register ${0} <= {1}", _rt, _result);
        }

        public override string GetInstructionType()
        {
            return "Addi";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rt = ${0}, rs = ${1}, imm = {2}", _rt, _rs, _immediate);
        }
    }
}
