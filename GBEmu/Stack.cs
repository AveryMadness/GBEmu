namespace GBEmu;

public class Stack
{
    public ushort SP { get; set; }
    public MemoryBus memory;

    public Stack(MemoryBus memory)
    {
        this.memory = memory;
        SP = 0xFFFE;
    }
    
    public void Push(ushort value)
    {
        memory.WriteByte(--SP, (byte)(value >> 8));
        memory.WriteByte(--SP, (byte)(value & 0xFF));
        
        Console.WriteLine($"After PUSH: SP = 0x{SP:X4}");
        Console.WriteLine($"Stack at {SP:X4}: 0x{memory.ReadByte(SP):X2}");
        Console.WriteLine($"Stack at {SP+1:X4}: 0x{memory.ReadByte((ushort)(SP+1)):X2}");
    }
    
    public ushort Pop()
    {
        ushort value = memory.ReadWord(SP);
        SP += 2;
        return value;
    }
}