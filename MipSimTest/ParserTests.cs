using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MipSim;

namespace MipSimTest
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TestParseAdd()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "add $1, $2, $3", Tuple.Create("Add", "rd = $1, rs = $2, rt = $3") },
                { "Add $1,$2,$3", Tuple.Create("Add", "rd = $1, rs = $2, rt = $3") },
            });
        }

        [TestMethod]
        public void TestParseAddi()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "addi $1, $2, 3312", Tuple.Create("Addi", "rd = $1, rs = $2, imm = 3312") },
                { "Addi $1,$2,10000", Tuple.Create("Addi", "rd = $1, rs = $2, imm = 10000") },
            });
        }

        [TestMethod]
        public void TestParseJ()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "j 10", Tuple.Create("J", "imm = 10") },
                { "J 25", Tuple.Create("J", "imm = 25") },
            });
        }

        [TestMethod]
        public void TestParseJal()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "jal 10", Tuple.Create("Jal", "imm = 10") },
                { "JaL 25", Tuple.Create("Jal", "imm = 25") },
            });
        }

        [TestMethod]
        public void TestParseJR()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "jr $1", Tuple.Create("JR", "rs = $1") },
                { "JR $2", Tuple.Create("JR", "rs = $2") },
            });
        }

        [TestMethod]
        public void TestParseJP()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "jp 10", Tuple.Create("JP", "imm = 10") },
                { "Jp 25", Tuple.Create("JP", "imm = 25") },
            });
        }

        [TestMethod]
        public void TestParseLW()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "lw $2, 200($10)", Tuple.Create("LW", "rs = $10, rt = $2, imm = 200") },
                { "Lw $2,200($10)", Tuple.Create("LW", "rs = $10, rt = $2, imm = 200") },
            });
        }

        [TestMethod]
        public void TestParseXor()
        {
            TestParse(new Dictionary<string, Tuple<string, string>>
            {
                { "xor $1, $2, $3", Tuple.Create("Xor", "rd = $1, rs = $2, rt = $3") },
                { "XoR $1,$2,$3", Tuple.Create("Xor", "rd = $1, rs = $2, rt = $3") },
            });
        }

        [TestMethod]
        public void TestParseSyntaxErrors()
        {
            string[] instructions =
            {
                "xnor",
                "add $0, $1 $2",
                "add $0, 10, $2",
                "addi $0, $1, $2",
                "add $0 $1 $2",
                "j $0",
                "add $0, $1, $100",
                "",
            };

            foreach (var instruction in instructions)
            {
                try
                {
                    Parser.ParseInstruction(instruction, 0);

                    Assert.Fail();
                }
                catch (ParserException)
                {
                }
                catch (Exception)
                {
                    Assert.Fail();
                }
            }
        }

        private void TestParse(Dictionary<string, Tuple<string, string>> instructionDictionary)
        {
            foreach (KeyValuePair<string, Tuple<string, string>> instructionPair in instructionDictionary)
            {
                var instruction = Parser.ParseInstruction(instructionPair.Key, 0);

                Assert.AreEqual(instructionPair.Value.Item1, instruction.GetInstructionType());
                Assert.AreEqual(instructionPair.Value.Item2, instruction.GetDecodeFields());
            }
        }
    }
}
