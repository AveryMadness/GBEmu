using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GBEmu;

public class APU
{
    #region Sound Registers

    #region Square 1 (with sweep)
    private byte NR10 = 0x80; //$FF10 - Sweep
    private byte NR11 = 0xBF; //$FF11 - Length timer & Wave duty
    private byte NR12 = 0xF3; //$FF12 - Volume & Envelope
    private byte NR13 = 0xFF; //$FF13 - Frequency low
    private byte NR14 = 0xBF; //$FF14 - Frequency high & Control
    #endregion
    
    #region Square 2
    private byte NR21 = 0x3F; //$FF16 - Length timer & Wave duty
    private byte NR22 = 0x00; //$FF17 - Volume & Envelope
    private byte NR23 = 0xFF; //$FF18 - Frequency low
    private byte NR24 = 0xBF; //$FF19 - Frequency high & Control
    #endregion
    
    #region Wave
    private byte NR30 = 0x7F; //$FF1A - Enable
    private byte NR31 = 0xFF; //$FF1B - Length timer
    private byte NR32 = 0x9F; //$FF1C - Output level
    private byte NR33 = 0xFF; //$FF1D - Frequency low
    private byte NR34 = 0xBF; //$FF1E - Frequency high & Control
    #endregion
    
    #region Noise
    private byte NR41 = 0xFF; //$FF20 - Length timer
    private byte NR42 = 0x00; //$FF21 - Volume & Envelope
    private byte NR43 = 0x00; //$FF22 - Frequency & Randomness
    private byte NR44 = 0xBF; //$FF23 - Control
    #endregion
    
    #region Control/Status
    private byte NR50 = 0x77; //$FF24 - Master volume & VIN panning
    private byte NR51 = 0xF3; //$FF25 - Sound panning
    private byte NR52 = 0xF1; //$FF26 - Sound on/off
    #endregion
    
    #region Wave pattern RAM
    private byte[] WaveRAM = new byte[16];
    #endregion

    #endregion
    
    #region Wave Duty
    private byte[] WaveDutyTable = new byte[]
    {
        0b00000001, //12.5%
        0b00000011, //25%
        0b00001111, //50%
        0b11111100, //75%
    };
    #endregion
    
    #region Non-Register Variables
    private int[] waveDutyPositions = new int[4];
    private bool[] channelsEnabled = new bool[4];
    private float[] channelVolume = new float[4];
    private int[] frequencyTimer = new int[4];
    private int[] lengthTimer = new int[4];
    private int[] envelopeTimer = new int[4];
    private bool[] lengthEnabled = new bool[4];
    
    // Channel 1 sweep
    private int sweepTimer;
    private int shadowFrequency;
    private bool sweepEnabled;
    
    // Channel 3 wave variables
    private int wavePosition;
    private bool waveAccessing;
    
    // Channel 4 noise variables
    private ushort noiseShiftRegister = 0x7FFF; // 15-bit shift register
    
    // Audio output
    private BufferedWaveProvider waveProvider;
    private WaveOutEvent waveOut;
    private byte[] audioBuffer;
    private int bufferIndex = 0;
    
    // Constants
    private const int SAMPLE_RATE = 44100;
    private const int BUFFER_SIZE = SAMPLE_RATE / 60;
    private const int CLOCK_SPEED = 4194304; // Game Boy CPU clock speed
    private const int CYCLES_PER_SAMPLE = CLOCK_SPEED / SAMPLE_RATE;
    #endregion

    public APU()
    {
        // Initialize wave duty positions
        for (int i = 0; i < 4; i++)
        {
            waveDutyPositions[i] = 0;
        }
        
        // Initialize audio output
        waveProvider = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, 16, 2)); // Stereo output
        waveProvider.BufferLength = SAMPLE_RATE * 4; // 2 seconds buffer
        waveProvider.DiscardOnBufferOverflow = true;
        audioBuffer = new byte[BUFFER_SIZE * 4]; // Stereo, 16-bit samples
        
        // Initialize audio output device
        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();
        
        // Initialize Wave RAM with default values
        for (int i = 0; i < 16; i++)
        {
            WaveRAM[i] = (byte)(i % 2 == 0 ? 0x00 : 0xFF);
        }
    }

    // Disposes of audio resources
    public void Dispose()
    {
        waveOut?.Stop();
        waveOut?.Dispose();
        waveProvider = null;
    }

    private void UpdateAudio()
    {
        // Mix all channels
        float leftSample = 0.0f;
        float rightSample = 0.0f;
        
        bool masterEnable = (NR52 & 0x80) != 0;
        if (!masterEnable)
        {
            leftSample = 0.0f;
            rightSample = 0.0f;
        }
        else
        {
            byte leftVolume = (byte)((NR50 >> 4) & 0x7);
            byte rightVolume = (byte)(NR50 & 0x7);
            float leftVolumeScale = leftVolume / 7.0f;
            float rightVolumeScale = rightVolume / 7.0f;
            
            // Channel 1 - Square 1
            if (channelsEnabled[0])
            {
                float ch1Sample = GetSquare1Sample();
                if ((NR51 & 0x10) != 0) leftSample += ch1Sample * leftVolumeScale;
                if ((NR51 & 0x01) != 0) rightSample += ch1Sample * rightVolumeScale;
            }
            
            // Channel 2 - Square 2
            if (channelsEnabled[1])
            {
                float ch2Sample = GetSquare2Sample();
                if ((NR51 & 0x20) != 0) leftSample += ch2Sample * leftVolumeScale;
                if ((NR51 & 0x02) != 0) rightSample += ch2Sample * rightVolumeScale;
            }
            
            // Channel 3 - Wave
            if (channelsEnabled[2])
            {
                float ch3Sample = GetWaveSample();
                if ((NR51 & 0x40) != 0) leftSample += ch3Sample * leftVolumeScale;
                if ((NR51 & 0x04) != 0) rightSample += ch3Sample * rightVolumeScale;
            }
            
            // Channel 4 - Noise
            if (channelsEnabled[3])
            {
                float ch4Sample = GetNoiseSample();
                if ((NR51 & 0x80) != 0) leftSample += ch4Sample * leftVolumeScale;
                if ((NR51 & 0x08) != 0) rightSample += ch4Sample * rightVolumeScale;
            }
            
            // Normalize
            leftSample = Math.Clamp(leftSample * 0.25f, -1.0f, 1.0f);
            rightSample = Math.Clamp(rightSample * 0.25f, -1.0f, 1.0f);
        }
        
        // Convert to 16-bit samples
        short leftSample16 = (short)(leftSample * short.MaxValue);
        short rightSample16 = (short)(rightSample * short.MaxValue);
        
        // Check buffer capacity
        if (waveProvider.BufferedBytes >= waveProvider.BufferLength - 4)
        {
            return; // Buffer is full, skip this sample
        }
        
        // Add samples to buffer
        if (bufferIndex + 4 >= audioBuffer.Length)
        {
            waveProvider.AddSamples(audioBuffer, 0, bufferIndex);
            bufferIndex = 0;
        }
        
        // Write interleaved stereo samples (left, right)
        audioBuffer[bufferIndex++] = (byte)(leftSample16 & 0xFF);
        audioBuffer[bufferIndex++] = (byte)((leftSample16 >> 8) & 0xFF);
        audioBuffer[bufferIndex++] = (byte)(rightSample16 & 0xFF);
        audioBuffer[bufferIndex++] = (byte)((rightSample16 >> 8) & 0xFF);
    }

    private int cycleCount = 0;
    private int sampleCycleCounter = 0;

    // Called every T-cycle (4 M-cycles)
    public async Task Step()
    {
        // Update master enable flag in NR52
        UpdateNR52();
        
        // Skip if sound is disabled
        if ((NR52 & 0x80) == 0)
        {
            return;
        }
        
        cycleCount++;
        sampleCycleCounter++;
        
        // Frame sequencer runs at 512 Hz (every 8192 cycles)
        if (cycleCount % 8192 == 0)
        {
            StepFrameSequencer();
        }
        
        // Update each channel
        StepChannel1();
        StepChannel2();
        StepChannel3();
        StepChannel4();
        
        // Generate audio sample at appropriate rate
        if (sampleCycleCounter >= CYCLES_PER_SAMPLE)
        {
            UpdateAudio();
            sampleCycleCounter = 0;
        }
    }

    private void UpdateNR52()
    {
        // Update bits 0-3 with channel status
        byte status = (byte)(NR52 & 0xF0);
        for (int i = 0; i < 4; i++)
        {
            if (channelsEnabled[i])
            {
                status |= (byte)(1 << i);
            }
        }
        NR52 = status;
    }

    #region Channel 1 (Square 1 with sweep)

    private float GetSquare1Sample()
    {
        int waveDuty = (NR11 >> 6) & 0x03;
        bool high = ((WaveDutyTable[waveDuty] >> (7 - waveDutyPositions[0])) & 1) != 0;
        return high ? (channelVolume[0] / 15.0f) : -1.0f;
    }

    private void StepChannel1()
    {
        if (!channelsEnabled[0]) return;
        
        // Frequency timer
        frequencyTimer[0]--;
        if (frequencyTimer[0] <= 0)
        {
            waveDutyPositions[0] = (waveDutyPositions[0] + 1) % 8;
            
            ushort frequency = GetChannelFrequency(NR13, NR14);
            frequencyTimer[0] = (2048 - frequency) * 4;
        }
    }

    private void StepChannel1Sweep()
    {
        if (!sweepEnabled) return;
        
        sweepTimer--;
        if (sweepTimer <= 0)
        {
            byte sweepPeriod = (byte)((NR10 >> 4) & 0x7);
            sweepTimer = sweepPeriod > 0 ? sweepPeriod : 8;
            
            if (sweepPeriod > 0)
            {
                // Calculate new frequency
                int newFrequency = CalculateSweepFrequency();
                
                // Check if new frequency is valid
                if (newFrequency < 2048 && (NR10 & 0x07) != 0)
                {
                    shadowFrequency = newFrequency;
                    
                    // Update NR13 and NR14
                    NR13 = (byte)(newFrequency & 0xFF);
                    NR14 = (byte)((NR14 & 0xF8) | ((newFrequency >> 8) & 0x07));
                    
                    // Perform overflow check again
                    CalculateSweepFrequency();
                }
            }
        }
    }

    private int CalculateSweepFrequency()
    {
        int shift = NR10 & 0x07;
        int change = shadowFrequency >> shift;
        
        if ((NR10 & 0x08) != 0)
        {
            // Negate
            change = -change;
        }
        
        int newFrequency = shadowFrequency + change;
        
        // Overflow check
        if (newFrequency >= 2048)
        {
            channelsEnabled[0] = false;
        }
        
        return newFrequency;
    }

    private void TriggerChannel1()
    {
        channelsEnabled[0] = true;
        
        // Initialize length timer if it's zero
        if (lengthTimer[0] <= 0)
        {
            lengthTimer[0] = 64 - (NR11 & 0x3F);
        }
        
        // Set frequency
        ushort frequency = GetChannelFrequency(NR13, NR14);
        frequencyTimer[0] = (2048 - frequency) * 4;
        
        // Initialize volume
        channelVolume[0] = (NR12 >> 4) & 0x0F;
        
        // Initialize envelope
        envelopeTimer[0] = NR12 & 0x07;
        
        // Initialize wave duty position
        waveDutyPositions[0] = 0;
        
        // Initialize sweep
        byte sweepPeriod = (byte)((NR10 >> 4) & 0x7);
        sweepTimer = sweepPeriod > 0 ? sweepPeriod : 8;
        shadowFrequency = frequency;
        sweepEnabled = (sweepPeriod > 0) || ((NR10 & 0x07) > 0);
        
        // Immediately perform overflow check
        if ((NR10 & 0x07) > 0)
        {
            CalculateSweepFrequency();
        }
        
        // Check if DAC is enabled (NR12 & 0xF8 != 0)
        if ((NR12 & 0xF8) == 0)
        {
            channelsEnabled[0] = false;
        }
        
        // Update length enabled flag
        lengthEnabled[0] = (NR14 & 0x40) != 0;
    }

    private void StepChannel1Length()
    {
        if (lengthEnabled[0] && lengthTimer[0] > 0)
        {
            lengthTimer[0]--;
            if (lengthTimer[0] <= 0)
            {
                channelsEnabled[0] = false;
            }
        }
    }

    private void StepChannel1Envelope()
    {
        if (envelopeTimer[0] > 0)
        {
            envelopeTimer[0]--;
            
            if (envelopeTimer[0] <= 0)
            {
                byte envelopePeriod = (byte)(NR12 & 0x07);
                envelopeTimer[0] = envelopePeriod;
                
                if (envelopePeriod > 0)
                {
                    bool increase = (NR12 & 0x08) != 0;
                    
                    if (increase && channelVolume[0] < 15)
                    {
                        channelVolume[0]++;
                    }
                    else if (!increase && channelVolume[0] > 0)
                    {
                        channelVolume[0]--;
                    }
                }
            }
        }
    }

    #endregion

    #region Channel 2 (Square 2)

    private float GetSquare2Sample()
    {
        int waveDuty = (NR21 >> 6) & 0x03;
        bool high = ((WaveDutyTable[waveDuty] >> (7 - waveDutyPositions[1])) & 1) != 0;
        return high ? (channelVolume[1] / 15.0f) : -1.0f;
    }

    private bool StepChannel2()
    {
        if (!channelsEnabled[1]) return false;
        
        frequencyTimer[1]--;
        if (frequencyTimer[1] <= 0)
        {
            waveDutyPositions[1] = (waveDutyPositions[1] + 1) % 8;
            
            ushort frequency = GetChannelFrequency(NR23, NR24);
            frequencyTimer[1] = (2048 - frequency) * 4;
        }
        
        int waveDuty = (NR21 >> 6) & 0x03;
        return ((WaveDutyTable[waveDuty] >> (7 - waveDutyPositions[1])) & 1) != 0;
    }

    private void StepChannel2Length()
    {
        if (lengthEnabled[1] && lengthTimer[1] > 0)
        {
            lengthTimer[1]--;
            if (lengthTimer[1] <= 0)
            {
                channelsEnabled[1] = false;
            }
        }
    }

    private void StepChannel2Envelope()
    {
        if (envelopeTimer[1] > 0)
        {
            envelopeTimer[1]--;
            
            if (envelopeTimer[1] <= 0)
            {
                byte envelopePeriod = (byte)(NR22 & 0x07);
                envelopeTimer[1] = envelopePeriod;
                
                if (envelopePeriod > 0)
                {
                    bool increase = (NR22 & 0x08) != 0;
                    
                    if (increase && channelVolume[1] < 15)
                    {
                        channelVolume[1]++;
                    }
                    else if (!increase && channelVolume[1] > 0)
                    {
                        channelVolume[1]--;
                    }
                }
            }
        }
    }

    private void TriggerChannel2()
    {
        channelsEnabled[1] = true;
        
        // Initialize length timer if it's zero
        if (lengthTimer[1] <= 0)
        {
            lengthTimer[1] = 64 - (NR21 & 0x3F);
        }
        
        // Set frequency
        ushort frequency = GetChannelFrequency(NR23, NR24);
        frequencyTimer[1] = (2048 - frequency) * 4;
        
        // Initialize wave duty position
        waveDutyPositions[1] = 0;
        
        // Initialize volume
        channelVolume[1] = (NR22 >> 4) & 0x0F;
        
        // Initialize envelope
        envelopeTimer[1] = NR22 & 0x07;
        
        // Check if DAC is enabled (NR22 & 0xF8 != 0)
        if ((NR22 & 0xF8) == 0)
        {
            channelsEnabled[1] = false;
        }
        
        // Update length enabled flag
        lengthEnabled[1] = (NR24 & 0x40) != 0;
    }

    #endregion

    #region Channel 3 (Wave)

    private float GetWaveSample()
    {
        if (!channelsEnabled[2] || (NR30 & 0x80) == 0)
        {
            return -1.0f;
        }
        
        // Get current wave sample
        int position = waveDutyPositions[2];
        byte waveByte = WaveRAM[position / 2];
        
        // Extract the correct 4-bit sample
        int sample = (position % 2 == 0) ? (waveByte >> 4) & 0x0F : waveByte & 0x0F;
        
        // Apply volume shift
        int volumeCode = (NR32 >> 5) & 0x03;
        if (volumeCode == 0) return -1.0f; // Muted
        
        // Apply volume shift: 0=0%, 1=100%, 2=50%, 3=25%
        switch (volumeCode)
        {
            case 0: sample = 0; break;       // 0% (mute)
            case 1: break;                   // 100%
            case 2: sample = sample >> 1; break; // 50%
            case 3: sample = sample >> 2; break; // 25%
        }
        
        // Convert to float in range [-1.0, 1.0]
        return (sample / 7.5f) - 1.0f;
    }

    private void StepChannel3()
    {
        if (!channelsEnabled[2] || (NR30 & 0x80) == 0) return;
        
        frequencyTimer[2]--;
        if (frequencyTimer[2] <= 0)
        {
            waveDutyPositions[2] = (waveDutyPositions[2] + 1) % 32;
            
            ushort frequency = GetChannelFrequency(NR33, NR34);
            frequencyTimer[2] = (2048 - frequency) * 2; // Wave channel runs at double the speed
        }
        
        // Set flag when accessing wave RAM
        waveAccessing = true;
        waveAccessing = false;
    }

    private void StepChannel3Length()
    {
        if (lengthEnabled[2] && lengthTimer[2] > 0)
        {
            lengthTimer[2]--;
            if (lengthTimer[2] <= 0)
            {
                channelsEnabled[2] = false;
            }
        }
    }

    private void TriggerChannel3()
    {
        channelsEnabled[2] = true;
        
        // Wave channel is enabled only if the DAC is on
        if ((NR30 & 0x80) == 0)
        {
            channelsEnabled[2] = false;
        }
        
        // Initialize length timer if it's zero
        if (lengthTimer[2] <= 0)
        {
            lengthTimer[2] = 256 - NR31;
        }
        
        // Set frequency
        ushort frequency = GetChannelFrequency(NR33, NR34);
        frequencyTimer[2] = (2048 - frequency) * 2;
        
        // Reset wave position
        waveDutyPositions[2] = 0;
        
        // Update length enabled flag
        lengthEnabled[2] = (NR34 & 0x40) != 0;
    }

    #endregion

    #region Channel 4 (Noise)

    private float GetNoiseSample()
    {
        // Get current noise sample (bit 0 of shift register)
        bool high = (noiseShiftRegister & 0x1) == 0;
        return high ? (channelVolume[3] / 15.0f) : -1.0f;
    }

    private void StepChannel4()
    {
        if (!channelsEnabled[3]) return;
        
        frequencyTimer[3]--;
        if (frequencyTimer[3] <= 0)
        {
            // Get divisor code
            byte divisorCode = (byte)(NR43 & 0x07);
            int divisor = divisorCode == 0 ? 8 : divisorCode * 16;
            
            // Get shift amount
            byte shift = (byte)((NR43 >> 4) & 0x0F);
            
            // Reset timer
            frequencyTimer[3] = divisor << shift;
            
            // Update LFSR (Linear Feedback Shift Register)
            bool bit0 = (noiseShiftRegister & 0x1) != 0;
            bool bit1 = (noiseShiftRegister & 0x2) != 0;
            bool xor = bit0 ^ bit1;
            
            noiseShiftRegister >>= 1;
            
            if (xor)
            {
                noiseShiftRegister |= 0x4000; // Set bit 14
                
                // If 7-bit mode, also set bit 6
                if ((NR43 & 0x08) != 0)
                {
                    noiseShiftRegister |= 0x40;
                }
            }
        }
    }

    private void StepChannel4Length()
    {
        if (lengthEnabled[3] && lengthTimer[3] > 0)
        {
            lengthTimer[3]--;
            if (lengthTimer[3] <= 0)
            {
                channelsEnabled[3] = false;
            }
        }
    }

    private void StepChannel4Envelope()
    {
        if (envelopeTimer[3] > 0)
        {
            envelopeTimer[3]--;
            
            if (envelopeTimer[3] <= 0)
            {
                byte envelopePeriod = (byte)(NR42 & 0x07);
                envelopeTimer[3] = envelopePeriod;
                
                if (envelopePeriod > 0)
                {
                    bool increase = (NR42 & 0x08) != 0;
                    
                    if (increase && channelVolume[3] < 15)
                    {
                        channelVolume[3]++;
                    }
                    else if (!increase && channelVolume[3] > 0)
                    {
                        channelVolume[3]--;
                    }
                }
            }
        }
    }

    private void TriggerChannel4()
    {
        channelsEnabled[3] = true;
        
        // Initialize length timer if it's zero
        if (lengthTimer[3] <= 0)
        {
            lengthTimer[3] = 64 - (NR41 & 0x3F);
        }
        
        // Set frequency timer
        byte divisorCode = (byte)(NR43 & 0x07);
        int divisor = divisorCode == 0 ? 8 : divisorCode * 16;
        byte shift = (byte)((NR43 >> 4) & 0x0F);
        frequencyTimer[3] = divisor << shift;
        
        // Initialize noise shift register
        noiseShiftRegister = 0x7FFF;
        
        // Initialize volume
        channelVolume[3] = (NR42 >> 4) & 0x0F;
        
        // Initialize envelope
        envelopeTimer[3] = NR42 & 0x07;
        
        // Check if DAC is enabled (NR42 & 0xF8 != 0)
        if ((NR42 & 0xF8) == 0)
        {
            channelsEnabled[3] = false;
        }
        
        // Update length enabled flag
        lengthEnabled[3] = (NR44 & 0x40) != 0;
    }

    #endregion

    #region Frame Sequencer

    private int frameSequencerStep = 0;
    
    private void StepFrameSequencer()
    {
        switch (frameSequencerStep)
        {
            case 0:
                StepChannel1Length();
                StepChannel2Length();
                StepChannel3Length();
                StepChannel4Length();
                break;
            
            case 2:
                StepChannel1Length();
                StepChannel2Length();
                StepChannel3Length();
                StepChannel4Length();
                StepChannel1Sweep();
                break;
            
            case 4:
                StepChannel1Length();
                StepChannel2Length();
                StepChannel3Length();
                StepChannel4Length();
                break;
            
            case 6:
                StepChannel1Length();
                StepChannel2Length();
                StepChannel3Length();
                StepChannel4Length();
                StepChannel1Sweep();
                break;
            
            case 7:
                StepChannel1Envelope();
                StepChannel2Envelope();
                StepChannel4Envelope();
                break;
        }
        
        frameSequencerStep = (frameSequencerStep + 1) % 8;
    }

    #endregion

    #region Utility Methods

    private ushort GetChannelFrequency(byte NRx3Reg, byte NRx4Reg)
    {
        byte low = NRx3Reg;
        byte high = NRx4Reg;
        return (ushort)(((high & 0x07) << 8) | low);
    }

    private byte GetChannelInitialVolume(byte NRx2Reg)
    {
        return (byte)((NRx2Reg & 0xF0) >> 4);
    }
    
    private byte GetChannelEnvelopeTimer(byte NRx2Reg)
    {
        return (byte)(NRx2Reg & 0x07);
    }
    
    private bool GetChannelEnvelopeDirection(byte NRx2Reg)
    {
        return (NRx2Reg & 0x08) != 0;
    }

    private byte GetChannelInitialLengthTimer(byte NRx1Reg)
    {
        return (byte)(64 - (NRx1Reg & 0x3F));
    }

    private bool GetChannelLengthEnabled(byte NRx4Reg)
    {
        return (NRx4Reg & 0x40) != 0;
    }

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
            case 0xFF14:
            {
                NR14 = value;
                
                if ((NR14 & 0b10000000) != 0)
                {
                    TriggerChannel1();
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
            case 0xFF1E:
            {
                NR34 = value;
                
                if ((NR34 & 0b10000000) != 0)
                {
                    TriggerChannel3();
                }

                break;
            }

            case 0xFF20: NR41 = value; break;
            case 0xFF21: NR42 = value; break;
            case 0xFF22: NR43 = value; break;
            case 0xFF23:
            {
                NR44 = value;
                
                if ((NR44 & 0b10000000) != 0)
                {
                    TriggerChannel4();
                }

                break;
            }

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