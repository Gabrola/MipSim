using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MipSim;
using MipSim.Instructions;

namespace MipSimTest
{
    [TestClass]
    public class DataHazardsTests
    {
        [TestMethod]
        public void TestLoadUseHazard()
        {
            var cpu = new CPU();
            cpu.Store(0, 100);

            const string instr0 = "lw $2, 0($0)";
            const string instr1 = "add $3, $2, $0";

            cpu.AddInstruction(new LW(instr0, 1, 2, 0, 1));
            cpu.AddInstruction(new Add(instr1, 2, 3, 2, 0));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr0, 0));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "LW: rs = $1, rt = $2, imm = 0", 0));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr1, 1));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "LW Address = 0 + 0 = 0", 0));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Add: rd = $3, rs = $2, rt = $0", 1));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "Memory access result = 100", 0));
            clockCycle++;
            //STALL LOCATION
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $2 <= 100", 0));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Add 100 + 0 = 100", 1));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 1));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $3 <= 100", 1));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }
    }
}
