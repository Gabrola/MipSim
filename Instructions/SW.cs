namespace MipSim.Instructions
{
    public class SW : Instruction
    {
        private readonly int _rs;
        private readonly int _rt;
        private readonly int _offset;

        private int _base;

        private int _data;  //Data read from register Rt

        public SW(string instr, int instructionNumber, int rt, int offset, int rs) 
            : base(instr, instructionNumber)
        {
            _rs = rs;
            _rt = rt;
            _offset = offset;
        }

        public override bool Decode()
        {
            _base = CPU.Instance.RegRead(_rs);
            _data = CPU.Instance.RegRead(_rt);

            return true;
        }

        public override bool Execute()
        {
            if (!CPU.Instance.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.Instance.IsRegisterForwarded(_rs))
                    _base = CPU.Instance.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }

            if (!CPU.Instance.IsRegisterReady(_rt))
            {
                //Check if value has been forwarded
                if (CPU.Instance.IsRegisterForwarded(_rt))
                    _data = CPU.Instance.GetForwardedRegister(_rt);
                else
                    return false; //Else stall
            }

            return true;
        }

        public override void MemoryOp()
        {
            CPU.Instance.Store((_base + _offset), _data);
        }

        public override void WriteBack()
        {
        }

        public override string GetExecute()
        {
            return string.Format("SW Address = {0} + {1} = {2}", _base, _offset, _base + _offset);
        }

        public override string GetMem()
        {
            return string.Format("Value written in memory = {0}", _data);
        }

        public override string GetWriteback()
        {
            return "None";
        }

        public override string GetInstructionType()
        {
            return "SW";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rs = ${0}, rt = ${1}, imm = {2}", _rs, _rt, _offset);
        }
    }
}
