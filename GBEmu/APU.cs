namespace GBEmu;

public class APU
{
    #region Sound Registers

    #region Square 1
    private byte NR10; //$FF10
    private byte NR11; //$FF11
    private byte NR12; //$FF12
    private byte NR13; //$FF13
    private byte NR14; //$FF14
    #endregion
    
    #region Square 2
    private byte NR21; //$FF16
    private byte NR22; //$FF17
    private byte NR23; //$FF18
    private byte NR24; //$FF19
    #endregion
    
    #region Wave
    private byte NR30; //$FF1A
    private byte NR31; //$FF1B
    private byte NR32; //$FF1C
    private byte NR33; //$FF1D
    private byte NR34; //$FF1E
    #endregion
    
    #region Noise
    private byte NR41; //$FF20
    private byte NR42; //$FF21
    private byte NR43; //$FF22
    private byte NR44; //$FF23
    #endregion
    
    #region Control/Status
    private byte NR50; //$FF24
    private byte NR51; //$FF25
    private byte NR52; //$FF26
    #endregion
    
    #region Wave pattern RAM

    private byte[] WaveRAM = new byte[16];
    
    #endregion

    #endregion

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF26:
            {
                //Only first bit is R/W, rest are RO
                byte mask = 0b10000000;
                NR52 = (byte)((NR52 & ~mask) | (value & mask));
                break;
            }
            
            //all RW/WO
            case 0xFF25: NR51 = value; break;
            case 0xFF24: NR50 = value; break;
            
            case 0xFF10: NR10 = value; break;
            case 0xFF11: NR11 = value; break;
            case 0xFF12: NR12 = value; break;
            case 0xFF13: NR13 = value; break;
            case 0xFF14: NR14 = value; break;
            
            case 0xFF16: NR21 = value; break;
            case 0xFF17: NR22 = value; break;
            case 0xFF18: NR23 = value; break;
            case 0xFF19: NR24 = value; break;
            
            case 0xFF1A: NR30 = value; break;
            case 0xFF1B: NR31 = value; break;
            case 0xFF1C: NR32 = value; break;
            case 0xFF1D: NR33 = value; break;
            case 0xFF1E: NR34 = value; break;

            case 0xFF20: NR41 = value; break;
            case 0xFF21: NR42 = value; break;
            case 0xFF22: NR43 = value; break;
            case 0xFF23: NR44 = value; break;

            //WAVE RAM!
            case var addr when addr >= 0xFF30 && addr <= 0xFF3F:
            {
                byte index = (byte)(addr - 0xFF30);
                WaveRAM[index] = value;
                break;
            }
            
            default:
                //nothing!
                break;
        }
    }

    public byte ReadRegister(ushort address)
    {
        switch (address)
        {
            case 0xFF26: return NR52;
            case 0xFF25: return NR51;
            case 0xFF24: return NR50;
            
            case 0xFF10: return NR10;
            case 0xFF11:
            {
                byte mask = 0b11000000;
                return (byte)(NR11 & mask);
            }
            case 0xFF12: return NR12;
            case 0xFF13: return NR13;
            case 0xFF14:
            {
                byte mask = 0b01111000;
                return (byte)(NR14 & mask);
            }
            
            case 0xFF16:
            {
                byte mask = 0b11000000;
                return (byte)(NR21 & mask);
            }
            case 0xFF17: return NR22;
            case 0xFF18: return NR23;
            case 0xFF19:
            {
                byte mask = 0b01111000;
                return (byte)(NR24 & mask);
            }
            
            case 0xFF1A: return NR30;
            case 0xFF1B: return 0xFF; //NR31: write only
            case 0xFF1C: return NR32;
            case 0xFF1D: return 0xFF; //NR33: write only
            case 0xFF1E: return NR34;

            case var addr when addr >= 0xFF30 && addr <= 0xFF3F:
            {
                byte index = (byte)(addr - 0xFF30);
                //IMPLEMENT: check if Channel 3 is reading wave ram currently, if so return the proper value
                //return WaveRAM[index]; 
                return 0xFF; //write only otherwise
            }
            
            case 0xFF20: return 0xFF; //NR41: write only
            case 0xFF21: return NR42;
            case 0xFF22: return NR43;
            case 0xFF23:
            {
                byte mask = 0b01111111;
                return (byte)(NR44 & mask);
            }
            
            default:
                return 0xFF;
        }
    }
}