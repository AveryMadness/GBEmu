using System.Diagnostics;

namespace GBEmu;

public class SM83
{

    public static Dictionary<byte, Action> PrefixInstructionMap = new Dictionary<byte, Action>
    {
        {0x11, RL_C},
        {0x7C, BIT_7_H},
        {0x37, SWAP_A}
    };
 
    public static Dictionary<byte, Action> InstructionMap = new Dictionary<byte, Action>
    {
        {0x00, NOP},
        {0x01, LD_BC_n16},
        {0x05, DEC_B},
        {0x0B, DEC_BC},
        {0x0D, DEC_C},
        {0xC3, JP_nn},
        {0x18, JR_e},
        {0x20, JRNZ_e8},
        {0xAF, XORA_A},
        {0x21, LDHL_n16},
        {0x11, LD_DE},
        {0x0E, LD_C},
        {0x06, LD_B},
        {0x32, LDHLDEC_A},
        {0x3E, LD_A},
        {0xE0, LDHn8_A},
        {0xF0, LDHA_n8},
        {0xF3, DI},
        {0xFB, EI},
        {0xFE, CPA_n8},
        {0x36, LDHL_n8},
        {0xEA, LDa16_A},
        {0x31, LDSP_n16},
        {0x2A, LDA_HLINC},
        {0xE2, LDHC_A},
        {0x03, INC_BC},
        {0x0C, INC_C},
        {0xCD, CALL},
        {0x78, LD_A_B},
        {0xB1, OR_A_C},
        {0xC9, RET},
        {0xD9, RETI},
        {0xC5, PUSH_BC},
        {0xD5, PUSH_DE},
        {0xE5, PUSH_HL},
        {0xF5, PUSH_AF},
        {0xA7, AND_A},
        {0x28, JR_Z_e8},
        {0xC0, RETNZ},
        {0xFA, LD_A_a16},
        {0xC8, RETZ},
        {0x3D, DEC_A},
        {0x34, INC_HLA},
        {0x3C, INC_A},
        {0xC1, POP_BC},
        {0xD1, POP_DE},
        {0xE1, POP_HL},
        {0xF1, POP_AF},
        {0x2F, CPL},
        {0xCB, PREFIX},
        {0x77, LDHL_A},
        {0x1A, LD_A_DE},
        {0x4F, LD_C_A},
        {0x17, RLA},
        {0x22, LDHLINC_A},
        {0x23, INC_HL},
        {0x13, INC_DE},
        {0x7B, LD_A_E},
        {0x2E, LD_L_n8},
        {0x67, LD_H_A},
        {0x57, LD_D_A},
        {0x04, INC_B},
        {0x1E, LD_E_n8},
        {0x1D, DEC_E},
        {0x24, INC_H},
        {0x7C, LD_A_H},
        {0x90, SUB_A_B},
        {0x15, DEC_D},
        {0x16, LD_D_n8},
        {0xBE, CPA_HL},
        {0x7D, LD_A_L},
        {0x86, ADD_A_HL},
        {0xE6, AND_A_n8}
    }; 
    
    public static MemoryBus MemoryBus;
    public static ushort ProgramCounter = 0;
    public static int Cycles = 0;
    public static Registers Registers = new Registers();
    public static Stack Stack;
    public static bool IME = false;
    public static ushort[] InterruptVectors = { 0x40, 0x48, 0x50, 0x58, 0x60 };
    public static int WaitingForMasterInterruptChange = -1;
    public static bool NewMasterInterrupt = false;

    public static List<string> Callstack = [];

    public static byte IE
    {
        get => MemoryBus.ReadByte(0xFFFF);
        set => MemoryBus.WriteByte(0xFFFF, value);
    }

    public static byte IF
    {
        get => MemoryBus.ReadByte(0xFF0F);
        set => MemoryBus.WriteByte(0xFF0F, value);
    }

    public static void HandleInterrupts()
    {
        if (!IME) return;

        byte pendingInterrupts = (byte)(IE & IF);

        if (pendingInterrupts == 0) return;

        IME = false;

        for (int i = 0; i < 5; i++)
        {
            if ((pendingInterrupts & (1 << i)) != 0)
            {
                Stack.Push(ProgramCounter);
                ProgramCounter = InterruptVectors[i];

                IF &= (byte)~(1 << i);
                break;
            }
        }
    }

    public async static Task ExecuteNextInstruction()
    {
       byte opcode = MemoryBus.ReadByte(ProgramCounter++);
       if (InstructionMap.TryGetValue(opcode, out var instructionFunc))
       {
           int PCCallLoc = ProgramCounter - 1;
           instructionFunc();
           //AsyncLogger.asyncLogger.Log($"(0x{(PCCallLoc):X8}) Executed Instruction 0x{opcode:X2} ({instructionFunc.Method.Name})");
           Callstack.Add($"(0x{(PCCallLoc):X8}) Executed Instruction 0x{opcode:X2} ({instructionFunc.Method.Name})");
           
           if (WaitingForMasterInterruptChange > 0)
           {
               WaitingForMasterInterruptChange--;
           }
           else if (WaitingForMasterInterruptChange == 0)
           {
               IME = NewMasterInterrupt;
               AsyncLogger.asyncLogger.Log($"Interrupts {(IME ? "Enabled" : "Disabled")}");
               WaitingForMasterInterruptChange--;
           }
       }
       else
       {
           Console.WriteLine("=== CPU DUMP ===");

           // Print registers
           Console.WriteLine($"AF: 0x{Registers.AF:X4}  BC: 0x{Registers.BC:X4}  DE: 0x{Registers.DE:X4}  HL: 0x{Registers.HL:X4}");
           Console.WriteLine($"SP: 0x{Stack.SP:X4}  PC: 0x{ProgramCounter - 1:X4}");

           // Get current opcode
           Console.WriteLine($"Opcode at PC: 0x{opcode:X2}");

           // Print stack dump (next 8 bytes)
           Console.WriteLine("\nStack Dump:");
           for (int i = -1; i < 8; i++)
           {
               ushort addr = (ushort)(Stack.SP - i);
               byte value = MemoryBus.ReadByte(addr);
               Console.WriteLine($"0x{addr:X4} ({(addr == Stack.SP ? "SP" : "SP - " + i)}): 0x{value:X2}");
           }
           
           Console.WriteLine("\nCall Stack:");
           for (int i = Callstack.Count - 6; i < Callstack.Count; i++)
           {
               string call = Callstack[i];
               Console.WriteLine(call);
           }

           Console.WriteLine("=================");
           throw new NotImplementedException($"Opcode 0x{opcode:X2} not implemented");
       }
    }

    public static void BIT_7_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }

    public static void RL_C()
    {
        bool newCarry = (Registers.C & 0x80) != 0;
        Registers.C = (byte)((Registers.C << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }

    public static void SWAP_A()
    {
        byte hi = (byte)(Registers.A >> 4);
        byte lo = (byte)(Registers.A & 0x0F);

        Registers.A = (byte)((lo << 4) | hi);

        if (Registers.A == 0) Registers.ZeroFlag = true;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }

    public static void PREFIX()
    {
        byte prefixInstruction = ReadByte();

        if (PrefixInstructionMap.TryGetValue(prefixInstruction, out var instructionFunc))
        {
            instructionFunc();
            AsyncLogger.asyncLogger.Log($"Executed Prefix instruction {instructionFunc.Method.Name}");
        }
        else
        {
            throw new NotImplementedException($"PREFIX Opcode 0x{prefixInstruction:X2} not implemented");
        }

        Cycles += 4;
    }

    public static void RLA()
    {
        bool newCarry = (Registers.A & 0x80) != 0;

        Registers.A = (byte)((Registers.A << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = false;
        Cycles += 4;
    }

    public static void AND_A()
    {
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }

    public static void AND_A_n8()
    {
        byte value = ReadByte();
        byte regValue = Registers.A;
        Registers.A = (byte)(value & regValue);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Registers.CarryFlag = false;
        Cycles += 8;
    }

    public static void ADD_A_HL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 8;
    }

    public static void SUB_A_B()
    {
        byte oldA = Registers.A;
        Registers.A -= Registers.B;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, Registers.B);
        Registers.CarryFlag = oldA < Registers.B;
        Cycles += 4;
    }

    public static void PUSH_AF()
    {
        Stack.Push(Registers.AF);
        Cycles += 16;
    }
    
    public static void POP_AF()
    {
        Registers.AF = Stack.Pop();
        Registers.F &= 0xF0;  // Clear lower 4 bits of F
        Cycles += 16;
    }
    
    public static void PUSH_BC()
    {
        Stack.Push(Registers.BC);
        Cycles += 16;
    }
    
    public static void POP_BC()
    {
        Registers.BC = Stack.Pop();
        Cycles += 16;
    }
    
    public static void PUSH_DE()
    {
        Stack.Push(Registers.DE);
        Cycles += 16;
    }
    
    public static void POP_DE()
    {
        Registers.DE = Stack.Pop();
        Cycles += 16;
    }
    
    public static void PUSH_HL()
    {
        Stack.Push(Registers.HL);
        Cycles += 16;
    }
    
    public static void POP_HL()
    {
        Registers.HL = Stack.Pop();
        Cycles += 16;
    }

    public static void CALL()
    {
        ushort address = ReadWord();
        Stack.Push(ProgramCounter);
        ProgramCounter = address;
        Cycles += 24;
    }

    public static void RETZ()
    {
        if (Registers.ZeroFlag)
        {
            ushort address = Stack.Pop();
            ProgramCounter = address;
            Cycles += 20;
        }
        else
        {
            Cycles += 8;
        }
    }

    public static void CPL()
    {
        Registers.A = (byte)~Registers.A;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }

    public static void RETNZ()
    {
        if (!Registers.ZeroFlag)
        {
            ushort address = Stack.Pop();
            ProgramCounter = address;
            Cycles += 20;
        }
        else
        {
            Cycles += 8;
        }
    }

    public static void RET()
    {
        ushort address = Stack.Pop();
        ProgramCounter = address;
        Cycles += 16;
    }

    public static void RETI()
    {
        ushort address = Stack.Pop();
        ProgramCounter = address;
        IME = true;
        Cycles += 16;
    }

    public static void DI()
    {
        IME = false;
        Cycles += 4;
    }
    
    public static void EI()
    {
        WaitingForMasterInterruptChange = 1;
        NewMasterInterrupt = true;
        Cycles += 4;
    }

    public static void NOP()
    {
        Cycles += 4;
    }

    public static void CPA_n8()
    {
        byte value = ReadByte();
        byte regValue = Registers.A;
        Registers.ZeroFlag = (regValue == value);
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((regValue & 0xF) < (value & 0xF));
        Registers.CarryFlag = (regValue < value);
        Cycles += 8;
    }

    public static void CPA_HL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte regValue = Registers.A;
        Registers.ZeroFlag = (regValue == value);
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((regValue & 0xF) < (value & 0xF));
        Registers.CarryFlag = (regValue < value);
        Cycles += 8;
    }

    public static void JP_nn()
    {
        ushort address = ReadWord();
        ProgramCounter = address;
        Cycles += 16;
    }

    public static void JR_Z_e8()
    {
        sbyte offset = (sbyte)ReadByte();
        if (Registers.ZeroFlag)
        {
            ProgramCounter += (ushort)offset;
            Cycles += 12;
        }
        else
        {
            Cycles += 8;
        }
    }

    public static void JR_e()
    {
        sbyte offset = (sbyte)ReadByte();
        ProgramCounter = (ushort)(ProgramCounter + offset);
        Cycles += 12;
    }

    public static void JRNZ_e8()
    {
        sbyte offset = (sbyte)ReadByte();

        if (!Registers.ZeroFlag)
        {
            ProgramCounter = (ushort)(ProgramCounter + offset);
            Cycles += 12;
        }
        else
        {
            Cycles += 8;
        }
    }

    public static void XORA_A()
    {
        Registers.A = 0; // Since A ^ A is always 0
        Registers.ZeroFlag = true;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }

    public static void LDHL_n16()
    {
        ushort value = ReadWord();
        Registers.HL = value;
        Cycles += 12;
    }

    public static void LDHL_A()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.A);
        Cycles += 8;
    }

    public static void LDHLINC_A()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.A);
        Registers.HL++;

        Cycles += 8;
    }
    
    public static void LDHLDEC_A()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.A);
        Registers.HL--;

        Cycles += 8;
    }

    public static void LDA_HLINC()
    {
        byte value = MemoryBus.ReadByte(Registers.HL);
        Registers.A = value;
        Registers.HL++;

        Cycles += 8;
    }

    public static void LDHL_n8()
    {
        byte value = ReadByte();
        ushort address = Registers.HL;
        
        MemoryBus.WriteByte(address, value);

        Cycles += 12;
    }

    public static void LDHn8_A()
    {
        byte address = ReadByte();
        ushort fullAddress = (ushort)(0xFF00 + address);
        MemoryBus.WriteByte(fullAddress, Registers.A);
        Cycles += 12;
    }
    
    public static void LDHA_n8()
    {
        byte address = ReadByte();
        ushort fullAddress = (ushort)(0xFF00 + address);
        byte value = MemoryBus.ReadByte(fullAddress);
        Registers.A = value;
        Cycles += 12;
    }

    public static void LDHC_A()
    {
        ushort address = (ushort)(0xFF00 + Registers.C);
        MemoryBus.WriteByte(address, Registers.A);
        Cycles += 8;
    }

    public static void LD_E_n8()
    {
        byte value = ReadByte();
        Registers.E = value;
        Cycles += 8;
    }
    
    public static void LD_D_n8()
    {
        byte value = ReadByte();
        Registers.D = value;
        Cycles += 8;
    }

    public static void LD_C()
    {
        byte value = ReadByte();
        Registers.C = value;
        Cycles += 8;
    }

    public static void LD_H_A()
    {
        Registers.H = Registers.A;
        Cycles += 4;
    }
    
    public static void LD_D_A()
    {
        Registers.D = Registers.A;
        Cycles += 4;
    }
    
    public static void LD_L_n8()
    {
        byte value = ReadByte();
        Registers.L = value;
        Cycles += 8;
    }

    public static void LD_DE()
    {
        ushort value = ReadWord();
        Registers.DE = value;
        Cycles += 12;
    }
    
    public static void LD_B()
    {
        byte value = ReadByte();
        Registers.B = value;
        Cycles += 8;
    }

    public static void LD_BC_n16()
    {
        ushort value = ReadWord();
        Registers.BC = value;
        Cycles += 12;
    }

    public static void LD_A_B()
    {
        Registers.A = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_A_L()
    {
        Registers.A = Registers.L;
        Cycles += 4;
    }

    public static void LD_A_E()
    {
        Registers.A = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_A_H()
    {
        Registers.A = Registers.H;
        Cycles += 4;
    }

    public static void LD_C_A()
    {
        Registers.C = Registers.A;
        Cycles += 4;
    }

    public static void LD_A_DE()
    {
        byte value = MemoryBus.ReadByte(Registers.DE);
        Registers.A = value;
        Cycles += 8;
    }
    
    public static void LD_A()
    {
        byte value = ReadByte();
        Registers.A = value;
        Cycles += 8;
    }

    public static void LD_A_a16()
    {
        ushort address = ReadWord();
        byte value = MemoryBus.ReadByte(address);
        Registers.A = value;
        Cycles += 16;
    }

    public static void LDa16_A()
    {
        ushort address = ReadWord();
        MemoryBus.WriteByte(address, Registers.A);

        Cycles += 16;
    }

    public static void LDSP_n16()
    {
        ushort address = ReadWord();
        Stack.SP = address;
        Cycles += 12;
    }

    public static void INC_HL()
    {
        bool halfCarry = CheckHalfCarry_Add16(Registers.HL, 1);
        Registers.HL++;
        Registers.ZeroFlag = Registers.HL == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = halfCarry;
        Cycles += 12;
    }
    
    public static void INC_DE()
    {
        bool halfCarry = CheckHalfCarry_Add16(Registers.DE, 1);
        Registers.DE++;
        Registers.ZeroFlag = Registers.DE == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = halfCarry;
        Cycles += 12;
    }

    public static void INC_HLA()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        bool halfCarry = CheckHalfCarry_Add16(value, 1);
        value++;
        MemoryBus.WriteByte(address, value);
        Registers.ZeroFlag = value == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = halfCarry;
        Cycles += 12;
    }

    public static void INC_BC()
    {
        Registers.BC++;
        Cycles += 8;
    }
    
    public static void INC_B()
    {
        bool halfCarry = CheckHalfCarry_Add8(Registers.B, 1);

        Registers.B++;

        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = false;
        Registers.ZeroFlag = Registers.B == 0;
        
        Cycles += 4;
    }

    public static void INC_C()
    {
        bool halfCarry = CheckHalfCarry_Add8(Registers.C, 1);

        Registers.C++;

        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = false;
        Registers.ZeroFlag = Registers.C == 0;
        
        Cycles += 4;
    }
    
    public static void INC_H()
    {
        bool halfCarry = CheckHalfCarry_Add8(Registers.H, 1);

        Registers.H++;

        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = false;
        Registers.ZeroFlag = Registers.H == 0;
        
        Cycles += 4;
    }
    
    public static void INC_A()
    {
        bool halfCarry = CheckHalfCarry_Add8(Registers.A, 1);

        Registers.A++;

        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = false;
        Registers.ZeroFlag = Registers.A == 0;
        
        Cycles += 4;
    }

    public static void DEC_BC()
    {
        Registers.BC--;
        Cycles += 8;
    }
    
    public static void DEC_A()
    {
        byte value = Registers.A;
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        value -= 1;
        Registers.A = value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = true;
        Cycles += 4;
    }

    public static void DEC_B()
    {
        byte value = Registers.B;
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        value -= 1;
        Registers.B = value;
        Registers.ZeroFlag = Registers.B == 0;
        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = true;
        Cycles += 4;
    }

    public static void DEC_C()
    {
        byte value = Registers.C;
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        Registers.C = (byte)(Registers.C - 1);
        Registers.ZeroFlag = Registers.C == 0;
        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = true;
        Cycles += 4;
    }
    
    public static void DEC_E()
    {
        byte value = Registers.E;
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        value -= 1;
        Registers.E = value;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = true;
        Cycles += 4;
    }
    
    public static void DEC_D()
    {
        byte value = Registers.D;
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        value -= 1;
        Registers.D = value;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.HalfCarryFlag = halfCarry;
        Registers.SubtractFlag = true;
        Cycles += 4;
    }

    public static void OR_A_C()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | Registers.C);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }
    
    public static ushort ReadWord()
    {
        ushort word = (ushort)(MemoryBus.ReadByte(ProgramCounter) | (MemoryBus.ReadByte((ushort)(ProgramCounter + 1)) << 8));
        ProgramCounter += 2;
        return word;
    }

    public static byte ReadByte()
    {
        byte b = MemoryBus.ReadByte(ProgramCounter);
        ProgramCounter++;
        return b;
    }

    public static bool CheckHalfCarry_Add8(byte a, byte b)
    {
        return (a & 0x0F) + (b & 0x0F) > 0x0F;
    }
    
    public static bool CheckHalfCarry_Add16(ushort value1, ushort value2)
    {
        return ((value1 & 0x0FFF) + (value2 & 0x0FFF)) > 0x0FFF;
    }

    public static bool CheckHalfCarry_Sub8(byte a, byte b)
    {
        return (a & 0x0F) < (b & 0x0F);
    }
    
    public static bool CheckHalfCarry_Sub16(ushort value1, ushort value2)
    {
        return ((value1 & 0x0FFF) < (value2 & 0x0FFF));
    }

    public static bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }

}