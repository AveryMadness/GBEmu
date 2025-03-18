using NAudio.Wave;

namespace GBEmu;

public class APU
{
    #region Sound Registers

    #region Square 1
    private byte NR10 = 0x80; //$FF10
    private byte NR11 = 0xBF; //$FF11
    private byte NR12 = 0xF3; //$FF12
    private byte NR13 = 0xFF; //$FF13
    private byte NR14 = 0xBF; //$FF14
    #endregion
    
    #region Square 2
    private byte NR21 = 0x3F; //$FF16
    private byte NR22 = 0x00; //$FF17
    private byte NR23 = 0xFF; //$FF18
    private byte NR24 = 0xBF; //$FF19
    #endregion
    
    #region Wave
    private byte NR30 = 0x7F; //$FF1A
    private byte NR31 = 0xFF; //$FF1B
    private byte NR32 = 0x9F; //$FF1C
    private byte NR33 = 0xFF; //$FF1D
    private byte NR34 = 0xBF; //$FF1E
    #endregion
    
    #region Noise
    private byte NR41 = 0xFF; //$FF20
    private byte NR42 = 0x00; //$FF21
    private byte NR43 = 0x00; //$FF22
    private byte NR44 = 0xBF; //$FF23
    #endregion
    
    #region Control/Status
    private byte NR50 = 0x77; //$FF24
    private byte NR51 = 0xF3; //$FF25
    private byte NR52 = 0xF1; //$FF26
    #endregion
    
    #region Wave pattern RAM

    private byte[] WaveRAM = new byte[16];
    
    #endregion

    #endregion
    
    #region Wave Duty

    private List<byte> WaveDutyTable = new List<byte>
    {
        0b00000001, //12.5%
        0b00000011, //25%
        0b00001111, //50%
        0b11111100, //75%
    };
    #endregion
    
    #region Non-Register Variables

    private List<int> waveDutyPositions = [0, 0, 0, 0];
    private List<bool> channelsEnabled = [false, false, false, false];
    private List<float> channelVolume = [0, 0, 0, 0];
    private List<int> frequencyTimer = [0, 0, 0, 0];
    private List<int> lengthTimer = [0, 0, 0, 0];
    private List<int> envelopeTimer = [0, 0, 0, 0];

    private BufferedWaveProvider waveProvider;
    private WaveOutEvent waveOut;
    private byte[] audioBuffer = new byte[BUFFER_SIZE * 2];
    private int bufferIndex = 0;

    private const int SAMPLE_RATE = 44100;
    private const int BUFFER_SIZE = SAMPLE_RATE / 60;
    #endregion

    public APU()
    {
        waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1));
        waveProvider.BufferLength = SAMPLE_RATE * 2;
        waveProvider.DiscardOnBufferOverflow = true;
        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();
    }

    private void UpdateAudio(float sample)
    {
        short sample16 = (short)(sample * short.MaxValue);
        
        if (waveProvider.BufferedBytes >= waveProvider.BufferLength - 2)
        {
            return; // Skip adding samples if the buffer is full
        }

        // Ensure we don't exceed buffer size
        if (bufferIndex + 2 >= audioBuffer.Length)
        {
            waveProvider.AddSamples(audioBuffer, 0, bufferIndex);
            bufferIndex = 0;
        }

        audioBuffer[bufferIndex++] = (byte)(sample16 & 0xFF);
        audioBuffer[bufferIndex++] = (byte)((sample16 >> 8) & 0xFF);
    }

    private int cycleCount = 0;

    //called every T-Cycle, (4 M-Cycles)
    public async Task Step()
    {
        cycleCount++;

        float stepSample = 0.0f;

        if (cycleCount % 8192 == 0)
        {
            StepFrameSequencer();
        }

        if (channelsEnabled[2])
        {
            bool playHigh = StepChannel2();
            stepSample += playHigh ? (channelVolume[2] / 15.0f) : -1;
        }

        stepSample *= 0.5f; //normalize volume

        UpdateAudio(stepSample);
    }

    private int frameSequencerStep = 0;
    private void StepFrameSequencer()
    {
        if (frameSequencerStep % 2 == 0 || frameSequencerStep == 0)
        {
            if (channelsEnabled[2])
                StepChannel2Length();
        }

        if (frameSequencerStep == 7)
        {
            if (envelopeTimer[2] > 0)
                StepChannel2Envelope();
        }
        
        frameSequencerStep++;

        if (frameSequencerStep > 7)
            frameSequencerStep = 0;
    }

    private void StepChannel2Envelope()
    {
        envelopeTimer[2]--;

        if (envelopeTimer[2] <= 0)
        {
            if (GetChannelEnvelopeDistance(NR22) == 1)
            {
                if (channelVolume[2] < 15)
                    channelVolume[2]++;
            }
            else
            {
                if (channelVolume[2] > 0)
                    channelVolume[2]--;
            }

            envelopeTimer[2] = GetChannelInitialVolume(NR22);
        }
    }

    private void StepChannel2Length()
    {
        if (GetChannelLengthEnabled(NR24))
        {
            lengthTimer[2]--;

            if (lengthTimer[2] <= 0)
            {
                channelsEnabled[2] = false;
            }
        }
    }

    private void TriggerChannel2()
    {
        channelsEnabled[2] = true;

        if (lengthTimer[2] <= 0)
        {
            lengthTimer[2] = GetChannelInitialLengthTimer(NR21);
        }
        
        ushort frequency = GetChannelFrequency(NR23, NR24);
        frequencyTimer[2] = (2048 - frequency) * 4;

        waveDutyPositions[2] = 0;
        
        int initialVolume = GetChannelInitialVolume(NR22);

        if (initialVolume == 0)
            channelsEnabled[2] = false;
        
        channelVolume[2] = initialVolume;
        envelopeTimer[2] = GetChannelEnvelopeTimer(NR22);
    }

    private bool StepChannel2()
    {
        int waveDuty = (NR21 >> 6) & 0b11;
        
        frequencyTimer[2]--;

        if (frequencyTimer[2] <= 0)
        {
            waveDutyPositions[2] = (waveDutyPositions[2] + 1) % 8;

            ushort frequency = GetChannelFrequency(NR23, NR24);
            frequencyTimer[2] = (2048 - frequency) * 4;
        }
        
        return ((WaveDutyTable[waveDuty] >> (7 - waveDutyPositions[2])) & 1) != 0;
    }
    
    private ushort GetChannelFrequency(byte NRx3Reg, byte NRx4Reg)
    {
        byte low = NRx3Reg;  
        byte high = NRx4Reg;
        return (ushort)(((high & 0b00000111) << 8) | low);
    }

    private byte GetChannelInitialVolume(byte NRx2Reg)
    {
        return (byte)(NRx2Reg & 0b11110000);
    }
    
    private byte GetChannelEnvelopeTimer(byte NRx2Reg)
    {
        return (byte)(NRx2Reg & 0b00000111);
    }
    
    private byte GetChannelEnvelopeDistance(byte NRx2Reg)
    {
        return (byte)(NRx2Reg & 0b00001000);
    }

    private byte GetChannelInitialLengthTimer(byte NRx1Reg)
    {
        return (byte)(64 - (NRx1Reg & 0b00111111));
    }

    private bool GetChannelLengthEnabled(byte NRx4Reg)
    {
        return (NRx4Reg & 0b01000000) != 0;
    }

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
            case 0xFF14:
            {
                NR14 = value;
                
                if ((NR14 & 0b10000000) != 0)
                {
                    //TriggerChannel1();
                }

                break;
            }
            
            case 0xFF16: NR21 = value; break;
            case 0xFF17: NR22 = value; break;
            case 0xFF18: NR23 = value; break;
            case 0xFF19:
            {
                NR24 = value;
                
                if ((NR24 & 0b10000000) != 0)
                {
                    TriggerChannel2();
                }

                break;
            }
            
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