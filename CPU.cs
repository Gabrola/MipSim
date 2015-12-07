using System;
using System.Collections.Generic;
using MipSim.CPUComponents;
using MipSim.Instructions;

namespace MipSim
{
    public class CPU
    {
        private readonly GenericMemory _registerFile;
        private readonly GenericMemory _dataMemory;
        private readonly ProgramCounter _pc;
        private readonly InstructionMemory _instructions;
        private readonly ProcedureStack _stack;

        public int ClockCycle { get; private set; }
        private int _instructionExecution;
        private bool _isStalled;

        private readonly Queue<Instruction> _instructionQueue;
        private readonly HashSet<int> _awaitingRegisters;
        private readonly Dictionary<int, int> _forwardedRegisters;
        public readonly List<ExecutionRecordList> ExecutionRecords;

        public readonly BTB Predictor;

        public static CPU Instance;

        public CPU()
        {
            _registerFile = new GenericMemory(16);
            _dataMemory = new GenericMemory(16);
            _pc = new ProgramCounter();
            _instructions = new InstructionMemory();
            _stack = new ProcedureStack();

            IsReady = false;

            ClockCycle = 0;
            _instructionExecution = 0;
            _isStalled = false;

            _instructionQueue = new Queue<Instruction>();
            _awaitingRegisters = new HashSet<int>();
            _forwardedRegisters = new Dictionary<int, int>();
            ExecutionRecords = new List<ExecutionRecordList>();
            Predictor = new BTB();

            _registerFile.Write(0, 0);

            Instance = this;
        }

        public bool IsReady { get; private set; }

        public Dictionary<int, string> ParseCode(string[] code)
        {
            var errors = new Dictionary<int, string>();

            for(int i = 0; i < code.Length; ++i)
            {
                try
                {
                    _instructions.Add(Parser.ParseInstruction(code[i], i));
                }
                catch (ParserException e)
                {
                    errors.Add(i + 1, e.Message);
                }
            }

            if (errors.Count == 0)
                IsReady = true;

            return errors;
        }

        public void AddInstruction(Instruction instruction)
        {
            _instructions.Add(instruction);
            IsReady = true;
        }

        public bool RunClock()
        {
            //Finished running if PC has exceeded instructions and all previous instructions have already finished running (determined by empty instruction queue)
            if(!IsReady || (_pc.ArrayCounter >= _instructions.Count && _instructionQueue.Count == 0))
                return false;

            _awaitingRegisters.Clear();
            _forwardedRegisters.Clear();

            if (_pc.ArrayCounter < _instructions.Count && !_isStalled)
            {
                var inst = (Instruction) _instructions[_pc.ArrayCounter].Clone();
                inst.Initialize(_instructionExecution++);
                _instructionQueue.Enqueue(inst);
            }

            Instruction[] instructionQueueArray = _instructionQueue.ToArray();

            bool isJumpTaken = false;
            int jumpIndex = 0;
            _isStalled = false;

            ExecutionRecords.Add(new ExecutionRecordList());

            //Run other stages for previous instructions in queue
            for (int i = 0; i < instructionQueueArray.Length; ++i)
            {
                Instruction instruction = instructionQueueArray[i];

                //If stall is needed do not advance any further instructions
                if (!instruction.AdvanceClock())
                {
                    _isStalled = true;
                    break;
                }

                AddExecutionRecord(instruction);

                //Mark register as awaiting values
                if (instruction.WriteAwaiting != -1)
                    _awaitingRegisters.Add(instruction.WriteAwaiting);

                //Add forwarded values
                if (instruction.ForwardedRegister.HasValue)
                    _forwardedRegisters[instruction.WriteAwaiting] = instruction.ForwardedRegister.Value;

                if(instruction.ClearAwaiting)
                    instruction.ClearAwaits();

                if (instruction.JumpData != null && !isJumpTaken)
                {
                    var jumpData = instruction.JumpData;

                    if (jumpData.IsJumpTaken)
                    {
                        _pc.Jump(jumpData);
                        instruction.JumpData.IsJumpTaken = false;
                        isJumpTaken = true;
                        jumpIndex = i;
                    }
                }
            }

            //Discards instructions after jump or branch statement
            if (isJumpTaken)
            {
                _instructionQueue.Clear();

                for(int i = 0; i <= jumpIndex; ++i)
                    _instructionQueue.Enqueue(instructionQueueArray[i]);
            }

            //Dequeue finished instructions
            if (instructionQueueArray[0].RelativeClock == 4)
                _instructionQueue.Dequeue();

            //If no jumps were taken advance program counter by 4
            if (!isJumpTaken && !_isStalled && _pc.ArrayCounter < _instructions.Count)
                _pc.Advance();

            ClockCycle++;

            return true;
        }

        public void AddExecutionRecord(Instruction instruction)
        {
            switch (instruction.RelativeClock)
            {
                case 0:
                    ExecutionRecords[ClockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instruction.GetFetch(), instruction.ExecutionOrder, instruction));
                    break;
                case 1:
                    ExecutionRecords[ClockCycle].Add(new ExecutionRecord(ExecutionType.Decode, instruction.GetDecode(), instruction.ExecutionOrder, instruction));
                    break;
                case 2:
                    ExecutionRecords[ClockCycle].Add(new ExecutionRecord(ExecutionType.Execute, instruction.GetExecute(), instruction.ExecutionOrder, instruction));
                    break;
                case 3:
                    ExecutionRecords[ClockCycle].Add(new ExecutionRecord(ExecutionType.Memory, instruction.GetMem(), instruction.ExecutionOrder, instruction));
                    break;
                case 4:
                    ExecutionRecords[ClockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, instruction.GetWriteback(), instruction.ExecutionOrder, instruction));
                    break;
            }
        }

        public int Load(int address)
        {
            return _dataMemory.Read(address >> 2);
        }

        public void Store(int address, int value)
        {
            _dataMemory.Write(address >> 2, value);
        }

        public int RegRead(int register)
        {
            return _registerFile.Read(register);
        }

        public void RegWrite(int register, int value)
        {
            if(register == 0)
                throw new UnauthorizedAccessException();

            _registerFile.Write(register, value);
        }

        public bool IsRegisterReady(int register)
        {
            return !_awaitingRegisters.Contains(register);
        }

        public bool IsRegisterForwarded(int register)
        {
            return _forwardedRegisters.ContainsKey(register);
        }

        public int GetForwardedRegister(int register)
        {
            return _forwardedRegisters[register];
        }

        public void StackPush(int address)
        {
            _stack.Push(address);
        }

        public int StackPop()
        {
            return _stack.Pop();
        }

        public int StackPeek()
        {
            return _stack.Peek();
        }

        public int GetPC()
        {
            return _pc.RealCounter;
        }

        public int GetArrayPC()
        {
            return _pc.ArrayCounter;
        }
    }
}
