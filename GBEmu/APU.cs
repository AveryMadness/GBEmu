using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GBEmu;

public class APU
{
    #region Sound Registers

    #region Square 1 (with sweep)
    private byte NR10 = 0x00; //$FF10 - Sweep (starts at 0x00, not 0x80)
    private byte NR11 = 0x00; //$FF11 - Length timer & Wave duty
    private byte NR12 = 0x00; //$FF12 - Volume & Envelope
    private byte NR13 = 0x00; //$FF13 - Frequency low
    private byte NR14 = 0x00; //$FF14 - Frequency high & Control
    #endregion
    
    #region Square 2
    private byte NR21 = 0x00; //$FF16 - Length timer & Wave duty
    private byte NR22 = 0x00; //$FF17 - Volume & Envelope
    private byte NR23 = 0x00; //$FF18 - Frequency low
    private byte NR24 = 0x00; //$FF19 - Frequency high & Control
    #endregion
    
    #region Wave
    private byte NR30 = 0x00; //$FF1A - Enable
    private byte NR31 = 0x00; //$FF1B - Length timer
    private byte NR32 = 0x00; //$FF1C - Output level
    private byte NR33 = 0x00; //$FF1D - Frequency low
    private byte NR34 = 0x00; //$FF1E - Frequency high & Control
    #endregion
    
    #region Noise
    private byte NR41 = 0x00; //$FF20 - Length timer
    private byte NR42 = 0x00; //$FF21 - Volume & Envelope
    private byte NR43 = 0x00; //$FF22 - Frequency & Randomness
    private byte NR44 = 0x00; //$FF23 - Control
    #endregion
    
    #region Control/Status
    private byte NR50 = 0x00; //$FF24 - Master volume & VIN panning
    private byte NR51 = 0x00; //$FF25 - Sound panning
    private byte NR52 = 0x00; //$FF26 - Sound on/off (starts powered OFF)
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
    private const int BUFFER_SIZE = SAMPLE_RATE / 30; // ~1470 samples per buffer
    private const int CLOCK_SPEED = 4194304; // T-cycle rate (4.194304 MHz)
    private const int CYCLES_PER_SAMPLE = CLOCK_SPEED / SAMPLE_RATE; // ~95 T-cycles per sample
    private const int FRAME_SEQUENCER_RATE = 8192; // Frame sequencer at 512Hz
    #endregion

    public APU()
    {
        // Initialize wave duty positions
        for (int i = 0; i < 4; i++)
        {
            waveDutyPositions[i] = 0;
            channelsEnabled[i] = false;
            channelVolume[i] = 0;
            frequencyTimer[i] = 0;
            lengthTimer[i] = 0;
            envelopeTimer[i] = 0;
            lengthEnabled[i] = false;
        }
        
        // Initialize sweep
        sweepTimer = 0;
        shadowFrequency = 0;
        sweepEnabled = false;
        
        // Initialize wave channel
        wavePosition = 0;
        waveAccessing = false;
        
        // Initialize noise
        noiseShiftRegister = 0x7FFF;
        
        // Initialize audio output
        waveProvider = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, 16, 2)); // Stereo output
        waveProvider.BufferLength = SAMPLE_RATE * 2; // 2 seconds buffer
        waveProvider.DiscardOnBufferOverflow = true;
        audioBuffer = new byte[BUFFER_SIZE * 4]; // Stereo, 16-bit samples
        
        // Initialize audio output device
        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();
        
        // Initialize Wave RAM with default pattern
        for (int i = 0; i < 16; i++)
        {
            WaveRAM[i] = 0x00;
        }
        
        Console.WriteLine($"APU initialized: {SAMPLE_RATE}Hz, {CYCLES_PER_SAMPLE} cycles/sample");
    }
    
    private int debugSampleCount = 0;
    private int debugStepCount = 0;
    
    public void PrintDebugInfo()
    {
        Console.WriteLine($"Steps: {debugStepCount}, Samples: {debugSampleCount}, Buffer: {waveProvider.BufferedBytes} bytes");
        Console.WriteLine($"Channels enabled: [{channelsEnabled[0]}, {channelsEnabled[1]}, {channelsEnabled[2]}, {channelsEnabled[3]}]");
        Console.WriteLine($"NR52: 0x{NR52:X2}, Master: {(NR52 & 0x80) != 0}");
    }
    
    private void PowerOffAPU()
    {
        // Clear all sound registers except length counters
        NR10 = 0x80;
        NR11 = (byte)(NR11 & 0x3F); // Keep length
        NR12 = 0x00;
        NR13 = 0x00;
        NR14 = (byte)(NR14 & 0x40); // Keep length enable
        
        NR21 = (byte)(NR21 & 0x3F);
        NR22 = 0x00;
        NR23 = 0x00;
        NR24 = (byte)(NR24 & 0x40);
        
        NR30 = 0x00;
        // NR31 kept
        NR32 = 0x00;
        NR33 = 0x00;
        NR34 = (byte)(NR34 & 0x40);
        
        // NR41 kept
        NR42 = 0x00;
        NR43 = 0x00;
        NR44 = (byte)(NR44 & 0x40);
        
        NR50 = 0x00;
        NR51 = 0x00;
        
        // Disable all channels
        for (int i = 0; i < 4; i++)
        {
            channelsEnabled[i] = false;
            channelVolume[i] = 0;
        }
    }
    
    public void FlushAudioBuffer()
    {
        if (bufferIndex > 0)
        {
            waveProvider.AddSamples(audioBuffer, 0, bufferIndex);
            bufferIndex = 0;
        }
    }

    // Disposes of audio resources
    public void Dispose()
    {
        FlushAudioBuffer();
        waveOut?.Stop();
        waveOut?.Dispose();
        waveProvider = null;
    }

    private void UpdateAudio()
    {
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
        
            // CHANGED: Use 0.3f instead of 0.25f for better mixing
            // This prevents the audio from being too quiet
            leftSample = Math.Clamp(leftSample * 0.3f, -1.0f, 1.0f);
            rightSample = Math.Clamp(rightSample * 0.3f, -1.0f, 1.0f);
        }
        
        // Convert to 16-bit samples
        short leftSample16 = (short)(leftSample * short.MaxValue);
        short rightSample16 = (short)(rightSample * short.MaxValue);
        
        // Add to buffer and flush when full
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
    private int frameSequencerCycles = 0;

    public void Step(int cycles = 1)
    {
        debugStepCount++;
        
        // Update master enable flag in NR52
        UpdateNR52();
        
        // Skip if sound is disabled
        if ((NR52 & 0x80) == 0)
        {
            return;
        }
        
        cycleCount += cycles;
        sampleCycleCounter += cycles;
        frameSequencerCycles += cycles;
        
        // Frame sequencer runs at 512 Hz (every 8192 T-cycles)
        while (frameSequencerCycles >= FRAME_SEQUENCER_RATE)
        {
            StepFrameSequencer();
            frameSequencerCycles -= FRAME_SEQUENCER_RATE;
        }
        
        // Update each channel
        for (int i = 0; i < cycles; i++)
        {
            StepChannel1();
            StepChannel2();
            StepChannel3();
            StepChannel4();
        }
        
        // Generate audio sample at appropriate rate
        while (sampleCycleCounter >= CYCLES_PER_SAMPLE)
        {
            UpdateAudio();
            debugSampleCount++;
            sampleCycleCounter -= CYCLES_PER_SAMPLE;
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
        float amplitude = channelVolume[0] / 15.0f;
        return high ? amplitude : -amplitude;
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
        
        // Initialize length timer from NR11
        int length = NR11 & 0x3F;
        lengthTimer[0] = (length == 0) ? 64 : (64 - length);
        
        // Set frequency
        ushort frequency = GetChannelFrequency(NR13, NR14);
        frequencyTimer[0] = (2048 - frequency) * 4;
        
        // Initialize volume
        channelVolume[0] = (NR12 >> 4) & 0x0F;
        
        // Initialize envelope
        int envelopePeriod = NR12 & 0x07;
        envelopeTimer[0] = (envelopePeriod == 0) ? 8 : envelopePeriod;
        
        // Initialize wave duty position
        waveDutyPositions[0] = 0;
        
        // Initialize sweep
        byte sweepPeriod = (byte)((NR10 >> 4) & 0x7);
        sweepTimer = (sweepPeriod == 0) ? 8 : sweepPeriod;
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
        // Get envelope period
        byte envelopePeriod = (byte)(NR12 & 0x07);
        
        // If period is 0, envelope is disabled
        if (envelopePeriod == 0)
        {
            return;
        }
        
        envelopeTimer[0]--;
        
        if (envelopeTimer[0] <= 0)
        {
            envelopeTimer[0] = envelopePeriod;
            
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

    #endregion

    #region Channel 2 (Square 2)

    private float GetSquare2Sample()
    {
        int waveDuty = (NR21 >> 6) & 0x03;
        bool high = ((WaveDutyTable[waveDuty] >> (7 - waveDutyPositions[1])) & 1) != 0;
        float amplitude = channelVolume[1] / 15.0f;
        return high ? amplitude : -amplitude;
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
        // Get envelope period
        byte envelopePeriod = (byte)(NR22 & 0x07);
        
        // If period is 0, envelope is disabled
        if (envelopePeriod == 0)
        {
            return;
        }
        
        envelopeTimer[1]--;
        
        if (envelopeTimer[1] <= 0)
        {
            envelopeTimer[1] = envelopePeriod;
            
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

    private void TriggerChannel2()
    {
        channelsEnabled[1] = true;
        
        // Initialize length timer from NR21
        int length = NR21 & 0x3F;
        lengthTimer[1] = (length == 0) ? 64 : (64 - length);
        
        // Set frequency
        ushort frequency = GetChannelFrequency(NR23, NR24);
        frequencyTimer[1] = (2048 - frequency) * 4;
        
        // Initialize wave duty position
        waveDutyPositions[1] = 0;
        
        // Initialize volume
        channelVolume[1] = (NR22 >> 4) & 0x0F;
        
        // Initialize envelope
        int envelopePeriod = NR22 & 0x07;
        envelopeTimer[1] = (envelopePeriod == 0) ? 8 : envelopePeriod;
        
        // Check if DAC is enabled
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
            return 0.0f;
        }
        
        // Get current wave sample
        int position = waveDutyPositions[2];
        byte waveByte = WaveRAM[position / 2];
        
        // Extract the correct 4-bit sample
        int sample = (position % 2 == 0) ? (waveByte >> 4) & 0x0F : waveByte & 0x0F;
        
        // Apply volume shift (BEFORE converting to float!)
        int volumeCode = (NR32 >> 5) & 0x03;
        switch (volumeCode)
        {
            case 0: sample = 0; break;           // Mute
            case 1: break;                        // 100%
            case 2: sample = sample >> 1; break; // 50%
            case 3: sample = sample >> 2; break; // 25%
        }
        
        // Convert 4-bit sample (0-15) to float (-1.0 to 1.0)
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
            frequencyTimer[2] = (2048 - frequency) * 2;
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
        
        // Initialize length timer from NR31 (full 8 bits!)
        lengthTimer[2] = (NR31 == 0) ? 256 : (256 - NR31);
        
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
        float amplitude = channelVolume[3] / 15.0f;
        return high ? amplitude : -amplitude;
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
        // Get envelope period
        byte envelopePeriod = (byte)(NR42 & 0x07);
        
        // If period is 0, envelope is disabled
        if (envelopePeriod == 0)
        {
            return;
        }
        
        envelopeTimer[3]--;
        
        if (envelopeTimer[3] <= 0)
        {
            envelopeTimer[3] = envelopePeriod;
            
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

    private void TriggerChannel4()
    {
        channelsEnabled[3] = true;
        
        // Initialize length timer from NR41
        int length = NR41 & 0x3F;
        lengthTimer[3] = (length == 0) ? 64 : (64 - length);
        
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
        int envelopePeriod = NR42 & 0x07;
        envelopeTimer[3] = (envelopePeriod == 0) ? 8 : envelopePeriod;
        
        // Check if DAC is enabled
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
            
            case 1:
                // Envelope only
                StepChannel1Envelope();
                StepChannel2Envelope();
                StepChannel4Envelope();
                break;
            
            case 2:
                StepChannel1Length();
                StepChannel2Length();
                StepChannel3Length();
                StepChannel4Length();
                StepChannel1Sweep();
                break;
            
            case 3:
                // Envelope only
                StepChannel1Envelope();
                StepChannel2Envelope();
                StepChannel4Envelope();
                break;
            
            case 4:
                StepChannel1Length();
                StepChannel2Length();
                StepChannel3Length();
                StepChannel4Length();
                break;
            
            case 5:
                // Envelope only
                StepChannel1Envelope();
                StepChannel2Envelope();
                StepChannel4Envelope();
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
    
    public void DebugChannelStatus()
    {
        /*WriteLine("=== APU Channel Status ===");
        Console.WriteLine($"NR52 (Master): 0x{NR52:X2}, Powered: {(NR52 & 0x80) != 0}");
        Console.WriteLine($"NR51 (Panning): 0x{NR51:X2}");
        Console.WriteLine($"NR50 (Volume): 0x{NR50:X2}");
        Console.WriteLine();
    
        // ADD THIS:
        Console.WriteLine("Envelope Registers:");
        Console.WriteLine($"  NR12 (Ch1): 0x{NR12:X2} - InitVol: {(NR12 >> 4) & 0xF}, Dir: {((NR12 & 0x08) != 0 ? "Inc" : "Dec")}, Period: {NR12 & 0x07}");
        Console.WriteLine($"  NR22 (Ch2): 0x{NR22:X2} - InitVol: {(NR22 >> 4) & 0xF}, Dir: {((NR22 & 0x08) != 0 ? "Inc" : "Dec")}, Period: {NR22 & 0x07}");
        Console.WriteLine($"  NR32 (Ch3): 0x{NR32:X2} - Volume Code: {(NR32 >> 5) & 0x03}");
        Console.WriteLine($"  NR42 (Ch4): 0x{NR42:X2} - InitVol: {(NR42 >> 4) & 0xF}, Dir: {((NR42 & 0x08) != 0 ? "Inc" : "Dec")}, Period: {NR42 & 0x07}");
        Console.WriteLine();
    
        Console.WriteLine("Channels Enabled:");
        for (int i = 0; i < 4; i++)
        {
            Console.WriteLine($"  Ch{i + 1}: {channelsEnabled[i]}, Volume: {channelVolume[i]}, Length: {lengthTimer[i]}, EnvTimer: {envelopeTimer[i]}");
        }*/
    }

    #endregion

    public void WriteRegister(ushort address, byte value)
    {
        // If APU is powered off, ignore all writes except to NR52
        bool apuPowered = (NR52 & 0x80) != 0;
        
        switch (address)
        {
            case 0xFF26:
            {
                bool wasPowered = (NR52 & 0x80) != 0;
                bool nowPowered = (value & 0x80) != 0;
                
                // Only first bit is R/W, rest are RO
                byte mask = 0b10000000;
                NR52 = (byte)((NR52 & ~mask) | (value & mask));
                
                // If powering off, clear all registers
                if (wasPowered && !nowPowered)
                {
                    PowerOffAPU();
                }
                // If powering on, initialize frame sequencer
                else if (!wasPowered && nowPowered)
                {
                    frameSequencerStep = 0;
                }
                
                break;
            }
            
            //all RW/WO
            case 0xFF25: 
                NR51 = value;  // Always writable
                break;
            case 0xFF24: 
                NR50 = value;  // Always writable
                break;
            
            case 0xFF10: 
                if (apuPowered) NR10 = value; 
                break;
            case 0xFF11: 
                if (apuPowered) NR11 = value;
                else NR11 = (byte)((NR11 & 0x3F) | (value & 0xC0)); // Length can always be written
                break;
            case 0xFF12: 
                if (apuPowered) 
                {
                    NR12 = value;
                    // If DAC is disabled, disable channel
                    if ((NR12 & 0xF8) == 0)
                    {
                        channelsEnabled[0] = false;
                    }
                }
                break;
            case 0xFF13: 
                if (apuPowered) NR13 = value; 
                break;
            case 0xFF14:
            {
                if (apuPowered)
                {
                    NR14 = value;
                    
                    if ((NR14 & 0b10000000) != 0)
                    {
                        TriggerChannel1();
                    }
                }
                else
                {
                    // Only length enable can be written when powered off
                    NR14 = (byte)((NR14 & 0xBF) | (value & 0x40));
                }

                break;
            }
            
            case 0xFF16: 
                if (apuPowered) NR21 = value;
                else NR21 = (byte)((NR21 & 0x3F) | (value & 0xC0));
                break;
            case 0xFF17: 
                if (apuPowered) 
                {
                    NR22 = value;
                    if ((NR22 & 0xF8) == 0)
                    {
                        channelsEnabled[1] = false;
                    }
                }
                break;
            case 0xFF18: 
                if (apuPowered) NR23 = value; 
                break;
            case 0xFF19:
            {
                if (apuPowered)
                {
                    NR24 = value;
                    
                    if ((NR24 & 0b10000000) != 0)
                    {
                        TriggerChannel2();
                    }
                }
                else
                {
                    NR24 = (byte)((NR24 & 0xBF) | (value & 0x40));
                }

                break;
            }
            
            case 0xFF1A: 
                if (apuPowered) 
                {
                    NR30 = value;
                    // If DAC is disabled, disable channel
                    if ((NR30 & 0x80) == 0)
                    {
                        channelsEnabled[2] = false;
                    }
                }
                break;
            case 0xFF1B: 
                if (apuPowered) NR31 = value;
                // Length can always be written
                else NR31 = value;
                break;
            case 0xFF1C: 
                if (apuPowered) NR32 = value; 
                break;
            case 0xFF1D: 
                if (apuPowered) NR33 = value; 
                break;
            case 0xFF1E:
            {
                if (apuPowered)
                {
                    NR34 = value;
                    
                    if ((NR34 & 0b10000000) != 0)
                    {
                        TriggerChannel3();
                    }
                }
                else
                {
                    NR34 = (byte)((NR34 & 0xBF) | (value & 0x40));
                }

                break;
            }

            case 0xFF20: 
                if (apuPowered) NR41 = value;
                // Length can always be written
                else NR41 = value;
                break;
            case 0xFF21: 
                if (apuPowered) 
                {
                    NR42 = value;
                    if ((NR42 & 0xF8) == 0)
                    {
                        channelsEnabled[3] = false;
                    }
                }
                break;
            case 0xFF22: 
                if (apuPowered) NR43 = value; 
                break;
            case 0xFF23:
            {
                if (apuPowered)
                {
                    NR44 = value;
                    
                    if ((NR44 & 0b10000000) != 0)
                    {
                        TriggerChannel4();
                    }
                }
                else
                {
                    NR44 = (byte)((NR44 & 0xBF) | (value & 0x40));
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