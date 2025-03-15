namespace GBEmu;

public class MemoryBus
{
    private byte[] ram = new byte[0x10000];  // Full 64KB addressable space
    private Cartridge cartridge;
    private byte[] bootRom;
    private PPU ppu;
    private InputController input;
    private APU apu;
    public bool useBootRom = true;

    public MemoryBus(byte[] bootRom, Cartridge cart, PPU ppu, InputController input, APU apu)
    {
        this.bootRom = bootRom;
        this.cartridge = cart;
        this.ppu = ppu;
        this.input = input;
        this.apu = apu;
    }

    public ushort ReadWord(ushort address)
    {
        ushort word = (ushort)(ReadByte(address) | (ReadByte((ushort)(address + 1)) << 8));

        return word;
    }
    
    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case <= 0x7FFF: // ROM (Fixed and switchable bank)
            {
                if (useBootRom && address < 0x100)
                {
                    return bootRom[address];
                }
                return cartridge.Read(address);
            }

            case >= 0x8000 and <= 0x9FFF: // VRAM
                return ppu.ReadVRAM(address);

            case >= 0xA000 and <= 0xBFFF: // External Cartridge RAM
                return cartridge.ReadRAM(address);

            case >= 0xC000 and <= 0xDFFF: // WRAM
                return ram[address];

            case >= 0xE000 and <= 0xFDFF: // Echo RAM (mirror of WRAM)
                return ram[address - 0x2000];

            case >= 0xFE00 and <= 0xFE9F: // OAM (sprite data)
                return ppu.ReadOAM(address);

            case >= 0xFF00 and <= 0xFF7F: // I/O Registers
                return HandleIORead(address);

            case >= 0xFF80 and <= 0xFFFE: // HRAM
                return ram[address];

            case 0xFFFF: // Interrupt Enable Register
                return ram[address];

            default:
                return 0xFF; // Unmapped memory returns 0xFF
        }
    }

    public void WriteWord(ushort address, ushort value)
    {
        byte high = (byte)((value >> 8) & 0xFF);
        byte low = (byte)(value & 0xFF);
        
        WriteByte(address, low);
        WriteByte((ushort)(address + 1), high);
    }

    public void WriteByte(ushort address, byte value)
    {
        if (address == 0xFF50)
        {
            useBootRom = false;
            return;
        }
        
        switch (address)
        {
            case <= 0x7FFF: // ROM is typically read-only, but may handle memory banking
                cartridge.Write(address, value);
                break;

            case >= 0x8000 and <= 0x9FFF: // VRAM
                ppu.WriteVRAM(address, value);
                break;

            case >= 0xA000 and <= 0xBFFF: // External RAM
                cartridge.WriteRAM(address, value);
                break;

            case >= 0xC000 and <= 0xDFFF: // WRAM
                ram[address] = value;
                break;

            case >= 0xE000 and <= 0xFDFF: // Echo RAM
                ram[address - 0x2000] = value;
                break;

            case >= 0xFE00 and <= 0xFE9F: // OAM
                ppu.WriteOAM(address, value);
                break;

            case >= 0xFF00 and <= 0xFF7F: // I/O Registers
                HandleIOWrite(address, value);
                break;

            case >= 0xFF80 and <= 0xFFFE: // HRAM
                ram[address] = value;
                break;

            case 0xFFFF: // Interrupt Enable Register
                ram[address] = value;
                break;
        }
    }

    private byte HandleIORead(ushort address)
    {
        switch (address)
        {
            case 0xFF00: // Joypad input
                return input.Read();
            
            case >= 0xFF10 and <= 0xFF3F:
                return apu.ReadRegister(address);

            case >= 0xFF40 and <= 0xFF4F: // GPU Registers
                return ppu.ReadRegister(address);

            default:
                return ram[address]; // Default to standard RAM behavior
        }
    }

    private void HandleIOWrite(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF00: // Joypad input
                input.Write(value);
                break;
            
            case >= 0xFF10 and <= 0xFF3F:
                apu.WriteRegister(address, value);
                break;

            case >= 0xFF40 and <= 0xFF4F: // GPU Registers
                ppu.WriteRegister(address, value);
                break;

            default:
                ram[address] = value;
                break;
        }
    }
}