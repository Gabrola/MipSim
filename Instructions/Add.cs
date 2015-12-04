namespace MipSim.Instructions
{
    public class Add : Instruction
    {
        private readonly int _rd;
        private readonly int _rs;
        private readonly int _rt;

        private int _op1;
        private int _op2;

        private int _result;

        public Add(string instr, int instructionNumber, int rd, int rs, int rt)
            : base(instr, instructionNumber)
        {
            _rd = rd;
            _rs = rs;
            _rt = rt;
        }

        public override void Decode()
        {
            _op1 = CPU.RegRead(_rs);
            _op2 = CPU.RegRead(_rt);
        }

        public override bool Execute()
        {
            WriteAwaiting = _rd;

            //Some previous instruction has not written value to register yet
            if (!CPU.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.IsRegisterForwarded(_rs))
                    _op1 = CPU.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }

            if (!CPU.IsRegisterReady(_rt))
            {
                if (CPU.IsRegisterForwarded(_rt))
                    _op2 = CPU.GetForwardedRegister(_rt);
                else
                    return false; //Stall
            }

            _result = _op1 + _op2;

            return true;
        }

        public override void MemoryOp()
        {
            //Forwarded data is available only AFTER the execute stage
            ForwardedRegister = _result;
        }

        public override void WriteBack()
        {
            CPU.RegWrite(_rd, _result);

            //At this point we have written the value to the register in first half of
            //the clock cycle so it should available from the register file directly
            ClearAwaits();
        }

        public override string GetExecute()
        {
            return string.Format("Add {0} + {1} = {2}", _op1, _op2, _result);
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
            return "Add";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rd = ${0}, rs = ${1}, rt = ${2}", _rd, _rs, _rt);
        }
    }
}
