namespace GBEmu;

public class Cartridge
{
    private byte[] rom;
    private byte[] ram;
    private int romBank = 1;
    private int ramBank = 0;
    private bool ramEnabled = false;
    private bool bankingMode = false;
    public bool RtcEnabled = false;
    private CartridgeType type;

    private const int KB = 1024;
    private const int MB = 1024 * KB;

    public bool HasSaveRam()
    {
        return type == CartridgeType.MBC1_RAM_BATTERY || type == CartridgeType.MBC2_BATTERY ||
               type == CartridgeType.ROM_RAM_BATTERY
               || type == CartridgeType.MMM01_RAM_BATTERY || type == CartridgeType.MBC3_TIMER_BATTERY ||
               type == CartridgeType.MBC3_TIMER_RAM_BATTERY
               || type == CartridgeType.MBC3_RAM_BATTERY || type == CartridgeType.MBC4_RAM_BATTERY ||
               type == CartridgeType.MBC5_RAM_BATTERY || type == CartridgeType.MBC5_RUMBLE_RAM_BATTERY
               || type == CartridgeType.HuC1_RAM_BATTERY;
    }

    public void LoadSaveRam(byte[] saveRam)
    {
        Array.Copy(saveRam, ram, saveRam.Length);
    }

    public void SaveRam()
    {
        string title = System.Text.Encoding.ASCII.GetString(rom, 0x0134, 16).TrimEnd('\0');
        File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + $"/{title}.sav", ram);
    }

    public bool HasRam()    
    {
        byte lowerNibble = (byte)((byte)type & 0x0F);
        return lowerNibble == 0x02 || lowerNibble == 0x03 ||
               lowerNibble == 0x08 || lowerNibble == 0x09 ||
               lowerNibble == 0x0C || lowerNibble == 0x0D ||
               lowerNibble == 0x0A || lowerNibble == 0x0B ||
               lowerNibble == 0x0E || lowerNibble == 0x0F;
    }

    public int GetRomSize()
    {
        if (IsMBC0())
        {
            return 32 * KB;
        }

        if (IsMBC1())
        {
            return 2 * MB;
        }

        if (IsMBC2())
        {
            return 512 * KB;
        }

        if (IsMBC3())
        {
            return 2 * MB;
        }

        if (IsMBC5())
        {
            return 8 * MB;
        }

        return 0;
    }
    
    public int GetRamSize()
    {
        if (!HasRam()) return 0;
        
        if (IsMBC0())
        {
            return 8 * KB;
        }

        if (IsMBC1())
        {
            return 32 * KB;
        }

        if (IsMBC2())
        {
            return 512; //512 BYTES of ram, insane
        }

        if (IsMBC3())
        {
            return 32 * KB;
        }

        if (IsMBC5())
        {
            return 128 * KB; //holy greed
        }

        return 0;
    }

    public bool IsMBC0()
    {
        return type == CartridgeType.ROM || type == CartridgeType.ROM_RAM || 
               type == CartridgeType.ROM_RAM_BATTERY;
    }
    
    public bool IsMBC1()
    {
        return type == CartridgeType.MBC1 || type == CartridgeType.MBC1_RAM || type == CartridgeType.MBC1_RAM_BATTERY
               || type == CartridgeType.HuC1_RAM_BATTERY;
    }

    public bool IsMBC2()
    {
        return type == CartridgeType.MBC2 || type == CartridgeType.MBC2_BATTERY;
    }
    
    public bool IsMBC3()
    {
        return type == CartridgeType.MBC3_TIMER_BATTERY || type == CartridgeType.MBC3_TIMER_RAM_BATTERY || 
               type == CartridgeType.MBC3 || type == CartridgeType.MBC3_RAM ||
               type == CartridgeType.MBC3_RAM_BATTERY  || type == CartridgeType.HuC3;
    }
    
    public bool IsMBC5()
    {
        return type == CartridgeType.MBC5 || type == CartridgeType.MBC5_RAM || 
               type ==  CartridgeType.MBC5_RAM_BATTERY|| type == CartridgeType.MBC5_RUMBLE ||
               type == CartridgeType.MBC5_RUMBLE_RAM || type == CartridgeType.MBC5_RUMBLE_RAM_BATTERY;
    }

    public Cartridge(byte[] romData, CartridgeType type)
    {
        this.type = type;

        int romSize = GetRomSize();
        int ramSize = GetRamSize();

        if (romData.Length > romSize)
        {
            //throw new Exception("ROM size is too large for cartridge type!");
        }

        rom = romData;
        ram = new byte[ramSize];
    }

    public byte Read(ushort address)
    {
        if (address <= 0x3FFF) // Fixed ROM bank
            return rom[address];

        if (address < 0x8000) // Switchable ROM bank
        {
            int bankAddress = (romBank * 0x4000) + (address - 0x4000);
            return rom[bankAddress];
        }

        return 0xFF; // Unmapped memory
    }

    public void Write(ushort address, byte value)
    {
        if (IsMBC1())
        {
            if (address < 0x2000) // RAM Enable
                ramEnabled = (value & 0xF) == 0x0A;

            else if (address < 0x4000) // ROM Bank Select
            {
                int bank = value & 0x1F;
                if (bank == 0) bank = 1;
                romBank = (romBank & 0x60) | bank;
            }
            
            else if (address < 0x6000) // RAM/other rom? Bank Select
            {
                if (!bankingMode)
                {
                    romBank = (romBank & 0x1F) | ((value & 0x03) << 5);
                }
                else
                {
                    ramBank = value & 0x03;
                }
            }
            
            else if (address < 0x8000)
                bankingMode = (value & 0x01) == 1;
        }
        else if (IsMBC2())
        {
            if (address < 0x2000)
            {
                if ((address & 0x0100) == 0)
                {
                    ramEnabled = (value & 0x0F) == 0x0A;
                }
            }
            else if (address < 0x4000)
            {
                if ((address & 0x0100) != 0)
                {
                    romBank = value & 0x0F;
                }
            }
        }
        else if (IsMBC3())
        {
            if (address < 0x2000)
            {
                ramEnabled = (value & 0x0F) == 0x0A;
            }
            
            else if (address < 0x4000)
            {
                romBank = value & 0x7F;
                if (romBank == 0) romBank = 1;
            }
            
            else if (address < 0x6000)
            {
                if (value <= 0x03)
                {
                    ramBank = value & 0x03;
                    RtcEnabled = false;
                }
                else if (value >= 0x08 && value <= 0x0C)
                {
                    RtcEnabled = true;
                }
            }
        }
        else if (IsMBC5())
        {
            if (address < 0x2000)
            {
                ramEnabled = (value & 0x0F) == 0x0A;
            }
            else if (address < 0x3000)
            {
                romBank = (romBank & 0x100) | value;
            }
            else if (address < 0x4000)
            {
                romBank = (romBank & 0xFF) | ((value & 0x01) << 8);
            }
            else if (address < 0x6000)
            {
                ramBank = value & 0x0F;
            }
        }
    }

    public byte ReadRAM(ushort address)
    {
        if (!ramEnabled) return 0xFF;

        int ramOffset = (ramBank * 0x2000) + (address - 0xA000);
        return ram[ramOffset];
    }

    public void WriteRAM(ushort address, byte value)
    {
        if (IsMBC2())
        {
            if (!ramEnabled) return;
            ram[address & 0x01FF] = (byte)(value & 0x0F);
        }
        else
        {
            if (!ramEnabled) return;
            int ramOffset = (ramBank * 0x2000) + (address - 0xA000);
            ram[ramOffset] = value;
        }
    }
}

public enum CartridgeType : byte
{
    ROM = 0x00,
    MBC1 = 0x01,
    MBC1_RAM = 0x02,
    MBC1_RAM_BATTERY = 0x03,
    MBC2 = 0x05,
    MBC2_BATTERY = 0x06,
    ROM_RAM = 0x08,
    ROM_RAM_BATTERY = 0x09,
    MMM01 = 0x0B,
    MMM01_RAM = 0x0C,
    MMM01_RAM_BATTERY = 0x0D,
    MBC3_TIMER_BATTERY = 0x0F,
    MBC3_TIMER_RAM_BATTERY = 0x10,
    MBC3 = 0x11,
    MBC3_RAM = 0x12,
    MBC3_RAM_BATTERY = 0x13,
    MBC4 = 0x15,
    MBC4_RAM = 0x16,
    MBC4_RAM_BATTERY = 0x17,
    MBC5 = 0x19,
    MBC5_RAM = 0x1A,
    MBC5_RAM_BATTERY = 0x1B,
    MBC5_RUMBLE = 0x1C,
    MBC5_RUMBLE_RAM = 0x1D,
    MBC5_RUMBLE_RAM_BATTERY = 0x1E,
    POCKET_CAMERA = 0xFC,
    BANDAI_TAMA5 = 0xFD,
    HuC3 = 0xFE,
    HuC1_RAM_BATTERY = 0xFF
}