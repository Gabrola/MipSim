﻿using System;

namespace MipSim.Instructions
{
    public abstract class Instruction : ICloneable
    {
        protected string InstrString;

        public int RelativeClock { get; private set; }

        public int InstructionNumber { get; private set; }

        public int ExecutionOrder { get; private set; }

        public int PC { get; private set; }

        protected Instruction(string instr, int instructionNumber)
        {
            InstrString = instr;
            InstructionNumber = instructionNumber;
            RelativeClock = -1;
            WriteAwaiting = -1;
            ForwardedRegister = null;
            JumpData = null;
            ClearAwaiting = false;
        }

        public virtual void Initialize(int executionOrder)
        {
            RelativeClock = -1;
            WriteAwaiting = -1;
            ForwardedRegister = null;
            ExecutionOrder = executionOrder;
            if (JumpData != null)
                JumpData.IsJumpTaken = false;
        }

        public virtual void Fetch()
        {
            PC = CPU.Instance.GetPC();
        }

        //Returns false if needs to stall and true otherwise
        public bool AdvanceClock()
        {
            switch (++RelativeClock)
            {
                default:
                    Fetch();
                    return true;

                case 1:
                    if (!Decode())
                    {
                        RelativeClock--;
                        return false;
                    }
                    return true;

                case 2:
                    if (!Execute())
                    {
                        RelativeClock--;
                        return false;
                    }
                    return true;

                case 3:
                    MemoryOp();
                    return true;

                case 4:
                    WriteBack();
                    return true;
            }
        }

        public string GetFetch()
        {
            return InstrString;
        }

        public void ClearAwaits()
        {
            WriteAwaiting = -1;
            ForwardedRegister = null;
            ClearAwaiting = false;
        }

        public abstract bool Decode();

        public abstract bool Execute();

        public abstract void MemoryOp();

        public abstract void WriteBack();

        public string GetDecode()
        {
            return GetInstructionType() + ": " + GetDecodeFields();
        }

        public abstract string GetExecute();

        public abstract string GetMem();

        public abstract string GetWriteback();

        public abstract string GetInstructionType();

        public abstract string GetDecodeFields();

        public JumpData JumpData { get; protected set; }

        public int WriteAwaiting { get; protected set; }

        public int? ForwardedRegister { get; protected set; }

        public bool ClearAwaiting { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
