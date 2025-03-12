using System.Numerics;

namespace GBEmu;

public class GPU
{
    private const byte LCDC_BG_DISPLAY = 0x01;
    private const byte LCDC_OBJ_ENABLE = 0x02;
    private const byte LCDC_OBJ_SIZE = 0x04;
    private const byte LCDC_BG_MAP = 0x08;
    private const byte LCDC_BG_WINDOW_TILE_DATA = 0x10;
    private const byte LCDC_WINDOW_ENABLE = 0x20;
    private const byte LCDC_WINDOW_MAP = 0x40;
    private const byte LCDC_DISPLAY_ENABLE = 0x80;

    private const byte STAT_MODE_FLAG = 0x03;
    private const byte STAT_LYC_COINCIDENCE = 0x04;
    private const byte STAT_HBLANK_INTERRUPT = 0x08;
    private const byte STAT_VBLANK_INTERRUPT = 0x10;
    private const byte STAT_OAM_INTERRUPT = 0x20;
    private const byte STAT_LYC_INTERRUPT = 0x40;

    private const byte MODE_HBLANK = 0;
    private const byte MODE_VBLANK = 1;
    private const byte MODE_OAM = 2;
    private const byte MODE_TRANSFER = 3;

    private const int OAM_SCAN_CYCLES = 80;
    private const int PIXEL_TRANSFER_CYCLES = 172;
    private const int HBLANK_CYCLES = 204;
    private const int LINE_CYCLES = OAM_SCAN_CYCLES + PIXEL_TRANSFER_CYCLES + HBLANK_CYCLES; // 456
    private const int VBLANK_LINES = 10;
    private const int SCREEN_WIDTH = 160;
    private const int SCREEN_HEIGHT = 144;

    private byte[] vram = new byte[0x2000]; 
    private byte[] oam = new byte[0xA0];    
    
    private byte lcdc; 
    private byte stat; 
    private byte scy;  
    private byte scx; 
    private byte ly;   
    private byte lyc; 
    private byte wy;   
    private byte wx;  
    private byte bgp;  
    private byte obp0; 
    private byte obp1; 

    private int modeClock;
    private byte mode;   

    private byte[,] frameBuffer = new byte[SCREEN_WIDTH, SCREEN_HEIGHT];
    private byte[,] tileData = new byte[384, 64]; 

    private MemoryBus memoryBus;

    public GPU()
    {
        Reset();
    }

    public void SetMemoryBus(MemoryBus memoryBus)
    {
        this.memoryBus = memoryBus;
    }

    public void Reset()
    {
        lcdc = 0x91;
        stat = 0;
        scy = 0;
        scx = 0;
        ly = 0;
        lyc = 0;
        wy = 0;
        wx = 0;
        bgp = 0xFC; 
        obp0 = 0xFF;
        obp1 = 0xFF;
        
        mode = MODE_OAM;
        modeClock = 0;
        
        Array.Clear(vram, 0, vram.Length);
        Array.Clear(oam, 0, oam.Length);
        Array.Clear(frameBuffer, 0, frameBuffer.Length);
    }

    public void Step(int cycles)
    {
        if ((lcdc & LCDC_DISPLAY_ENABLE) == 0)
        {
            modeClock = 0;
            ly = 0;
            mode = MODE_HBLANK;
            stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
            return;
        }

        modeClock += cycles;

        switch (mode)
        {
            case MODE_OAM:
                if (modeClock >= OAM_SCAN_CYCLES)
                {
                    modeClock -= OAM_SCAN_CYCLES;
                    mode = MODE_TRANSFER;
                    stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                }
                break;
                
            case MODE_TRANSFER:
                if (modeClock >= PIXEL_TRANSFER_CYCLES)
                {
                    modeClock -= PIXEL_TRANSFER_CYCLES;
                    mode = MODE_HBLANK;
                    stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                    
                    RenderScanline();
                    
                    if ((stat & STAT_HBLANK_INTERRUPT) != 0)
                    {
                        RequestLCDInterrupt();
                    }
                }
                break;
                
            case MODE_HBLANK:
                if (modeClock >= HBLANK_CYCLES)
                {
                    modeClock -= HBLANK_CYCLES;
                    ly++;
                    
                    CheckLYCoincidence();
                    
                    if (ly >= SCREEN_HEIGHT)
                    {
                        mode = MODE_VBLANK;
                        stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                        
                        Program.RenderFrame(frameBuffer);
                        
                        RequestVBlankInterrupt();
                        
                        if ((stat & STAT_VBLANK_INTERRUPT) != 0)
                        {
                            RequestLCDInterrupt();
                        }
                    }
                    else
                    {
                        mode = MODE_OAM;
                        stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                        
                        if ((stat & STAT_OAM_INTERRUPT) != 0)
                        {
                            RequestLCDInterrupt();
                        }
                    }
                }
                break;
                
            case MODE_VBLANK:
                if (modeClock >= LINE_CYCLES)
                {
                    modeClock -= LINE_CYCLES;
                    ly++;
                    
                    CheckLYCoincidence();
                    
                    if (ly >= SCREEN_HEIGHT + VBLANK_LINES)
                    {
                        mode = MODE_OAM;
                        stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                        ly = 0;
                        
                        CheckLYCoincidence();
                        
                        if ((stat & STAT_OAM_INTERRUPT) != 0)
                        {
                            RequestLCDInterrupt();
                        }
                    }
                }
                break;
        }
    }

    private void CheckLYCoincidence()
    {
        if (ly == lyc)
        {
            stat |= STAT_LYC_COINCIDENCE;
            
            if ((stat & STAT_LYC_INTERRUPT) != 0)
            {
                RequestLCDInterrupt();
            }
        }
        else
        {
            stat &= 0xFB;
        }
    }

    private void RequestLCDInterrupt()
    {
        SM83.RequestInterrupt(SM83.InterruptFlags.LCDStat);
    }

    private void RequestVBlankInterrupt()
    { 
        SM83.RequestInterrupt(SM83.InterruptFlags.VBlank);
    }

    private void RenderScanline()
    {
        if ((lcdc & LCDC_BG_DISPLAY) != 0)
        {
            RenderBackground();
        }
        
        if ((lcdc & LCDC_WINDOW_ENABLE) != 0 && ly >= wy)
        {
            RenderWindow();
        }
        
        if ((lcdc & LCDC_OBJ_ENABLE) != 0)
        {
            RenderSprites();
        }
    }

    private void RenderBackground()
    {
        bool unsignedTileIndexing = (lcdc & LCDC_BG_WINDOW_TILE_DATA) != 0;
        ushort tileDataBase = unsignedTileIndexing ? (ushort)0x8000 : (ushort)0x9000;
        
        ushort bgMapBase = (lcdc & LCDC_BG_MAP) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
        
        int yPos = (ly + scy) & 0xFF;
        
        int tileRow = yPos / 8;
        
        int yOffset = yPos % 8;
        
        for (int x = 0; x < SCREEN_WIDTH; x++)
        {
            int xPos = (x + scx) & 0xFF;
            
            int tileCol = xPos / 8;
            
            int xOffset = xPos % 8;
            
            int mapAddress = bgMapBase + tileRow * 32 + tileCol;
            int tileIndex = vram[mapAddress - 0x8000];
            
            if (!unsignedTileIndexing)
            {
                if (tileIndex > 127)
                {
                    tileIndex -= 256;
                }
            }
            
            ushort tileAddress;
            if (unsignedTileIndexing)
            {
                tileAddress = (ushort)(tileDataBase + tileIndex * 16);
            }
            else
            {
                tileAddress = (ushort)(tileDataBase + (tileIndex + 128) * 16);
            }
            
            ushort pixelRowAddress = (ushort)(tileAddress + yOffset * 2);
            
            byte pixelDataLow = vram[pixelRowAddress - 0x8000];
            byte pixelDataHigh = vram[(pixelRowAddress + 1) - 0x8000];
            
            int colorBit = 7 - xOffset;
            int colorValue = ((pixelDataHigh >> colorBit) & 0x01) << 1 | ((pixelDataLow >> colorBit) & 0x01);
            
            int mappedColor = (bgp >> (colorValue * 2)) & 0x03;
            
            frameBuffer[x, ly] = (byte)mappedColor;
        }
    }

    private void RenderWindow()
    {
        if (wx > 166) return; 
        
        bool unsignedTileIndexing = (lcdc & LCDC_BG_WINDOW_TILE_DATA) != 0;
        ushort tileDataBase = unsignedTileIndexing ? (ushort)0x8000 : (ushort)0x9000;
        
        ushort windowMapBase = (lcdc & LCDC_WINDOW_MAP) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
        
        int windowY = ly - wy;
        
        int tileRow = windowY / 8;
        
        int yOffset = windowY % 8;
        
        for (int x = 0; x < SCREEN_WIDTH; x++)
        {
            int windowX = x - (wx - 7);
            if (windowX < 0) continue;
            
            int tileCol = windowX / 8;
            
            int xOffset = windowX % 8;
            
            int mapAddress = windowMapBase + tileRow * 32 + tileCol;
            int tileIndex = vram[mapAddress - 0x8000];
            
            if (!unsignedTileIndexing)
            {
                if (tileIndex > 127)
                {
                    tileIndex -= 256;
                }
            }
            
            ushort tileAddress;
            if (unsignedTileIndexing)
            {
                tileAddress = (ushort)(tileDataBase + tileIndex * 16);
            }
            else
            {
                tileAddress = (ushort)(tileDataBase + (tileIndex + 128) * 16);
            }
            
            ushort pixelRowAddress = (ushort)(tileAddress + yOffset * 2);
            
            byte pixelDataLow = vram[pixelRowAddress - 0x8000];
            byte pixelDataHigh = vram[(pixelRowAddress + 1) - 0x8000];
            
            int colorBit = 7 - xOffset;
            int colorValue = ((pixelDataHigh >> colorBit) & 0x01) << 1 | ((pixelDataLow >> colorBit) & 0x01);
            
            int mappedColor = (bgp >> (colorValue * 2)) & 0x03;
            
            frameBuffer[x, ly] = (byte)mappedColor;
        }
    }

    private void RenderSprites()
    {
        int spriteHeight = (lcdc & LCDC_OBJ_SIZE) != 0 ? 16 : 8;
        bool tall = spriteHeight == 16;
        
        int spritesDrawn = 0;
        
        var visibleSprites = new List<(int index, int x)>();
        
        for (int i = 0; i < 40; i++)
        {
            int offset = i * 4;
            int spriteY = oam[offset] - 16;
            int spriteX = oam[offset + 1] - 8;
            
            if (ly >= spriteY && ly < spriteY + spriteHeight)
            {
                visibleSprites.Add((i, spriteX));
                
                spritesDrawn++;
                if (spritesDrawn >= 10) break;
            }
        }
        
        visibleSprites = visibleSprites.OrderBy(s => s.x).ToList();
        
        foreach (var sprite in visibleSprites)
        {
            int offset = sprite.index * 4;
            
            int spriteY = oam[offset] - 16;
            int spriteX = oam[offset + 1] - 8;
            byte tileIndex = oam[offset + 2];
            byte attributes = oam[offset + 3];
            
            bool behindBG = (attributes & 0x80) != 0;
            bool yFlip = (attributes & 0x40) != 0;
            bool xFlip = (attributes & 0x20) != 0;
            bool usePalette1 = (attributes & 0x10) != 0;
            
            if (tall)
            {
                tileIndex &= 0xFE;
            }
            
            int row = ly - spriteY;
            
            if (yFlip)
            {
                row = spriteHeight - 1 - row;
            }
            
            byte effectiveTileIndex = tileIndex;
            if (tall && row >= 8)
            {
                effectiveTileIndex++;
                row -= 8;
            }
            
            ushort tileAddress = (ushort)(0x8000 + effectiveTileIndex * 16);
            
            ushort pixelRowAddress = (ushort)(tileAddress + row * 2);
            byte pixelDataLow = vram[pixelRowAddress - 0x8000];
            byte pixelDataHigh = vram[(pixelRowAddress + 1) - 0x8000];
            
            byte palette = usePalette1 ? obp1 : obp0;
            
            for (int x = 0; x < 8; x++)
            {
                if (spriteX + x < 0 || spriteX + x >= SCREEN_WIDTH) continue;
                
                int bitPos = xFlip ? x : (7 - x);
                
                int colorValue = ((pixelDataHigh >> bitPos) & 0x01) << 1 | ((pixelDataLow >> bitPos) & 0x01);
                
                if (colorValue == 0) continue;
                
                if (behindBG && frameBuffer[spriteX + x, ly] != 0) continue;
                
                int mappedColor = (palette >> (colorValue * 2)) & 0x03;
                
                frameBuffer[spriteX + x, ly] = (byte)mappedColor;
            }
        }
    }

    public byte ReadVRAM(ushort address)
    {
        return vram[address - 0x8000];
    }

    public void WriteVRAM(ushort address, byte value)
    {
        vram[address - 0x8000] = value;
        
        if (address >= 0x8000 && address < 0x9800)
        {
            UpdateTileData(address);
        }
    }

    public byte ReadOAM(ushort address)
    {
        return oam[address - 0xFE00];
    }

    public void WriteOAM(ushort address, byte value)
    {
        oam[address - 0xFE00] = value;
    }

    public byte ReadRegister(ushort address)
    {
        switch (address)
        {
            case 0xFF40: return lcdc;
            case 0xFF41: return stat;
            case 0xFF42: return scy;
            case 0xFF43: return scx;
            case 0xFF44: return (byte)(Program.UseGameboyDoctor ? 0x90 : ly);
            case 0xFF45: return lyc;
            case 0xFF47: return bgp;
            case 0xFF48: return obp0;
            case 0xFF49: return obp1;
            case 0xFF4A: return wy;
            case 0xFF4B: return wx;
            default: return 0xFF; 
        }
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF40:
                bool lcdWasOn = (lcdc & LCDC_DISPLAY_ENABLE) != 0;
                bool lcdNowOn = (value & LCDC_DISPLAY_ENABLE) != 0;
                
                if (lcdWasOn && !lcdNowOn)
                {
                    ly = 0;
                    modeClock = 0;
                    mode = MODE_HBLANK;
                    stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                }
                else if (!lcdWasOn && lcdNowOn)
                {
                    modeClock = 0;
                }
                
                lcdc = value;
                break;
                
            case 0xFF41:
                stat = (byte)((value & 0xF8) | (stat & 0x07));
                break;
                
            case 0xFF42: scy = value; break;
            case 0xFF43: scx = value; break;
            case 0xFF44: break;
            case 0xFF45: lyc = value; CheckLYCoincidence(); break;
            case 0xFF47: bgp = value; break;
            case 0xFF48: obp0 = value; break;
            case 0xFF49: obp1 = value; break;
            case 0xFF4A: wy = value; break;
            case 0xFF4B: wx = value; break;
        }
    }

    public void DoDMATransfer(byte value)
    {
        ushort sourceAddress = (ushort)(value << 8);
        
        for (int i = 0; i < 0xA0; i++)
        {
            oam[i] = memoryBus.ReadByte((ushort)(sourceAddress + i));
        }
    }

    private void UpdateTileData(ushort address)
    {
        int relativeTileAddress = address - 0x8000;
        int tileIndex = relativeTileAddress / 16;
        int tileRow = (relativeTileAddress % 16) / 2;
        
        if (tileIndex >= 384) return;
        
        byte lowByte = vram[tileIndex * 16 + tileRow * 2];
        byte highByte = vram[tileIndex * 16 + tileRow * 2 + 1];
        
        for (int x = 0; x < 8; x++)
        {
            int bit = 7 - x;
            int colorValue = ((highByte >> bit) & 0x01) << 1 | ((lowByte >> bit) & 0x01);
            tileData[tileIndex, tileRow * 8 + x] = (byte)colorValue;
        }
    }
}