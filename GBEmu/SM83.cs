using System.Diagnostics;

namespace GBEmu;

public class SM83
{
    #region Prefix Map
    public static Dictionary<byte, Action> PrefixInstructionMap = new Dictionary<byte, Action>
    {
        {0x00, RLC_B},
        {0x01, RLC_C},
        {0x02, RLC_D},
        {0x03, RLC_E},
        {0x04, RLC_H},
        {0x05, RLC_L},
        {0x06, RLC_vHL},
        {0x07, RLC_A},
        {0x08, RRC_B},
        {0x09, RRC_C},
        {0x0A, RRC_D},
        {0x0B, RRC_E},
        {0x0C, RRC_H},
        {0x0D, RRC_L},
        {0x0E, RRC_vHL},
        {0x0F, RRC_A},
        {0x10, RL_B},
        {0x11, RL_C},
        {0x12, RL_D},
        {0x13, RL_E},
        {0x14, RL_H},
        {0x15, RL_L},
        {0x16, RL_vHL},
        {0x17, RL_A},
        {0x18, RR_B},
        {0x19, RR_C},
        {0x1A, RR_D},
        {0x1B, RR_E},
        {0x1C, RR_H},
        {0x1D, RR_L},
        {0x1E, RR_vHL},
        {0x1F, RR_A},
        {0x20, SLA_B},
        {0x21, SLA_C},
        {0x22, SLA_D},
        {0x23, SLA_E},
        {0x24, SLA_H},
        {0x25, SLA_L},
        {0x26, SLA_vHL},
        {0x27, SLA_A},
        {0x28, SRA_B},
        {0x29, SRA_C},
        {0x2A, SRA_D},
        {0x2B, SRA_E},
        {0x2C, SRA_H},
        {0x2D, SRA_L},
        {0x2E, SRA_vHL},
        {0x2F, SRA_A},
        {0x30, SWAP_B},
        {0x31, SWAP_C},
        {0x32, SWAP_D},
        {0x33, SWAP_E},
        {0x34, SWAP_H},
        {0x35, SWAP_L},
        {0x36, SWAP_vHL},
        {0x37, SWAP_A},
        {0x38, SRL_B},
        {0x39, SRL_C},
        {0x3A, SRL_D},
        {0x3B, SRL_E},
        {0x3C, SRL_H},
        {0x3D, SRL_L},
        {0x3E, SRL_vHL},
        {0x3F, SRL_A},
        {0x40, BIT_0_B},
        {0x41, BIT_0_C},
        {0x42, BIT_0_D},
        {0x43, BIT_0_E},
        {0x44, BIT_0_H},
        {0x45, BIT_0_L},
        {0x46, BIT_0_vHL},
        {0x47, BIT_0_A},
        {0x48, BIT_1_B},
        {0x49, BIT_1_C},
        {0x4A, BIT_1_D},
        {0x4B, BIT_1_E},
        {0x4C, BIT_1_H},
        {0x4D, BIT_1_L},
        {0x4E, BIT_1_vHL},
        {0x4F, BIT_1_A},
        {0x50, BIT_2_B},
        {0x51, BIT_2_C},
        {0x52, BIT_2_D},
        {0x53, BIT_2_E},
        {0x54, BIT_2_H},
        {0x55, BIT_2_L},
        {0x56, BIT_2_vHL},
        {0x57, BIT_2_A},
        {0x58, BIT_3_B},
        {0x59, BIT_3_C},
        {0x5A, BIT_3_D},
        {0x5B, BIT_3_E},
        {0x5C, BIT_3_H},
        {0x5D, BIT_3_L},
        {0x5E, BIT_3_vHL},
        {0x5F, BIT_3_A},
        {0x60, BIT_4_B},
        {0x61, BIT_4_C},
        {0x62, BIT_4_D},
        {0x63, BIT_4_E},
        {0x64, BIT_4_H},
        {0x65, BIT_4_L},
        {0x66, BIT_4_vHL},
        {0x67, BIT_4_A},
        {0x68, BIT_5_B},
        {0x69, BIT_5_C},
        {0x6A, BIT_5_D},
        {0x6B, BIT_5_E},
        {0x6C, BIT_5_H},
        {0x6D, BIT_5_L},
        {0x6E, BIT_5_vHL},
        {0x6F, BIT_5_A},
        {0x70, BIT_6_B},
        {0x71, BIT_6_C},
        {0x72, BIT_6_D},
        {0x73, BIT_6_E},
        {0x74, BIT_6_H},
        {0x75, BIT_6_L},
        {0x76, BIT_6_vHL},
        {0x77, BIT_6_A},
        {0x78, BIT_7_B},
        {0x79, BIT_7_C},
        {0x7A, BIT_7_D},
        {0x7B, BIT_7_E},
        {0x7C, BIT_7_H},
        {0x7D, BIT_7_L},
        {0x7E, BIT_7_vHL},
        {0x7F, BIT_7_A},
        {0x80, RES_0_B},
        {0x81, RES_0_C},
        {0x82, RES_0_D},
        {0x83, RES_0_E},
        {0x84, RES_0_H},
        {0x85, RES_0_L},
        {0x86, RES_0_vHL},
        {0x87, RES_0_A},
        {0x88, RES_1_B},
        {0x89, RES_1_C},
        {0x8A, RES_1_D},
        {0x8B, RES_1_E},
        {0x8C, RES_1_H},
        {0x8D, RES_1_L},
        {0x8E, RES_1_vHL},
        {0x8F, RES_1_A},
        {0x90, RES_2_B},
        {0x91, RES_2_C},
        {0x92, RES_2_D},
        {0x93, RES_2_E},
        {0x94, RES_2_H},
        {0x95, RES_2_L},
        {0x96, RES_2_vHL},
        {0x97, RES_2_A},
        {0x98, RES_3_B},
        {0x99, RES_3_C},
        {0x9A, RES_3_D},
        {0x9B, RES_3_E},
        {0x9C, RES_3_H},
        {0x9D, RES_3_L},
        {0x9E, RES_3_vHL},
        {0x9F, RES_3_A},
        {0xA0, RES_4_B},
        {0xA1, RES_4_C},
        {0xA2, RES_4_D},
        {0xA3, RES_4_E},
        {0xA4, RES_4_H},
        {0xA5, RES_4_L},
        {0xA6, RES_4_vHL},
        {0xA7, RES_4_A},
        {0xA8, RES_5_B},
        {0xA9, RES_5_C},
        {0xAA, RES_5_D},
        {0xAB, RES_5_E},
        {0xAC, RES_5_H},
        {0xAD, RES_5_L},
        {0xAE, RES_5_vHL},
        {0xAF, RES_5_A},
        {0xB0, RES_6_B},
        {0xB1, RES_6_C},
        {0xB2, RES_6_D},
        {0xB3, RES_6_E},
        {0xB4, RES_6_H},
        {0xB5, RES_6_L},
        {0xB6, RES_6_vHL},
        {0xB7, RES_6_A},
        {0xB8, RES_7_B},
        {0xB9, RES_7_C},
        {0xBA, RES_7_D},
        {0xBB, RES_7_E},
        {0xBC, RES_7_H},
        {0xBD, RES_7_L},
        {0xBE, RES_7_vHL},
        {0xBF, RES_7_A},
        {0xC0, SET_0_B},
        {0xC1, SET_0_C},
        {0xC2, SET_0_D},
        {0xC3, SET_0_E},
        {0xC4, SET_0_H},
        {0xC5, SET_0_L},
        {0xC6, SET_0_vHL},
        {0xC7, SET_0_A},
        {0xC8, SET_1_B},
        {0xC9, SET_1_C},
        {0xCA, SET_1_D},
        {0xCB, SET_1_E},
        {0xCC, SET_1_H},
        {0xCD, SET_1_L},
        {0xCE, SET_1_vHL},
        {0xCF, SET_1_A},
        {0xD0, SET_2_B},
        {0xD1, SET_2_C},
        {0xD2, SET_2_D},
        {0xD3, SET_2_E},
        {0xD4, SET_2_H},
        {0xD5, SET_2_L},
        {0xD6, SET_2_vHL},
        {0xD7, SET_2_A},
        {0xD8, SET_3_B},
        {0xD9, SET_3_C},
        {0xDA, SET_3_D},
        {0xDB, SET_3_E},
        {0xDC, SET_3_H},
        {0xDD, SET_3_L},
        {0xDE, SET_3_vHL},
        {0xDF, SET_3_A},
        {0xE0, SET_4_B},
        {0xE1, SET_4_C},
        {0xE2, SET_4_D},
        {0xE3, SET_4_E},
        {0xE4, SET_4_H},
        {0xE5, SET_4_L},
        {0xE6, SET_4_vHL},
        {0xE7, SET_4_A},
        {0xE8, SET_5_B},
        {0xE9, SET_5_C},
        {0xEA, SET_5_D},
        {0xEB, SET_5_E},
        {0xEC, SET_5_H},
        {0xED, SET_5_L},
        {0xEE, SET_5_vHL},
        {0xEF, SET_5_A},
        {0xF0, SET_6_B},
        {0xF1, SET_6_C},
        {0xF2, SET_6_D},
        {0xF3, SET_6_E},
        {0xF4, SET_6_H},
        {0xF5, SET_6_L},
        {0xF6, SET_6_vHL},
        {0xF7, SET_6_A},
        {0xF8, SET_7_B},
        {0xF9, SET_7_C},
        {0xFA, SET_7_D},
        {0xFB, SET_7_E},
        {0xFC, SET_7_H},
        {0xFD, SET_7_L},
        {0xFE, SET_7_vHL},
        {0xFF, SET_7_A}
    };
    #endregion

    #region Instruction Map
    public static Dictionary<byte, Action> InstructionMap = new Dictionary<byte, Action>
    {
        {0x00, NOP},
        {0x01, LD_BC_n16},
        {0x02, LD_vBC_A},
        {0x03, INC_BC},
        {0x04, INC_B},
        {0x05, DEC_B},
        {0x06, LD_B},
        {0x07, RCLA},
        {0x08, LDa16_SP},
        {0x09, ADD_HL_BC},
        {0x0A, LD_A_BC},
        {0x0B, DEC_BC},
        {0x0C, INC_C},
        {0x0D, DEC_C},
        {0x0E, LD_C},
        {0x0F, RRCA},
        {0x10, STOP},
        {0x11, LD_DE},
        {0x12, LD_DE_A},
        {0x13, INC_DE},
        {0x14, INC_D},
        {0x15, DEC_D},
        {0x16, LD_D_n8},
        {0x17, RLA},
        {0x18, JR_e},
        {0x19, ADD_HL_DE},
        {0x1A, LD_A_DE},
        {0x1B, DEC_DE},
        {0x1C, INC_E},
        {0x1D, DEC_E},
        {0x1E, LD_E_n8},
        {0x1F, RRA},
        {0x20, JRNZ_e8},
        {0x21, LDHL_n16},
        {0x22, LDHLINC_A},
        {0x23, INC_HL},
        {0x24, INC_H},
        {0x25, DEC_H},
        {0x26, LD_H_d8},
        {0x27, DAA},
        {0x28, JR_Z_e8},
        {0x29, ADD_HL_HL},
        {0x2A, LDA_HLINC},
        {0x2B, DEC_HL},
        {0x2C, INC_L},
        {0x2D, DEC_L},
        {0x2E, LD_L_n8},
        {0x2F, CPL},
        {0x30, JP_NC_r8},
        {0x31, LDSP_n16},
        {0x32, LDHLDEC_A},
        {0x33, INC_SP},
        {0x34, INC_HLA},
        {0x35, DEC_VHL},
        {0x36, LDHL_n8},
        {0x37, SCF},
        {0x38, JR_C_e8},
        {0x39, ADD_HL_SP},
        {0x3A, LD_A_HLDEC},
        {0x3B, DEC_SP},
        {0x3C, INC_A},
        {0x3D, DEC_A},
        {0x3E, LD_A},
        {0x3F, CCF},
        {0x40, LD_B_B},
        {0x41, LD_B_C},
        {0x42, LD_B_D},
        {0x43, LD_B_E},
        {0x44, LD_B_H},
        {0x45, LD_B_L},
        {0x46, LD_B_HL},
        {0x47, LD_B_A},
        {0x48, LD_C_B},
        {0x49, LD_C_C},
        {0x4A, LD_C_D},
        {0x4B, LD_C_E},
        {0x4C, LD_C_H},
        {0x4D, LD_C_L},
        {0x4E, LD_C_vHL},
        {0x4F, LD_C_A},
        {0x50, LD_D_B},
        {0x51, LD_D_C},
        {0x52, LD_D_D},
        {0x53, LD_D_E},
        {0x54, LD_D_H},
        {0x55, LD_D_L},
        {0x56, LD_D_HL},
        {0x57, LD_D_A},
        {0x58, LD_E_B},
        {0x59, LD_E_C},
        {0x5A, LD_E_D},
        {0x5B, LD_E_E},
        {0x5C, LD_E_H},
        {0x5D, LD_E_L},
        {0x5E, LD_E_HL},
        {0x5F, LD_E_A},
        {0x60, LD_H_B},
        {0x61, LD_H_C},
        {0x62, LD_H_D},
        {0x63, LD_H_E},
        {0x64, LD_H_H},
        {0x65, LD_H_L},
        {0x66, LD_H_vHL},
        {0x67, LD_H_A},
        {0x68, LD_L_B},
        {0x69, LD_L_C},
        {0x6A, LD_L_D},
        {0x6B, LD_L_E},
        {0x6C, LD_L_H},
        {0x6D, LD_L_L},
        {0x6E, LD_L_vHL},
        {0x6F, LD_L_A},
        {0x70, LD_vHL_B},
        {0x71, LD_HL_C},
        {0x72, LD_HL_D},
        {0x73, LD_HL_E},
        {0x74, LD_vHL_H},
        {0x75, LD_vHL_L},
        {0x76, HALT},
        {0x77, LDHL_A},
        {0x78, LD_A_B},
        {0x79, LD_A_C},
        {0x7A, LD_A_D},
        {0x7B, LD_A_E},
        {0x7C, LD_A_H},
        {0x7D, LD_A_L},
        {0x7E, LD_A_HL},
        {0x7F, LD_A_A},
        {0x80, ADD_A_B},
        {0x81, ADD_A_C},
        {0x82, ADD_A_D},
        {0x83, ADD_A_E},
        {0x84, ADD_A_H},
        {0x85, ADD_A_L},
        {0x86, ADD_A_HL},
        {0x87, ADD_A_A},
        {0x88, ADC_A_B},
        {0x89, ADC_A_C},
        {0x8A, ADC_A_D},
        {0x8B, ADC_A_E},
        {0x8C, ADC_A_H},
        {0x8D, ADC_A_L},
        {0x8E, ADC_A_vHL},
        {0x8F, ADC_A_A},
        {0x90, SUB_A_B},
        {0x91, SUB_A_C},
        {0x92, SUB_A_D},
        {0x93, SUB_A_E},
        {0x94, SUB_A_H},
        {0x95, SUB_A_L},
        {0x96, SUB_A_vHL},
        {0x97, SUB_A_A},
        {0x98, SBC_A_B},
        {0x99, SBC_A_C},
        {0x9A, SBC_A_D},
        {0x9B, SBC_A_E},
        {0x9C, SBC_A_H},
        {0x9D, SBC_A_L},
        {0x9E, SBC_A_vHL},
        {0x9F, SBC_A_A},
        {0xA0, AND_A_B},
        {0xA1, AND_A_C},
        {0xA2, AND_A_D},
        {0xA3, AND_A_E},
        {0xA4, AND_A_H},
        {0xA5, AND_A_L},
        {0xA6, AND_A_vHL},
        {0xA7, AND_A},
        {0xA8, XOR_A_B},
        {0xA9, XORA_C},
        {0xAA, XOR_A_D},
        {0xAB, XOR_A_E},
        {0xAC, XOR_A_H},
        {0xAD, XOR_A_L},
        {0xAE, XOR_vHL},
        {0xAF, XORA_A},
        {0xB0, OR_A_B},
        {0xB1, OR_A_C},
        {0xB2, OR_A_D},
        {0xB3, OR_A_E},
        {0xB4, OR_A_H},
        {0xB5, OR_A_L},
        {0xB6, OR_A_vHL},
        {0xB7, OR_A},
        {0xB8, CP_A_B},
        {0xB9, CP_A_C},
        {0xBA, CP_A_D},
        {0xBB, CP_A_E},
        {0xBC, CP_A_H},
        {0xBD, CP_A_L},
        {0xBE, CPA_HL},
        {0xBF, CP_A_A},
        {0xC0, RETNZ},
        {0xC1, POP_BC},
        {0xC2, JP_NZ},
        {0xC3, JP_nn},
        {0xC4, CALL_NZ},
        {0xC5, PUSH_BC},
        {0xC6, ADD_A_n8},
        {0xC7, RST_00},
        {0xC8, RETZ},
        {0xC9, RET},
        {0xCA, JP_Z},
        {0xCB, PREFIX},
        {0xCC, CALL_Z},
        {0xCD, CALL},
        {0xCE, ADC_A_n8},
        {0xCF, RST_08},
        {0xD0, RETNC},
        {0xD1, POP_DE},
        {0xD2, JP_NC_a16},
        {0xD4, CALL_NC},
        {0xD5, PUSH_DE},
        {0xD6, SUB_d8},
        {0xD7, RST_10},
        {0xD8, RETC},
        {0xD9, RETI},
        {0xDA, JP_C_a16},
        {0xDC, CALL_C},
        {0xDE, SBC_A_n8},
        {0xDF, RST_18},
        {0xE0, LDHn8_A},
        {0xE1, POP_HL},
        {0xE2, LDHC_A},
        {0xE5, PUSH_HL},
        {0xE6, AND_A_n8},
        {0xE7, RST_20},
        {0xE8, ADD_SP_r8},
        {0xE9, JP_HL},
        {0xEA, LDa16_A},
        {0xEE, XOR_A_d8},
        {0xEF, RST_28},
        {0xF0, LDHA_n8},
        {0xF1, POP_AF},
        {0xF2, LDH_A_vC},
        {0xF3, DI},
        {0xF5, PUSH_AF},
        {0xF6, OR_A_n8},
        {0xF7, RST_30},
        {0xF8, LD_HL_SP_P_r8},
        {0xF9, LD_SP_HL},
        {0xFA, LD_A_a16},
        {0xFB, EI},
        {0xFE, CPA_n8},
        {0xFF, RST_38}
    };

    #endregion


    public static MemoryBus MemoryBus;
    public static ushort ProgramCounter = 0;
    public static int Cycles = 0;
    public static Registers Registers = new Registers();
    public static Stack Stack;
    public static bool IME = false;
    public static ushort[] InterruptVectors = { 0x40, 0x48, 0x50, 0x58, 0x60 };
    public static int WaitingForMasterInterruptChange = -1;
    public static bool NewMasterInterrupt = false;
    public static bool IsHalted = false;
    public static bool WaitForInput = false;

    public static List<string> Callstack = [];

    public static byte IE
    {
        get => MemoryBus.ReadByte(0xFFFF);
        set => MemoryBus.WriteByte(0xFFFF, value);
    }

    [Flags]
    public enum InterruptFlags : byte
    {
        VBlank = 0x01,
        LCDStat = 0x02,
        Timer = 0x04,
        Serial = 0x08,
        Joypad = 0x10
    }

    public static byte IF
    {
        get => MemoryBus.ReadByte(0xFF0F);
        set => MemoryBus.WriteByte(0xFF0F, value);
    }

    public static byte DIV
    {
        get => MemoryBus.ReadByte(0xFF04);
        set => MemoryBus.WriteByte(0xFF04, value);
    }
    
    public static byte TIMA
    {
        get => MemoryBus.ReadByte(0xFF05);
        set => MemoryBus.WriteByte(0xFF05, value);
    }
    
    public static byte TMA
    {
        get => MemoryBus.ReadByte(0xFF06);
        set => MemoryBus.WriteByte(0xFF06, value);
    }
    
    public static byte TAC
    {
        get => MemoryBus.ReadByte(0xFF07);
        set => MemoryBus.WriteByte(0xFF07, value);
    }

    public static void RequestInterrupt(InterruptFlags flag)
    {
        IF |= (byte)flag;
    }

    public static void ClearInterrupt(InterruptFlags flag)
    {
        IF &= (byte)~flag;
    }

    public static void HandleInterrupts()
    {
        if (!IME) return;

        byte pendingInterrupts = (byte)(IF & IE);

        if (pendingInterrupts == 0)
            return;

        IME = false;

        if ((pendingInterrupts & (byte)InterruptFlags.VBlank) != 0)
        {
            ClearInterrupt(InterruptFlags.VBlank);
            CallInterruptHandler(0x0040);
        }
        else if ((pendingInterrupts & (byte)InterruptFlags.LCDStat) != 0)
        {
            ClearInterrupt(InterruptFlags.LCDStat);
            CallInterruptHandler(0x0048);
        }
        else if ((pendingInterrupts & (byte)InterruptFlags.Timer) != 0)
        {
            ClearInterrupt(InterruptFlags.Timer);
            CallInterruptHandler(0x0050);
        }
        else if ((pendingInterrupts & (byte)InterruptFlags.Serial) != 0)
        {
            ClearInterrupt(InterruptFlags.Serial);
            CallInterruptHandler(0x0058);
        }
        else if ((pendingInterrupts & (byte)InterruptFlags.Joypad) != 0)
        {
            ClearInterrupt(InterruptFlags.Joypad);
            CallInterruptHandler(0x0060);
        }
    }

    public static void CallInterruptHandler(ushort address)
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = address;
        Cycles += 20;
    }

    public static async Task ExecuteNextInstruction()
    { 
        if (Program.UseGameboyDoctor)
        {
            byte first = SM83.MemoryBus.ReadByte((ushort)(ProgramCounter));
            byte second = SM83.MemoryBus.ReadByte((ushort)(ProgramCounter + 1));
            byte third = SM83.MemoryBus.ReadByte((ushort)(ProgramCounter + 2));
            byte fourth = SM83.MemoryBus.ReadByte((ushort)(ProgramCounter + 3));
            string s = $"A:{SM83.Registers.A:X2} F:{SM83.Registers.F:X2} B:{SM83.Registers.B:X2} C:{SM83.Registers.C:X2} D:{SM83.Registers.D:X2} E:{SM83.Registers.E:X2} " +
                       $"H:{SM83.Registers.H:X2} L:{SM83.Registers.L:X2} SP:{SM83.Stack.SP:X4} PC:{(ProgramCounter):X4} PCMEM:{first:X2},{second:X2},{third:X2},{fourth:X2}\n";
            File.AppendAllText("gbemu.log", s);
        }
        byte opcode = MemoryBus.ReadByte(ProgramCounter++);
       if (InstructionMap.TryGetValue(opcode, out var instructionFunc))
       {
           int PCCallLoc = ProgramCounter - 1;
           instructionFunc();
           Callstack.Add($"(0x{(PCCallLoc):X8}) Executed Instruction 0x{opcode:X2} ({instructionFunc.Method.Name})");
           
           if (WaitingForMasterInterruptChange > 0)
           {
               WaitingForMasterInterruptChange--;
           }
           else if (WaitingForMasterInterruptChange == 0)
           {
               IME = NewMasterInterrupt;
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

    public static void BIT_7_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 12;
    }
    
    public static void BIT_0_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_0_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_7_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_0_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_0_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_0_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_0_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_0_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 12;
    }
    
    public static void BIT_0_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 0);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_B()
    {
        byte register = Registers.B;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_H()
    {
        byte register = Registers.H;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_2_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 2);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_3_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 3);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_4_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 4);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_5_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 5);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_6_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 6);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_vHL()
    {
        ushort address = Registers.HL;
        byte register = MemoryBus.ReadByte(address);
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_1_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 1);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_7_E()
    {
        byte register = Registers.E;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_7_L()
    {
        byte register = Registers.L;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_7_C()
    {
        byte register = Registers.C;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_7_D()
    {
        byte register = Registers.D;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }
    
    public static void BIT_7_A()
    {
        byte register = Registers.A;
        bool bitSet = IsBitSet(register, 7);
        Registers.ZeroFlag = !bitSet;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 8;
    }

    public static void RLC_B()
    {
        bool bit7 = IsBitSet(Registers.B, 7);
        Registers.B = (byte)((Registers.B << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RLC_C()
    {
        byte regValue = Registers.C;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.C = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RLC_D()
    {
        byte regValue = Registers.D;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.D = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RLC_E()
    {
        byte regValue = Registers.E;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.E = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RLC_H()
    {
        byte regValue = Registers.H;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.H = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RLC_L()
    {
        byte regValue = Registers.L;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.L = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RLC_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        bool bit7 = IsBitSet(regValue, 7);
        byte newVal = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        MemoryBus.WriteByte(address, newVal);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = newVal == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 16;
    }
    
    public static void RLC_A()
    {
        byte regValue = Registers.A;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.A = (byte)((regValue << 1) | (bit7 ? 1 : 0));
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RRC_B()
    {
        byte regValue = Registers.B;
        bool bit0 = IsBitSet(regValue, 0);
        Registers.B = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        bool bit0 = IsBitSet(regValue, 0);
        byte newVal = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        MemoryBus.WriteByte(address, newVal);
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = newVal == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_C()
    {
        byte regValue = Registers.C;
        bool bit0 = IsBitSet(regValue, 0);
        Registers.C = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_D()
    {
        byte regValue = Registers.D;
        bool bit0 = IsBitSet(regValue, 0);
        Registers.D = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_E()
    {
        byte regValue = Registers.E;
        bool bit0 = IsBitSet(regValue, 0);
        Registers.E = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_H()
    {
        byte regValue = Registers.H;
        bool bit0 = IsBitSet(regValue, 0);
        Registers.H = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_L()
    {
        byte regValue = Registers.L;
        bool bit0 = IsBitSet(regValue, 0);
        Registers.L = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RRC_A()
    {
        byte regValue = Registers.A;
        bool bit0 = (regValue & 0x01) != 0;
        Registers.A = (byte)((regValue >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void RL_B()
    {
        byte regValue = Registers.B;
        bool newCarry = (regValue & 0x80) != 0;
        Registers.B = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
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
    
    public static void RL_D()
    {
        byte regValue = Registers.D;
        bool newCarry = (regValue & 0x80) != 0;
        Registers.D = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RL_E()
    {
        byte regValue = Registers.E;
        bool newCarry = (regValue & 0x80) != 0;
        Registers.E = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RL_H()
    {
        byte regValue = Registers.H;
        bool newCarry = (regValue & 0x80) != 0;
        Registers.H = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RL_L()
    {
        byte regValue = Registers.L;
        bool newCarry = (regValue & 0x80) != 0;
        Registers.L = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RL_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        bool newCarry = (regValue & 0x80) != 0;
        byte newValue = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        MemoryBus.WriteByte(address, newValue);
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = newValue == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RL_A()
    {
        byte regValue = Registers.A;
        bool newCarry = (regValue & 0x80) != 0;
        Registers.A = (byte)((regValue << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void RR_B()
    {
        byte regValue = Registers.B;
        bool newCarry = IsBitSet(regValue, 0);
        Registers.B = (byte)((regValue >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }

    public static void RR_C()
    {
        bool newCarry = (Registers.C & 0x01) != 0;
        Registers.C = (byte)((Registers.C >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }
    
    public static void RR_D()
    {
        byte regValue = Registers.D;
        bool newCarry = IsBitSet(regValue, 0);
        Registers.D = (byte)((regValue >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }
    
    public static void RR_E()
    {
        bool newCarry = (Registers.E & 0x01) != 0;
        Registers.E = (byte)((Registers.E >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }
    
    public static void RR_H()
    {
        byte regValue = Registers.H;
        bool newCarry = IsBitSet(regValue, 0);
        Registers.H = (byte)((regValue >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }
    
    public static void RR_L()
    {
        byte regValue = Registers.L;
        bool newCarry = IsBitSet(regValue, 0);
        Registers.L = (byte)((regValue >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }
    
    public static void RR_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        bool newCarry = IsBitSet(regValue, 0);
        byte newValue = (byte)((regValue >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        MemoryBus.WriteByte(address, newValue);
        
        Registers.ZeroFlag = newValue == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }
    
    public static void RR_A()
    {
        byte regValue = Registers.A;
        bool newCarry = IsBitSet(regValue, 0);
        Registers.A = (byte)((regValue >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = newCarry;
        Cycles += 8;
    }

    public static void SLA_B()
    {
        byte regValue = Registers.B;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.B = (byte)(regValue << 1);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SLA_C()
    {
        byte regValue = Registers.C;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.C = (byte)(regValue << 1);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SLA_D()
    {
        byte regValue = Registers.D;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.D = (byte)(regValue << 1);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SLA_E()
    {
        byte regValue = Registers.E;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.E = (byte)(regValue << 1);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SLA_H()
    {
        byte regValue = Registers.H;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.H = (byte)(regValue << 1);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SLA_L()
    {
        byte regValue = Registers.L;
        bool bit7 = IsBitSet(regValue, 7);
        Registers.L = (byte)(regValue << 1);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SLA_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        bool bit7 = IsBitSet(regValue, 7);
        byte newValue = (byte)(regValue << 1);
        MemoryBus.WriteByte(address, newValue);
        Registers.CarryFlag = bit7;
        Registers.ZeroFlag = newValue == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_B()
    {
        byte regValue = Registers.B;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.B = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_C()
    {
        byte regValue = Registers.C;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.C = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_D()
    {
        byte regValue = Registers.D;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.D = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_E()
    {
        byte regValue = Registers.E;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.E = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_H()
    {
        byte regValue = Registers.H;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.H = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_L()
    {
        byte regValue = Registers.L;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.L = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        byte newValue = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        MemoryBus.WriteByte(address, newValue);
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = newValue == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SRA_A()
    {
        byte regValue = Registers.A;
        bool bit0 = IsBitSet(regValue, 0);
        bool bit7 = IsBitSet(regValue, 7);
        Registers.A = (byte)((regValue >> 1) | (bit7 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_B()
    {
        byte regValue = Registers.B;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.B = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.B == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_C()
    {
        byte regValue = Registers.C;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.C = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.C == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_D()
    {
        byte regValue = Registers.D;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.D = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_E()
    {
        byte regValue = Registers.E;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.E = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_H()
    {
        byte regValue = Registers.H;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.H = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_L()
    {
        byte regValue = Registers.L;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.L = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        byte newValue = (byte)((lo << 4) | hi);
        MemoryBus.WriteByte(address, newValue);

        Registers.ZeroFlag = newValue == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }
    
    public static void SWAP_A()
    {
        byte regValue = Registers.A;
        byte hi = (byte)(regValue >> 4);
        byte lo = (byte)(regValue & 0x0F);

        Registers.A = (byte)((lo << 4) | hi);

        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
    }

    public static void RES_0_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 0);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 0);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 0);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 0);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 0);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 0);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 0);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 1);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 1);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 1);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 1);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 1);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 1);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_1_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 1);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 2);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 2);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 2);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 2);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 2);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 2);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_2_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 2);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 3);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 3);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 3);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 3);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 3);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 3);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_3_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 3);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 4);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 4);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 4);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 4);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 4);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 4);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_4_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 4);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 5);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 5);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 5);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 5);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 5);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 5);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_5_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 5);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 6);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 6);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 6);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 6);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 6);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 6);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_6_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 6);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_A()
    {
        byte value = Registers.A;
        byte newValue = ResetBit(value, 7);
        Registers.A = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_B()
    {
        byte value = Registers.B;
        byte newValue = ResetBit(value, 7);
        Registers.B = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_C()
    {
        byte value = Registers.C;
        byte newValue = ResetBit(value, 7);
        Registers.C = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_D()
    {
        byte value = Registers.D;
        byte newValue = ResetBit(value, 7);
        Registers.D = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_E()
    {
        byte value = Registers.E;
        byte newValue = ResetBit(value, 7);
        Registers.E = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_H()
    {
        byte value = Registers.H;
        byte newValue = ResetBit(value, 7);
        Registers.H = newValue;
        Cycles += 8;
    }
    
    public static void RES_7_L()
    {
        byte value = Registers.L;
        byte newValue = ResetBit(value, 7);
        Registers.L = newValue;
        Cycles += 8;
    }
    
    public static void RES_0_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 0);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_1_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 1);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_2_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 2);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_3_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 3);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_4_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 4);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_5_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 5);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_6_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 6);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void RES_7_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte newValue = ResetBit(value, 7);
        MemoryBus.WriteByte(address, newValue);
        Cycles += 8;
    }
    
    public static void SET_0_A()
    {
        Registers.A = SetBit(Registers.A, 0);
    }

    public static void SET_0_B()
    {
        Registers.B = SetBit(Registers.B, 0);
    }
    
    public static void SET_0_C()
    {
        Registers.C = SetBit(Registers.C, 0);
    }
    
    public static void SET_0_D()
    {
        Registers.D = SetBit(Registers.D, 0);
    }
    
    public static void SET_0_E()
    {
        Registers.E = SetBit(Registers.E, 0);
    }
    
    public static void SET_0_H()
    {
        Registers.H = SetBit(Registers.H, 0);
    }
    
    public static void SET_0_L()
    {
        Registers.L = SetBit(Registers.L, 0);
    }
    
    public static void SET_0_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 0));
    }
    
    public static void SET_1_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 1));
    }
    
    public static void SET_2_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 2));
    }
    
    public static void SET_3_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 3));
    }
    
    public static void SET_4_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 4));
    }
    
    public static void SET_5_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 5));
    }
    
    public static void SET_6_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 6));
    }
    
    public static void SET_7_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        MemoryBus.WriteByte(address, SetBit(value, 7));
    }
    
    public static void SET_1_A()
    {
        Registers.A = SetBit(Registers.A, 1);
    }

    public static void SET_1_B()
    {
        Registers.B = SetBit(Registers.B, 1);
    }
    
    public static void SET_1_C()
    {
        Registers.C = SetBit(Registers.C, 1);
    }
    
    public static void SET_1_D()
    {
        Registers.D = SetBit(Registers.D, 1);
    }
    
    public static void SET_1_E()
    {
        Registers.E = SetBit(Registers.E, 1);
    }
    
    public static void SET_1_H()
    {
        Registers.H = SetBit(Registers.H, 1);
    }
    
    public static void SET_1_L()
    {
        Registers.L = SetBit(Registers.L, 1);
    }
    
    public static void SET_2_A()
    {
        Registers.A = SetBit(Registers.A, 2);
    }

    public static void SET_2_B()
    {
        Registers.B = SetBit(Registers.B, 2);
    }
    
    public static void SET_2_C()
    {
        Registers.C = SetBit(Registers.C, 2);
    }
    
    public static void SET_2_D()
    {
        Registers.D = SetBit(Registers.D, 2);
    }
    
    public static void SET_2_E()
    {
        Registers.E = SetBit(Registers.E, 2);
    }
    
    public static void SET_2_H()
    {
        Registers.H = SetBit(Registers.H, 2);
    }
    
    public static void SET_2_L()
    {
        Registers.L = SetBit(Registers.L, 2);
    }
    
    public static void SET_3_A()
    {
        Registers.A = SetBit(Registers.A, 3);
    }

    public static void SET_3_B()
    {
        Registers.B = SetBit(Registers.B, 3);
    }
    
    public static void SET_3_C()
    {
        Registers.C = SetBit(Registers.C, 3);
    }
    
    public static void SET_3_D()
    {
        Registers.D = SetBit(Registers.D, 3);
    }
    
    public static void SET_3_E()
    {
        Registers.E = SetBit(Registers.E, 3);
    }
    
    public static void SET_3_H()
    {
        Registers.H = SetBit(Registers.H, 3);
    }
    
    public static void SET_3_L()
    {
        Registers.L = SetBit(Registers.L, 3);
    }
    
    public static void SET_4_A()
    {
        Registers.A = SetBit(Registers.A, 4);
    }

    public static void SET_4_B()
    {
        Registers.B = SetBit(Registers.B, 4);
    }
    
    public static void SET_4_C()
    {
        Registers.C = SetBit(Registers.C, 4);
    }
    
    public static void SET_4_D()
    {
        Registers.D = SetBit(Registers.D, 4);
    }
    
    public static void SET_4_E()
    {
        Registers.E = SetBit(Registers.E, 4);
    }
    
    public static void SET_4_H()
    {
        Registers.H = SetBit(Registers.H, 4);
    }
    
    public static void SET_4_L()
    {
        Registers.L = SetBit(Registers.L, 4);
    }
    
    public static void SET_5_A()
    {
        Registers.A = SetBit(Registers.A, 5);
    }

    public static void SET_5_B()
    {
        Registers.B = SetBit(Registers.B, 5);
    }
    
    public static void SET_5_C()
    {
        Registers.C = SetBit(Registers.C, 5);
    }
    
    public static void SET_5_D()
    {
        Registers.D = SetBit(Registers.D, 5);
    }
    
    public static void SET_5_E()
    {
        Registers.E = SetBit(Registers.E, 5);
    }
    
    public static void SET_5_H()
    {
        Registers.H = SetBit(Registers.H, 5);
    }
    
    public static void SET_5_L()
    {
        Registers.L = SetBit(Registers.L, 5);
    }
    
    public static void SET_6_A()
    {
        Registers.A = SetBit(Registers.A, 6);
    }

    public static void SET_6_B()
    {
        Registers.B = SetBit(Registers.B, 6);
    }
    
    public static void SET_6_C()
    {
        Registers.C = SetBit(Registers.C, 6);
    }
    
    public static void SET_6_D()
    {
        Registers.D = SetBit(Registers.D, 6);
    }
    
    public static void SET_6_E()
    {
        Registers.E = SetBit(Registers.E, 6);
    }
    
    public static void SET_6_H()
    {
        Registers.H = SetBit(Registers.H, 6);
    }
    
    public static void SET_6_L()
    {
        Registers.L = SetBit(Registers.L, 6);
    }
    
    public static void SET_7_A()
    {
        Registers.A = SetBit(Registers.A, 7);
    }

    public static void SET_7_B()
    {
        Registers.B = SetBit(Registers.B, 7);
    }
    
    public static void SET_7_C()
    {
        Registers.C = SetBit(Registers.C, 7);
    }
    
    public static void SET_7_D()
    {
        Registers.D = SetBit(Registers.D, 7);
    }
    
    public static void SET_7_E()
    {
        Registers.E = SetBit(Registers.E, 7);
    }
    
    public static void SET_7_H()
    {
        Registers.H = SetBit(Registers.H, 7);
    }
    
    public static void SET_7_L()
    {
        Registers.L = SetBit(Registers.L, 7);
    }

    public static void SLA_A()
    {
        Registers.CarryFlag = (Registers.A & 0x80) != 0;
        Registers.A = (byte)(Registers.A << 1);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }

    public static void SRL_B()
    {
        byte regValue = Registers.B;
        Registers.CarryFlag = (regValue & 0x01) != 0;
        Registers.B = (byte)(regValue >> 1);
        Registers.ZeroFlag = Registers.B == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_C()
    {
        byte regValue = Registers.C;
        Registers.CarryFlag = (regValue & 0x01) != 0;
        Registers.C = (byte)(regValue >> 1);
        Registers.ZeroFlag = Registers.C == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_D()
    {
        byte regValue = Registers.D;
        Registers.CarryFlag = (regValue & 0x01) != 0;
        Registers.D = (byte)(regValue >> 1);
        Registers.ZeroFlag = Registers.D == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_E()
    {
        byte regValue = Registers.E;
        Registers.CarryFlag = (regValue & 0x01) != 0;
        Registers.E = (byte)(regValue >> 1);
        Registers.ZeroFlag = Registers.E == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_H()
    {
        byte regValue = Registers.H;
        Registers.CarryFlag = (regValue & 0x01) != 0;
        Registers.H = (byte)(regValue >> 1);
        Registers.ZeroFlag = Registers.H == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_L()
    {
        byte regValue = Registers.L;
        Registers.CarryFlag = (regValue & 0x01) != 0;
        Registers.L = (byte)(regValue >> 1);
        Registers.ZeroFlag = Registers.L == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        Registers.CarryFlag = (regValue & 0x01) != 0;
        byte newValue = (byte)(regValue >> 1);
        MemoryBus.WriteByte(address, newValue);
        Registers.ZeroFlag = newValue == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void SRL_A()
    {
        Registers.CarryFlag = (Registers.A & 0x01) != 0;
        Registers.A = (byte)(Registers.A >> 1);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;
        Cycles += 8;
    }

    public static void PREFIX()
    {
        byte prefixInstruction = ReadByte();

        if (PrefixInstructionMap.TryGetValue(prefixInstruction, out var instructionFunc))
        {
            instructionFunc();
        }
        else
        {
            Console.WriteLine("=== CPU DUMP ===");

            // Print registers
            Console.WriteLine($"AF: 0x{Registers.AF:X4}  BC: 0x{Registers.BC:X4}  DE: 0x{Registers.DE:X4}  HL: 0x{Registers.HL:X4}");
            Console.WriteLine($"SP: 0x{Stack.SP:X4}  PC: 0x{ProgramCounter - 1:X4}");

            // Get current opcode
            Console.WriteLine($"Opcode at PC: 0x{0xCB:X2}");
            Console.WriteLine($"Prefix Opcode: 0x{prefixInstruction:X2}");

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
            throw new NotImplementedException($"PREFIX Opcode 0x{prefixInstruction:X2} not implemented");
        }

        Cycles += 4;
    }

    public static void STOP()
    {
        byte whatever = ReadByte();
        if ((IE & 0x10) != 0)
        {
            IsHalted = true;
            WaitForInput = true;
        }

        Cycles += 4;
    }

    public static void HALT()
    {
        IsHalted = true;
        Cycles += 4;
    }

    public static void RCLA()
    {
        byte bit7 = (byte)((Registers.A & 0x80) >> 7);
        Registers.A = (byte)((Registers.A << 1) | bit7);
        Registers.ZeroFlag = false;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = bit7 == 1;
        Cycles += 4;
    }

    public static void RRCA()
    {
        bool bit0 = IsBitSet(Registers.A, 0);
        Registers.A = (byte)((Registers.A >> 1) | (bit0 ? 0x80 : 0x00));
        Registers.CarryFlag = bit0;
        Registers.ZeroFlag = false;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }

    public static void RRA()
    {
        bool newCarry = (Registers.A & 0x01) != 0;
        Registers.A = (byte)((Registers.A >> 1) | (Registers.CarryFlag ? 0x80 : 0x00));

        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.ZeroFlag = false;
        Registers.CarryFlag = newCarry;
    }

    public static void RLA()
    {
        bool newCarry = (Registers.A & 0x80) != 0;

        Registers.A = (byte)((Registers.A << 1) | (Registers.CarryFlag ? 1 : 0));
        Registers.CarryFlag = newCarry;
        Registers.ZeroFlag = false;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void OR_A_B()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | Registers.B);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }
    
    public static void OR_A_vHL()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);

        Registers.A = (byte)(Registers.A | value);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 8;
    }
    
    public static void OR_A_n8()
    {
        byte value = ReadByte();
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | value);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }

    public static void XOR_A_d8()
    {
        byte value = ReadByte();
        byte oldA = Registers.A;

        Registers.A = (byte)(oldA ^ value);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.CarryFlag = false;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 8;
    }

    public static void XOR_A_L()
    {
        byte value = Registers.L;
        byte oldA = Registers.A;

        Registers.A = (byte)(oldA ^ value);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.CarryFlag = false;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
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

    public static void OR_A()
    {
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 4;
    }
    
    public static void AND_A_B()
    {
        byte regValue = Registers.B;
        Registers.A = (byte)(Registers.A & regValue);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }
    
    public static void AND_A_C()
    {
        Registers.A = (byte)(Registers.A & Registers.C);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }
    
    public static void AND_A_D()
    {
        byte regValue = Registers.D;
        Registers.A = (byte)(Registers.A & regValue);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }
    
    public static void AND_A_E()
    {
        byte regValue = Registers.E;
        Registers.A = (byte)(Registers.A & regValue);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }
    
    public static void AND_A_H()
    {
        byte regValue = Registers.H;
        Registers.A = (byte)(Registers.A & regValue);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }
    
    public static void AND_A_L()
    {
        byte regValue = Registers.L;
        Registers.A = (byte)(Registers.A & regValue);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = true;
        Cycles += 4;
    }
    
    public static void AND_A_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        Registers.A = (byte)(Registers.A & regValue);
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

    public static void ADD_A_n8()
    {
        byte value = ReadByte();
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 8;
    }

    public static void ADC_A_n8()
    {
        byte value = ReadByte();
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_B()
    {
        byte value = Registers.B;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_A()
    {
        byte value = Registers.A;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_C()
    {
        byte value = Registers.C;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_D()
    {
        byte value = Registers.D;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_E()
    {
        byte value = Registers.E;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_H()
    {
        byte value = Registers.H;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }
    
    public static void ADC_A_L()
    {
        byte value = Registers.L;
        int carry = Registers.CarryFlag ? 1 : 0;
        byte oldA = Registers.A;
        int result = Registers.A + value + carry;
        Registers.A = (byte)result;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0F) + (value & 0x0F) + carry) > 0x0F;
        Registers.CarryFlag = (oldA + value + carry) > 0xFF;
        Cycles += 8;
    }

    public static void ADD_SP_r8()
    {
        sbyte value = (sbyte)(ReadByte());

        Registers.HalfCarryFlag = ((Stack.SP & 0x0F) + (value & 0x0F)) > 0x0F;
        Registers.CarryFlag = ((Stack.SP & 0xFF) + (value & 0xFF)) > 0xFF;
        Registers.ZeroFlag = false;
        Registers.SubtractFlag = false;

        Stack.SP = (ushort)(Stack.SP + value);
        Cycles += 16;
    }

    public static void LD_HL_SP_P_r8()
    {
        sbyte value = (sbyte)(ReadByte());

        Registers.HalfCarryFlag = ((Stack.SP & 0x0F) + (value & 0x0F)) > 0x0F;
        Registers.CarryFlag = ((Stack.SP & 0xFF) + (value & 0xFF)) > 0xFF;
        Registers.ZeroFlag = false;
        Registers.SubtractFlag = false;

        Registers.HL = (ushort)(Stack.SP + value);
        Cycles += 12;
    }

    public static void ADD_HL_SP()
    {
        Registers.CarryFlag = (Registers.HL + Stack.SP) > 0xFFFF;
        Registers.HalfCarryFlag = ((Registers.HL & 0x0FFF) + (Stack.SP & 0x0FFF)) > 0x0FFF;

        Registers.HL = (ushort)(Registers.HL + Stack.SP);
        Registers.SubtractFlag = false;
        Cycles += 8;
    }
    
    public static void ADD_HL_BC()
    {
        ushort value = Registers.BC;
        ushort oldA = Registers.HL;
        Registers.HL += value;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add16(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x10000) != 0;
        Cycles += 8;
    }
    
    public static void ADD_HL_HL()
    {
        ushort value = Registers.HL;
        ushort oldA = Registers.HL;
        Registers.HL += value;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = ((oldA & 0x0FFF) + (value & 0x0FFF)) > 0x0FFF;
        Registers.CarryFlag = ((oldA + value) & 0x10000) != 0;
        Cycles += 8;
    }

    public static void ADD_HL_DE()
    {
        ushort value = Registers.DE;
        ushort oldHL = Registers.HL;
        Registers.HL += value;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add16(oldHL, value);
        Registers.CarryFlag = ((oldHL + value) & 0x10000) != 0;
        Cycles += 8;
    }
    
    public static void ADD_A_A()
    {
        byte value = Registers.A;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }
    
    public static void ADD_A_B()
    {
        byte value = Registers.B;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }
    
    public static void ADD_A_C()
    {
        byte value = Registers.C;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }
    
    public static void ADD_A_D()
    {
        byte value = Registers.D;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }
    
    public static void ADD_A_E()
    {
        byte value = Registers.E;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }
    
    public static void ADD_A_H()
    {
        byte value = Registers.H;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }
    
    public static void ADD_A_L()
    {
        byte value = Registers.L;
        byte oldA = Registers.A;
        Registers.A += value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = CheckHalfCarry_Add8(oldA, value);
        Registers.CarryFlag = ((oldA + value) & 0x100) != 0;
        Cycles += 4;
    }

    public static void SBC_A_n8()
    {
        byte value = ReadByte();
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_B()
    {
        byte value = Registers.B;
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_C()
    {
        byte value = Registers.C;
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_D()
    {
        byte value = Registers.D;
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_E()
    {
        byte value = Registers.E;
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_H()
    {
        byte value = Registers.H;
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_L()
    {
        byte value = Registers.L;
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        int carry = Registers.CarryFlag ? 1 : 0;

        int result = Registers.A - value - carry;

        Registers.ZeroFlag = ((byte)result) == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (value & 0x0F) - carry) < 0;
        Registers.CarryFlag = result < 0;

        Registers.A = (byte)result;
        Cycles += 8;
    }
    
    public static void SBC_A_A()
    {
        Registers.ZeroFlag = !Registers.CarryFlag;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = Registers.CarryFlag;

        Registers.A = (byte)(Registers.CarryFlag ? 0xFF : 0x00);
        Cycles += 8;
    }

    public static void SUB_d8()
    {
        byte oldA = Registers.A;
        byte value = ReadByte();
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, value);
        Registers.A -= value;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.CarryFlag = oldA < value;
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
    
    public static void SUB_A_C()
    {
        byte oldA = Registers.A;
        Registers.A -= Registers.C;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, Registers.C);
        Registers.CarryFlag = oldA < Registers.C;
        Cycles += 4;
    }
    
    public static void SUB_A_D()
    {
        byte regValue = Registers.D;
        byte oldA = Registers.A;
        Registers.A -= regValue;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, regValue);
        Registers.CarryFlag = oldA < regValue;
        Cycles += 4;
    }
    
    public static void SUB_A_E()
    {
        byte regValue = Registers.E;
        byte oldA = Registers.A;
        Registers.A -= regValue;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, regValue);
        Registers.CarryFlag = oldA < regValue;
        Cycles += 4;
    }
    
    public static void SUB_A_H()
    {
        byte regValue = Registers.H;
        byte oldA = Registers.A;
        Registers.A -= regValue;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, regValue);
        Registers.CarryFlag = oldA < regValue;
        Cycles += 4;
    }
    
    public static void SUB_A_L()
    {
        byte regValue = Registers.L;
        byte oldA = Registers.A;
        Registers.A -= regValue;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, regValue);
        Registers.CarryFlag = oldA < regValue;
        Cycles += 4;
    }
    
    public static void SUB_A_vHL()
    {
        ushort address = Registers.HL;
        byte regValue = MemoryBus.ReadByte(address);
        byte oldA = Registers.A;
        Registers.A -= regValue;
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(oldA, regValue);
        Registers.CarryFlag = oldA < regValue;
        Cycles += 4;
    }
    
    public static void SUB_A_A()
    {
        Registers.A = 0;
        Registers.ZeroFlag = true;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
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

    public static void CALL_Z()
    {
        ushort address = ReadWord();

        if (Registers.ZeroFlag)
        {
            Stack.Push(ProgramCounter);
            ProgramCounter = address;
            Cycles += 24;
        }
        else
        {
            Cycles += 12;
        }
    }

    public static void CALL_NZ()
    {
        ushort address = ReadWord();

        if (!Registers.ZeroFlag)
        {
            Stack.Push(ProgramCounter);
            ProgramCounter = address;
            Cycles += 24;
        }
        else
        {
            Cycles += 12;
        }
    }
    
    public static void CALL_NC()
    {
        ushort address = ReadWord();

        if (!Registers.CarryFlag)
        {
            Stack.Push(ProgramCounter);
            ProgramCounter = address;
            Cycles += 24;
        }
        else
        {
            Cycles += 12;
        }
    }
    
    public static void CALL_C()
    {
        ushort address = ReadWord();

        if (Registers.CarryFlag)
        {
            Stack.Push(ProgramCounter);
            ProgramCounter = address;
            Cycles += 24;
        }
        else
        {
            Cycles += 12;
        }
    }
    
    public static void RST_00()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0000;
        Cycles += 16;
    }

    public static void RST_08()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0008;
        Cycles += 16;
    }
    
    public static void RST_10()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0010;
        Cycles += 16;
    }
    
    public static void RST_18()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0018;
        Cycles += 16;
    }
    
    public static void RST_20()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0020;
        Cycles += 16;
    }
    
    public static void RST_28()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0028;
        Cycles += 16;
    }
    
    public static void RST_30()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0030;
        Cycles += 16;
    }
    
    public static void RST_38()
    {
        Stack.Push(ProgramCounter);
        ProgramCounter = 0x0038;
        Cycles += 16;
    }

    public static void RETNC()
    {
        if (!Registers.CarryFlag)
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

    public static void RETC()
    {
        if (Registers.CarryFlag)
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

    public static void CP_A_E()
    {
        byte result = (byte)(Registers.A - Registers.E);

        Registers.ZeroFlag = result == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (Registers.E & 0x0F)) < 0;
        Registers.CarryFlag = Registers.A < Registers.E;
        Cycles += 4;
    }
    
    public static void CP_A_H()
    {
        byte regValue = Registers.H;
        byte result = (byte)(Registers.A - regValue);

        Registers.ZeroFlag = result == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (regValue & 0x0F)) < 0;
        Registers.CarryFlag = Registers.A < regValue;
        Cycles += 4;
    }

    public static void CP_A_A()
    {
        Registers.ZeroFlag = true;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 4;
    }
    
    public static void CP_A_L()
    {
        byte regValue = Registers.L;
        byte result = (byte)(Registers.A - regValue);

        Registers.ZeroFlag = result == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (regValue & 0x0F)) < 0;
        Registers.CarryFlag = Registers.A < regValue;
        Cycles += 4;
    }
    
    public static void CP_A_D()
    {
        byte result = (byte)(Registers.A - Registers.D);

        Registers.ZeroFlag = result == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (Registers.D & 0x0F)) < 0;
        Registers.CarryFlag = Registers.A < Registers.D;
        Cycles += 4;
    }
    
    public static void CP_A_C()
    {
        byte result = (byte)(Registers.A - Registers.C);

        Registers.ZeroFlag = result == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (Registers.C & 0x0F)) < 0;
        Registers.CarryFlag = Registers.A < Registers.C;
        Cycles += 4;
    }
    
    public static void CP_A_B()
    {
        byte result = (byte)(Registers.A - Registers.B);

        Registers.ZeroFlag = result == 0;
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = ((Registers.A & 0x0F) - (Registers.B & 0x0F)) < 0;
        Registers.CarryFlag = Registers.A < Registers.B;
        Cycles += 4;
    }

    public static void DAA()
    {
        byte correction = 0;
        bool setCarry = false;

        if (!Registers.SubtractFlag) // After addition
        {
            if (Registers.HalfCarryFlag || (Registers.A & 0x0F) > 9)
            {
                correction |= 0x06;
            }

            if (Registers.CarryFlag || Registers.A > 0x99)  // FIXED: A > 0x99
            {
                correction |= 0x60;
                setCarry = true;
            }

            Registers.A += correction;
        }
        else // After subtraction
        {
            if (Registers.HalfCarryFlag)
            {
                correction |= 0x06;
            }

            if (Registers.CarryFlag) // FIXED: Preserve carry flag
            {
                correction |= 0x60;
                setCarry = true;
            }

            Registers.A -= correction;
        }

        Registers.ZeroFlag = Registers.A == 0;
        Registers.HalfCarryFlag = false; // DAA always clears H flag
        Registers.CarryFlag = setCarry;  // Correctly update carry flag
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
    
    public static void JP_C_a16()
    {
        ushort address = ReadWord();

        if (Registers.CarryFlag)
        {
            ProgramCounter = address;
            Cycles += 16;
        }
        else
        {
            Cycles += 12;
        }
    }

    public static void JP_NC_a16()
    {
        ushort address = ReadWord();

        if (!Registers.CarryFlag)
        {
            ProgramCounter = address;
            Cycles += 16;
        }
        else
        {
            Cycles += 12;
        }
    }

    public static void JP_NC_r8()
    {
        sbyte offset = (sbyte)ReadByte();

        if (!Registers.CarryFlag)
        {
            ProgramCounter = (ushort)(ProgramCounter + offset);
            Cycles += 12;
        }
        else
        {
            Cycles += 8;
        }
    }
    
    public static void JP_Z()
    {
        ushort address = ReadWord();

        if (Registers.ZeroFlag)
        {
            ProgramCounter = address;
            Cycles += 16;
        }
        else
        {
            Cycles += 12;
        }
    }
    
    public static void JP_NZ()
    {
        ushort address = ReadWord();

        if (!Registers.ZeroFlag)
        {
            ProgramCounter = address;
            Cycles += 16;
        }
        else
        {
            Cycles += 12;
        }
    }
    
    public static void JP_HL()
    {
        ushort address = Registers.HL;
        ProgramCounter = address;
        Cycles += 4;
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

    public static void JR_C_e8()
    {
        sbyte offset = (sbyte)ReadByte();
        if (Registers.CarryFlag)
        {
            ProgramCounter = (ushort)(ProgramCounter + offset);
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

    public static void XOR_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        byte regValue = Registers.A;
        Registers.A = (byte)(regValue ^ value);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.CarryFlag = false;
        Cycles += 8;
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
    
    public static void XOR_A_B()
    {
        byte regValue = Registers.B;
        Registers.A = (byte)(Registers.A ^ regValue); 
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void XORA_C()
    {
        Registers.A = (byte)(Registers.A ^ Registers.C);
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void XOR_A_D()
    {
        byte regValue = Registers.D;
        Registers.A = (byte)(Registers.A ^ regValue); 
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void XOR_A_E()
    {
        byte regValue = Registers.E;
        Registers.A = (byte)(Registers.A ^ regValue); 
        Registers.ZeroFlag = Registers.A == 0;
        Registers.SubtractFlag = false;
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void XOR_A_H()
    {
        byte regValue = Registers.H;
        Registers.A = (byte)(Registers.A ^ regValue); 
        Registers.ZeroFlag = Registers.A == 0;
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

    public static void LD_SP_HL()
    {
        Stack.SP = Registers.HL;
        Cycles += 8;
    }
    
    public static void LD_A_HLDEC()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        Registers.A = value;
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
    
    public static void LD_E_A()
    {
        Registers.E = Registers.A;
        Cycles += 4;
    }
    
    public static void LD_E_B()
    {
        Registers.E = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_E_C()
    {
        Registers.E = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_E_D()
    {
        Registers.E = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_E_E()
    {
        Registers.E = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_E_H()
    {
        Registers.E = Registers.H;
        Cycles += 4;
    }
    
    public static void LD_E_L()
    {
        Registers.E = Registers.L;
        Cycles += 4;
    }
    
    public static void LD_D_H()
    {
        Registers.D = Registers.H;
        Cycles += 4;
    }
    
    public static void LD_E_HL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        Registers.E = value;
        Cycles += 8;
    }
    
    public static void LD_D_HL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        Registers.D = value;
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

    public static void LD_H_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        Registers.H = value;
        Cycles += 8;
    }
    
    public static void LD_H_d8()
    {
        byte value = ReadByte();
        Registers.H = value;
        Cycles += 8;
    }
    
    public static void LD_H_D()
    {
        Registers.H = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_H_B()
    {
        Registers.H = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_H_C()
    {
        Registers.H = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_H_E()
    {
        Registers.H = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_H_H()
    {
        Registers.H = Registers.H;
        Cycles += 4;
    }
    
    public static void LD_H_L()
    {
        Registers.H = Registers.L;
        Cycles += 4;
    }

    public static void LD_HL_E()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.E);
        Cycles += 8;
    }
    
    public static void LD_vHL_H()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.H);
        Cycles += 8;
    }
    
    public static void LD_vHL_L()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.L);
        Cycles += 8;
    }

    public static void LD_vHL_B()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.B);
        Cycles += 8;
    }
    
    public static void LD_HL_D()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.D);
        Cycles += 8;
    }
    
    public static void LD_HL_C()
    {
        ushort address = Registers.HL;
        MemoryBus.WriteByte(address, Registers.C);
        Cycles += 8;
    }
    
    public static void LD_B_A()
    {
        Registers.B = Registers.A;
        Cycles += 4;
    }
    
    public static void LD_B_B()
    {
        Registers.B = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_B_C()
    {
        Registers.B = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_B_D()
    {
        Registers.B = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_B_E()
    {
        Registers.B = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_B_H()
    {
        Registers.B = Registers.H;
        Cycles += 4;
    }
    
    public static void LD_B_L()
    {
        Registers.B = Registers.L;
        Cycles += 4;
    }
    
    public static void LD_D_A()
    {
        Registers.D = Registers.A;
        Cycles += 4;
    }
    
    public static void LD_D_B()
    {
        Registers.D = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_D_C()
    {
        Registers.D = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_D_D()
    {
        Registers.D = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_D_E()
    {
        Registers.D = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_D_L()
    {
        Registers.D = Registers.L;
        Cycles += 4;
    }
    
    public static void LD_L_A()
    {
        Registers.L = Registers.A;
        Cycles += 4;
    }
    
    public static void LD_L_vHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        Registers.L = value;
        Cycles += 8;
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
    
    public static void LD_DE_A()
    {
        ushort address = Registers.DE;
        byte value = Registers.A;
        MemoryBus.WriteByte(address, value);
        Cycles += 8;
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

    public static void LD_vBC_A()
    {
        ushort address = Registers.BC;
        MemoryBus.WriteByte(address, Registers.A);
        Cycles += 8;
    }

    public static void LD_A_B()
    {
        Registers.A = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_A_C()
    {
        Registers.A = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_A_D()
    {
        Registers.A = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_L_B()
    {
        Registers.L = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_L_C()
    {
        Registers.L = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_L_D()
    {
        Registers.L = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_L_E()
    {
        Registers.L = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_L_H()
    {
        Registers.L = Registers.H;
        Cycles += 4;
    }
    
    public static void LD_L_L()
    {
        Registers.L = Registers.L;
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
    
    public static void LD_C_B()
    {
        Registers.C = Registers.B;
        Cycles += 4;
    }
    
    public static void LD_C_C()
    {
        Registers.C = Registers.C;
        Cycles += 4;
    }
    
    public static void LD_C_D()
    {
        Registers.C = Registers.D;
        Cycles += 4;
    }
    
    public static void LD_C_E()
    {
        Registers.C = Registers.E;
        Cycles += 4;
    }
    
    public static void LD_C_H()
    {
        Registers.C = Registers.H;
        Cycles += 4;
    }
    
    public static void LD_C_L()
    {
        Registers.C = Registers.L;
        Cycles += 4;
    }

    public static void LD_A_DE()
    {
        byte value = MemoryBus.ReadByte(Registers.DE);
        Registers.A = value;
        Cycles += 8;
    }

    public static void LD_A_HL()
    {
        byte value = MemoryBus.ReadByte(Registers.HL);
        Registers.A = value;
        Cycles += 8;
    }
    
    public static void LD_A_BC()
    {
        byte value = MemoryBus.ReadByte(Registers.BC);
        Registers.A = value;
        Cycles += 8;
    }
    
    public static void LD_B_HL()
    {
        byte value = MemoryBus.ReadByte(Registers.HL);
        Registers.B = value;
        Cycles += 8;
    }
    
    public static void LD_C_vHL()
    {
        byte value = MemoryBus.ReadByte(Registers.HL);
        Registers.C = value;
        Cycles += 8;
    }
    
    public static void LD_A()
    {
        byte value = ReadByte();
        Registers.A = value;
        Cycles += 8;
    }

    public static void LD_A_A()
    {
        Registers.A = Registers.A;
        Cycles += 4;
    }

    public static void LD_A_a16()
    {
        ushort address = ReadWord();
        byte value = MemoryBus.ReadByte(address);
        Registers.A = value;
        Cycles += 16;
    }

    public static void LDa16_SP()
    {
        ushort address = ReadWord();
        MemoryBus.WriteWord(address, Stack.SP);
        Cycles += 20;
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

    public static void LDH_A_vC()
    {
        ushort address = (ushort)(0xFF00 + Registers.C);
        byte value = MemoryBus.ReadByte(address);
        Registers.A = value;
        Cycles += 8;
    }

    public static void INC_HL()
    {
        Registers.HL++;
        Cycles += 8;
    }

    public static void INC_SP()
    {
        Stack.SP++;
        Cycles += 8;
    }
    
    public static void INC_E()
    {
        Registers.HalfCarryFlag = ((Registers.E & 0x0F) == 0x0F);
        Registers.E++;
        Registers.ZeroFlag = Registers.E == 0;
        Registers.SubtractFlag = false;
        Cycles += 4;
    }
    
    public static void INC_D()
    {
        Registers.HalfCarryFlag = ((Registers.D & 0x0F) == 0x0F);
        Registers.D++;
        Registers.ZeroFlag = Registers.D == 0;
        Registers.SubtractFlag = false;
        Cycles += 4;
    }
    
    public static void INC_L()
    {
        Registers.HalfCarryFlag = ((Registers.L & 0x0F) == 0x0F);
        Registers.L++;
        Registers.ZeroFlag = Registers.L == 0;
        Registers.SubtractFlag = false;
        Cycles += 12;
    }
    
    public static void INC_DE()
    {
        Registers.DE++;
        Cycles += 8;
    }

    public static void INC_HLA()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        bool halfCarry = (value & 0x0F) == 0x0F;
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

    public static void DEC_HL()
    {
        Registers.HL--;
        Cycles += 8;
    }

    public static void DEC_DE()
    {
        Registers.DE--;
        Cycles += 8;
    }

    public static void DEC_SP()
    {
        Stack.SP--;
        Cycles += 8;
    }
    
    public static void DEC_H()
    {
        Registers.HalfCarryFlag = CheckHalfCarry_Sub8(Registers.H, 1);
        Registers.H--;
        Cycles += 4;
        Registers.ZeroFlag = Registers.H == 0;
        Registers.SubtractFlag = true;
    }

    public static void DEC_VHL()
    {
        ushort address = Registers.HL;
        byte value = MemoryBus.ReadByte(address);
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        Registers.SubtractFlag = true;
        Registers.HalfCarryFlag = halfCarry;
        value--;
        Registers.ZeroFlag = value == 0;
        MemoryBus.WriteByte(address, value);
        Cycles += 12;
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
    
    public static void DEC_L()
    {
        byte value = Registers.L;
        bool halfCarry = CheckHalfCarry_Sub8(value, 1);
        value -= 1;
        Registers.L = value;
        Registers.ZeroFlag = Registers.L == 0;
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
    
    public static void OR_A_D()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | Registers.D);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }

    public static void OR_A_E()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | Registers.E);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }
    
    public static void OR_A_H()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | Registers.H);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }
    
    public static void OR_A_L()
    {
        Registers.CarryFlag = false;
        Registers.HalfCarryFlag = false;
        Registers.SubtractFlag = false;

        Registers.A = (byte)(Registers.A | Registers.L);
        Registers.ZeroFlag = Registers.A == 0;
        Cycles += 4;
    }

    public static void SCF()
    {
        Registers.CarryFlag = true;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
        Cycles += 4;
    }
    
    public static void CCF()
    {
        Registers.CarryFlag = !Registers.CarryFlag;
        Registers.SubtractFlag = false;
        Registers.HalfCarryFlag = false;
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

    public static byte ResetBit(byte value, int bit)
    {
        return (byte)(value & ~(1 << bit));
    }
    
    public static byte SetBit(byte value, int bit)
    {
        return (byte)(value | (1 << bit));
    }

}