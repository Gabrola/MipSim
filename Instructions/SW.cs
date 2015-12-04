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

        public override void Decode()
        {
            _base = CPU.RegRead(_rs);
            _data = CPU.RegRead(_rt);
        }

        public override bool Execute()
        {
            if (!CPU.IsRegisterReady(_rs))
            {
                //Check if value has been forwarded
                if (CPU.IsRegisterForwarded(_rs))
                    _base = CPU.GetForwardedRegister(_rs);
                else
                    return false; //Else stall
            }

            if (!CPU.IsRegisterReady(_rt))
            {
                //Check if value has been forwarded
                if (CPU.IsRegisterForwarded(_rt))
                    _data = CPU.GetForwardedRegister(_rt);
                else
                    return false; //Else stall
            }

            return true;
        }

        public override void MemoryOp()
        {
            CPU.Store((_base + _offset), _data);
        }

        public override void WriteBack()
        {
        }

        public override string GetDecode()
        {
            return string.Format("SW Instruction: rt => ${0}, offset => {1}, rs => ${2}", _rt, _offset, _rs);
        }

        public override string GetExecute()
        {
            return string.Format("SW Address -> {0} + Offset -> {1}", _base, _offset);
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
