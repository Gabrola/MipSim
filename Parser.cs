using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MipSim.Instructions;
using System.Text.RegularExpressions;

namespace MipSim
{
    public class Parser
    {
        public static Instruction ParseInstruction(string instruction, int instructionNumber)
        {
            //Patterns for all possible instructions
            string[] patterns = { 
                //Using @ before the string disables string character escaping preventing the need to use double \ to escape regex characters
                //Rd is limited 1-15 since we shouldn't be able to write to $0
                @"^(add|xor|slt) \$([1-9]|1[0-5]),\s?\$([0-9]|1[0-5]),\s?\$([0-9]|1[0-5])$",
                @"^addi \$([1-9]|1[0-5]),\s?\$([0-9]|1[0-5]),\s?([0-9]+)$",
                @"^(sw|lw) \$([0-9]|1[0-5]),\s?([0-9]+)\(\$([1-9]|1[0-5])\)$",
                "^(j|jal|jp) ([0-9]+)$",
                @"^jr \$([0-9]|1[0-5])$",
                @"^ble (\$([0-9]|1[0-5])),\s?\$([0-9]|1[0-5]),\s?([0-9]+)$",
                "^rp$"
            };

            instruction = instruction.ToLower().Trim();

            //Syntax check

            for (int i = 0; i < patterns.Length; i++)
			{
			    var reg = new Regex(patterns[i]);
			    var match = reg.Match(instruction);
                if(match.Success)
                {
                    switch (i)
                    {
                        case 0:
                            if(match.Groups[1].Value == "add")
                                return new Add(instruction, instructionNumber, int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
                            if (match.Groups[1].Value == "xor")
                                return new Xor(instruction, instructionNumber, int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
                            if (match.Groups[1].Value == "slt")
                                return new Slt(instruction, instructionNumber, int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
                            break;
                        case 1:
                            return new Addi(instruction, instructionNumber, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
                        case 2:
                            if (match.Groups[1].Value == "sw")
                                return new SW(instruction, instructionNumber, int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
                            if (match.Groups[1].Value == "lw")
                                return new LW(instruction, instructionNumber, int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
                            break;
                        case 3:
                            if (match.Groups[1].Value == "j")
                                return new J(instruction, instructionNumber, int.Parse(match.Groups[2].Value));
                            if (match.Groups[1].Value == "jal")
                                return new Jal(instruction, instructionNumber, int.Parse(match.Groups[2].Value));
                            if (match.Groups[1].Value == "jp")
                                return new JumpProcedure(instruction, instructionNumber, int.Parse(match.Groups[2].Value));
                            break;
                        case 4:
                            return new JR(instruction, instructionNumber, int.Parse(match.Groups[1].Value));
                        case 5:
                            //TODO: Konsowa or Hazem
                            break;
                        case 6:
                            return new ReturnProcedure(instruction, instructionNumber);
                    }

                    break;
                }

			}

            throw new ParserException("Invalid instruction");
        }
    }
}
