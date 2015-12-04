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
            //TODO: Konsowa and Hazem
            //Patterns for all possible instructions
            string[] patterns = { 
                "^(add|xor|slt) (\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?(\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?(\\$([0-9]|[1-2][0-9]|3[0-1]))$",
                "^addi (\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?(\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?[0-9]+$",
                "^(sw|lw) (\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?[0-9]+\\((\\$([0-9]|[1-2][0-9]|3[0-1]))\\)$",
                "^(j|jal|jp) (([0-9]*[A-Za-z]*)+|[A-Za-z]+[0-9]*)$",
                "^jr (\\$(([1-2][0-9]|3[0-1])|[0-9]))$",
                "^ble (\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?(\\$([0-9]|[1-2][0-9]|3[0-1])),\\s?(([0-9]*[A-Za-z]*)+|[A-Za-z]+[0-9]*)$",
                "^rp$"
            };

            //Syntax check
            bool validInsruction = false;
            for (int i = 0; i < patterns.Length; i++)
			{
			    var reg = new Regex(patterns[i]);
                if(reg.IsMatch(instruction))
                {
                    validInsruction = true;
                    break;
                }

			}

            string instrName = "";

            if (validInsruction)
	        {
                

                if (instruction.Trim() != "rp")
                    instrName = instruction.Split(' ')[0];
                else
                    instrName = instruction;

                switch (instrName)
	            {
                    
                    case "add":
                        int rd = int.Parse(instruction.Split('$')[1].Split(',')[0].Trim());
                        int rs = int.Parse(instruction.Split(',')[1].Trim('$'));
                        int rt = int.Parse(instruction.Split(',')[2].Trim());
                        Add add = new Add(instruction, instructionNumber, rd, rs, rt);
                        break;
                    case "lw":
                        int imm = int.Parse(instruction.Split(',')[1].Split('(')[0].Trim());
                        rs = int.Parse(instruction.Split(',')[1].Split('(')[1].Trim('$',')'));
                        rt = int.Parse(instruction.Split('$')[1].Split(',')[0].Trim());
                        LW lw = new LW(instruction, instructionNumber, rt, imm, rs);
                        break;
                    case "sw":
                        imm = int.Parse(instruction.Split(',')[1].Split('(')[0].Trim());
                        rs = int.Parse(instruction.Split(',')[1].Split('(')[1].Trim('$',')'));
                        rt = int.Parse(instruction.Split('$')[1].Split(',')[0].Trim());
                        SW sw = new SW(instruction, instructionNumber, rt, imm, rs);
                        break;
                    case "jr":
                        rs = int.Parse(instruction.Split('$')[1].Trim());
                        JR jr = new JR(instruction, instructionNumber, rs);
                        break;
                    case "j":
                        int address = int.Parse(instruction.Split(' ')[1].Trim());
                        J j = new J(instruction,instructionNumber,address);
                        break;
                    case "jp":
                        address = int.Parse(instruction.Split(' ')[1].Trim());
                        JumpProcedure jp = new JumpProcedure(instruction, instructionNumber, address);
                        break;
                    case "rp":
                        ReturnProcedure rp = new ReturnProcedure(instruction, instructionNumber);
                        break;
		            default:
                        throw new ParserException("invalid instruction called " + instrName);
	            }
	        }

            throw new ParserException("invalid instruction called " + instrName);
        }
    }
}
