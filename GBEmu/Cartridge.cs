namespace GBEmu;

public class Cartridge
{
    private byte[] rom;
    private byte[] ram;
    private int romBank = 1;
    private int ramBank = 0;
    private bool ramEnabled = false;
    private CartridgeType type;

    public Cartridge(byte[] romData, CartridgeType type)
    {
        rom = romData;
        ram = new byte[8 * 1024]; // Default 8KB RAM
        this.type = type;
    }

    public byte Read(ushort address)
    {
        if (address <= 0x3FFF) // Fixed ROM bank
            return rom[address];

        if (address >= 0x4000 && address <= 0x7FFF) // Switchable ROM bank
        {
            int bankAddress = (romBank * 0x4000) + (address - 0x4000);
            return rom[bankAddress];
        }

        return 0xFF; // Unmapped memory
    }

    public void Write(ushort address, byte value)
    {
        if (type == CartridgeType.MBC1)
        {
            if (address <= 0x1FFF) // RAM Enable
                ramEnabled = (value & 0xF) == 0xA;

            else if (address >= 0x2000 && address <= 0x3FFF) // ROM Bank Select
                romBank = value & 0x1F;
        }
    }

    public byte ReadRAM(ushort address)
    {
        if (!ramEnabled) return 0xFF;
        return ram[address - 0xA000];
    }

    public void WriteRAM(ushort address, byte value)
    {
        if (!ramEnabled) return;
        ram[address - 0xA000] = value;
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