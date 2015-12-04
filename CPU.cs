using System.Collections.Generic;
using MipSim.CPUComponents;
using MipSim.Instructions;

namespace MipSim
{
    public class CPU
    {
        private static readonly GenericMemory RegisterFile;
        private static readonly GenericMemory DataMemory;
        private static readonly ProgramCounter PC;
        private static readonly InstructionMemory Instructions;
        private static readonly ProcedureStack Stack;

        private static int _clockCycle;
        private static bool _isStalled;

        private static readonly Queue<Instruction> InstructionQueue;
        private static readonly HashSet<int> AwaitingRegisters;
        private static readonly Dictionary<int, int> ForwardedRegisters;

        static CPU()
        {
            RegisterFile = new GenericMemory(16);
            DataMemory = new GenericMemory(16);
            PC = new ProgramCounter();
            Instructions = new InstructionMemory();
            Stack = new ProcedureStack();

            IsReady = false;

            _clockCycle = 0;
            _isStalled = false;

            InstructionQueue = new Queue<Instruction>();
            AwaitingRegisters = new HashSet<int>();
            ForwardedRegisters = new Dictionary<int, int>();

            RegisterFile.Write(0, 0);
        }

        public static bool IsReady { get; private set; }

        public static Dictionary<int, string> ParseCode(string[] code)
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

        public static void AddInstruction(Instruction instruction)
        {
            Instructions.Add(instruction);
            IsReady = true;
        }

        public static void RunClock()
        {
            //Finished running if PC has exceeded instructions and all previous instructions have already finished running (determined by empty instruction queue)
            if(!IsReady || (PC.Counter >= Instructions.Count && InstructionQueue.Count == 0))
                return;

            AwaitingRegisters.Clear();
            ForwardedRegisters.Clear();

            if (PC.Counter < Instructions.Count && !_isStalled)
            {
                Instructions[PC.Counter].Initialize(_clockCycle);
                InstructionQueue.Enqueue(Instructions[PC.Counter]);
            }

            Instruction[] instructionQueueArray = InstructionQueue.ToArray();

            bool isJumpTaken = false;
            int jumpIndex = 0;
            _isStalled = false;

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

                //Mark register as awaiting values
                if (instruction.WriteAwaiting != -1)
                    AwaitingRegisters.Add(instruction.WriteAwaiting);

                //Add forwarded values
                if (instruction.ForwardedRegister.HasValue)
                    ForwardedRegisters[instruction.WriteAwaiting] = instruction.ForwardedRegister.Value;

                if (instruction.JumpData != null && !isJumpTaken)
                {
                    var jumpData = instruction.JumpData;

                    if (jumpData.IsJumpTaken)
                    {
                        if (jumpData.Type == JumpType.Branch)
                            PC.Advance(jumpData.Address);
                        else if (jumpData.Type == JumpType.Jump)
                            PC.Jump(jumpData.Address);

                        isJumpTaken = true;
                        jumpIndex = i;
                    }
                }
            }

            //Discards instructions after jump or branch statement
            if (isJumpTaken)
            {
                InstructionQueue.Clear();

                for(int i = 0; i < jumpIndex; ++i)
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

        public static int Load(int address)
        {
            return DataMemory.Read(address);
        }

        public static void Store(int address, int value)
        {
            DataMemory.Write(address, value);
        }

        public static int RegRead(int register)
        {
            return RegisterFile.Read(register);
        }

        public static void RegWrite(int register, int value)
        {
            RegisterFile.Write(register, value);
        }

        public static bool IsRegisterReady(int register)
        {
            return !AwaitingRegisters.Contains(register);
        }

        public static bool IsRegisterForwarded(int register)
        {
            return ForwardedRegisters.ContainsKey(register);
        }

        public static int GetForwardedRegister(int register)
        {
            return ForwardedRegisters[register];
        }

        public static void StackPush(int address)
        {
            Stack.Push(address);
        }

        public static int StackPop()
        {
            return Stack.Pop();
        }
    }
}
