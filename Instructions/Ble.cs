namespace MipSim.Instructions
{
    public class Ble : Instruction
    {
        private readonly int _rs;
        private readonly int _rt;
        private readonly int _imm;

        private int _op1;
        private int _op2;

        private bool _predictedTaken;

        public Ble(string instr, int instructionNumber, int rs, int rt, int imm)
            : base(instr, instructionNumber)
        {
            JumpData = new JumpData { Type = JumpType.JumpDirect, IsJumpTaken = false };

            _rs = rs;
            _rt = rt;
            _imm = imm;
        }

        public override void Initialize(int executionOrder)
        {
            base.Initialize(executionOrder);

            _predictedTaken = false;

            int predictedJumpPC;

            if (CPU.Instance.Predictor.PredictBranch(PC, out predictedJumpPC))
            {
                JumpData.IsJumpTaken = true;
                JumpData.Address = predictedJumpPC;
                _predictedTaken = true;
            }
        }

        public override bool Decode()
        {
            JumpData.Address = PC + ((_imm + 1) << 2);

            _op1 = CPU.Instance.RegRead(_rs);
            _op2 = CPU.Instance.RegRead(_rt);

            return true;
        }

        public override bool Execute()
        {
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

            bool branch = _op1 <= _op2;

            CPU.Instance.Predictor.UpdatePrediction(PC, JumpData.Address, branch);

            if (branch == _predictedTaken)
                return true;

            if (branch)
                JumpData.IsJumpTaken = true;
            else
            {
                JumpData.IsJumpTaken = true;
                JumpData.Address = PC + 4;
            }

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
            return string.Format("Ble {0} <= {1} = {2}", _op1, _op2, _op1 <= _op2);
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
            return "Ble";
        }

        public override string GetDecodeFields()
        {
            return string.Format("rs = ${0}, rt = ${1}, imm = {2}", _rs, _rt, _imm);
        }
    }
}
