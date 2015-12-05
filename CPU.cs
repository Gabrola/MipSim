using System.Collections.Generic;
using MipSim.CPUComponents;
using MipSim.Instructions;

namespace MipSim
{
    public class CPU
    {
        private readonly GenericMemory RegisterFile;
        private readonly GenericMemory DataMemory;
        private readonly ProgramCounter PC;
        private readonly InstructionMemory Instructions;
        private readonly ProcedureStack Stack;

        private int _clockCycle;
        private int _instructionExecution;
        private bool _isStalled;

        private readonly Queue<Instruction> InstructionQueue;
        private readonly HashSet<int> AwaitingRegisters;
        private readonly Dictionary<int, int> ForwardedRegisters;
        private readonly Dictionary<int, int> InstructionExecutionDictionary;
        public readonly List<ExecutionRecordList> ExecutionRecords; 

        public static CPU Instance;

        public CPU()
        {
            RegisterFile = new GenericMemory(16);
            DataMemory = new GenericMemory(16);
            PC = new ProgramCounter();
            Instructions = new InstructionMemory();
            Stack = new ProcedureStack();

            IsReady = false;

            _clockCycle = 0;
            _instructionExecution = 0;
            _isStalled = false;

            InstructionQueue = new Queue<Instruction>();
            AwaitingRegisters = new HashSet<int>();
            ForwardedRegisters = new Dictionary<int, int>();
            InstructionExecutionDictionary = new Dictionary<int, int>();
            ExecutionRecords = new List<ExecutionRecordList>();

            RegisterFile.Write(0, 0);

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
                    Instructions.Add(Parser.ParseInstruction(code[i], i));
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
            Instructions.Add(instruction);
            IsReady = true;
        }

        public void RunClock()
        {
            //Finished running if PC has exceeded instructions and all previous instructions have already finished running (determined by empty instruction queue)
            if(!IsReady || (PC.ArrayCounter >= Instructions.Count && InstructionQueue.Count == 0))
                return;

            AwaitingRegisters.Clear();
            ForwardedRegisters.Clear();

            if (PC.ArrayCounter < Instructions.Count && !_isStalled)
            {
                Instructions[PC.ArrayCounter].Initialize(_clockCycle);
                InstructionQueue.Enqueue(Instructions[PC.ArrayCounter]);
                InstructionExecutionDictionary[Instructions[PC.ArrayCounter].InstructionNumber] = _instructionExecution++;
            }

            Instruction[] instructionQueueArray = InstructionQueue.ToArray();

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
                    AwaitingRegisters.Add(instruction.WriteAwaiting);

                //Add forwarded values
                if (instruction.ForwardedRegister.HasValue)
                    ForwardedRegisters[instruction.WriteAwaiting] = instruction.ForwardedRegister.Value;

                if(instruction.ClearAwaiting)
                    instruction.ClearAwaits();

                if (instruction.JumpData != null && !isJumpTaken)
                {
                    var jumpData = instruction.JumpData;

                    if (jumpData.IsJumpTaken)
                    {
                        PC.Jump(jumpData);
                        isJumpTaken = true;
                        jumpIndex = i;
                    }
                }
            }

            //Discards instructions after jump or branch statement
            if (isJumpTaken)
            {
                InstructionQueue.Clear();

                for(int i = 0; i <= jumpIndex; ++i)
                    InstructionQueue.Enqueue(instructionQueueArray[i]);
            }

            //Dequeue finished instructions
            if (instructionQueueArray[0].RelativeClock == 4)
                InstructionQueue.Dequeue();

            //If no jumps were taken advance program counter by 4
            if (!isJumpTaken && !_isStalled)
                PC.Advance();

            _clockCycle++;
        }

        public void AddExecutionRecord(Instruction instruction)
        {
            switch (instruction.RelativeClock)
            {
                case 0:
                    ExecutionRecords[_clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instruction.GetFetch(), InstructionExecutionDictionary[instruction.InstructionNumber]));
                    break;
                case 1:
                    ExecutionRecords[_clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, instruction.GetDecode(), InstructionExecutionDictionary[instruction.InstructionNumber]));
                    break;
                case 2:
                    ExecutionRecords[_clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, instruction.GetExecute(), InstructionExecutionDictionary[instruction.InstructionNumber]));
                    break;
                case 3:
                    ExecutionRecords[_clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, instruction.GetMem(), InstructionExecutionDictionary[instruction.InstructionNumber]));
                    break;
                case 4:
                    ExecutionRecords[_clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, instruction.GetWriteback(), InstructionExecutionDictionary[instruction.InstructionNumber]));
                    break;
            }
        }

        public int Load(int address)
        {
            return DataMemory.Read(address >> 2);
        }

        public void Store(int address, int value)
        {
            DataMemory.Write(address >> 2, value);
        }

        public int RegRead(int register)
        {
            return RegisterFile.Read(register);
        }

        public void RegWrite(int register, int value)
        {
            RegisterFile.Write(register, value);
        }

        public bool IsRegisterReady(int register)
        {
            return !AwaitingRegisters.Contains(register);
        }

        public bool IsRegisterForwarded(int register)
        {
            return ForwardedRegisters.ContainsKey(register);
        }

        public int GetForwardedRegister(int register)
        {
            return ForwardedRegisters[register];
        }

        public void StackPush(int address)
        {
            Stack.Push(address);
        }

        public int StackPop()
        {
            return Stack.Pop();
        }

        public int StackPeek()
        {
            return Stack.Peek();
        }

        public int GetPC()
        {
            return PC.RealCounter;
        }
    }
}
