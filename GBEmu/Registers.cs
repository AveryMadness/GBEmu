namespace GBEmu;

public class Registers
{
    // 8-bit registers
    public byte A { get; set; } = 0x01;
    public byte B { get; set; } = 0x00;
    public byte C { get; set; } = 0x13;
    public byte D { get; set; } = 0x00;
    public byte E { get; set; } = 0xD8;
    public byte H { get; set; } = 0x01;
    public byte L { get; set; } = 0x4D;

    // 8-bit Flags Register (F)
    private byte _f;

    public byte F
    {
        get => (byte)(_f & 0xF0); // The lower 4 bits are always 0 (unused)
        set => _f = (byte)(value & 0xF0); // Mask out unused bits
    }

    // 16-bit register pairs (little-endian behavior)
    public ushort AF
    {
        get => (ushort)((A << 8) | F);
        set
        {
            A = (byte)(value >> 8);
            F = (byte)(value & 0xF0); // Mask out lower 4 bits
        }
    }

    public ushort BC
    {
        get => (ushort)((B << 8) | C);
        set
        {
            B = (byte)(value >> 8);
            C = (byte)(value & 0xFF);
        }
    }

    public ushort DE
    {
        get => (ushort)((D << 8) | E);
        set
        {
            D = (byte)(value >> 8);
            E = (byte)(value & 0xFF);
        }
    }

    public ushort HL
    {
        get => (ushort)((H << 8) | L);
        set
        {
            H = (byte)(value >> 8);
            L = (byte)(value & 0xFF);
        }
    }

    // Special registers
    public ushort SP { get; set; } // Stack Pointer
    public ushort PC { get; set; } // Program Counter

    // Flags (stored in F register)
    public bool ZeroFlag
    {
        get => (F & 0x80) != 0;
        set => F = (byte)(value ? (F | 0x80) : (F & ~0x80));
    }

    public bool SubtractFlag
    {
        get => (F & 0x40) != 0;
        set => F = (byte)(value ? (F | 0x40) : (F & ~0x40));
    }

    public bool HalfCarryFlag
    {
        get => (F & 0x20) != 0;
        set => F = (byte)(value ? (F | 0x20) : (F & ~0x20));
    }

    public bool CarryFlag
    {
        get => (F & 0x10) != 0;
        set => F = (byte)(value ? (F | 0x10) : (F & ~0x10));
    }

    // Constructor: Set default values
    public Registers()
    {
        Reset();
    }

    public void Reset()
    {
        A = 0x01;
        F = 0xB0; // Default: Z = 1, N = 0, H = 1, C = 1
        B = 0x00;
        C = 0x13;
        D = 0x00;
        E = 0xD8;
        H = 0x01;
        L = 0x4D;
        SP = 0xFFFE;
        PC = 0x0100; // Entry point after boot
    }
}
