using System.Numerics;
using System.Text;

namespace GBEmu;

public class PPU
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

    public PPU()
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
    // If LCD is disabled, just reset states and return
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
            // OAM search takes 80 cycles
            if (modeClock >= OAM_SCAN_CYCLES)
            {
                modeClock -= OAM_SCAN_CYCLES;
                mode = MODE_TRANSFER;
                stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
            }
            break;
            
        case MODE_TRANSFER:
            // Pixel transfer takes 172-289 cycles (use 172 for simplicity)
            if (modeClock >= PIXEL_TRANSFER_CYCLES)
            {
                modeClock -= PIXEL_TRANSFER_CYCLES;
                mode = MODE_HBLANK;
                stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                
                // Line is rendered at the beginning of HBlank
                RenderScanline();
                
                // Check for STAT interrupt
                if ((stat & STAT_HBLANK_INTERRUPT) != 0)
                {
                    SM83.RequestInterrupt(SM83.InterruptFlags.LCDStat);
                }
            }
            break;
            
        case MODE_HBLANK:
            // HBlank lasts until the end of the scanline (456 cycles total per line)
            if (modeClock >= HBLANK_CYCLES)
            {
                modeClock -= HBLANK_CYCLES;
                ly++;
                
                CheckLYCoincidence();
                
                if (ly >= SCREEN_HEIGHT)
                {
                    // Enter VBlank when all visible lines are done
                    mode = MODE_VBLANK;
                    stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                    
                    // Render the complete frame
                    Program.RenderFrame(frameBuffer);
                    
                    // Trigger VBlank interrupt
                    SM83.RequestInterrupt(SM83.InterruptFlags.VBlank);
                    
                    // Also trigger STAT interrupt if enabled
                    if ((stat & STAT_VBLANK_INTERRUPT) != 0)
                    {
                        SM83.RequestInterrupt(SM83.InterruptFlags.LCDStat);
                    }
                }
                else
                {
                    // Move to OAM scan for the next line
                    mode = MODE_OAM;
                    stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                    
                    // Trigger STAT interrupt if enabled
                    if ((stat & STAT_OAM_INTERRUPT) != 0)
                    {
                        SM83.RequestInterrupt(SM83.InterruptFlags.LCDStat);
                    }
                }
            }
            break;
            
        case MODE_VBLANK:
            // Each scanline in VBlank still takes 456 cycles
            if (modeClock >= LINE_CYCLES)
            {
                modeClock -= LINE_CYCLES;
                ly++;
                
                CheckLYCoincidence();
                
                // VBlank lasts for 10 lines (144-153)
                if (ly >= SCREEN_HEIGHT + VBLANK_LINES)
                {
                    // Start a new frame
                    mode = MODE_OAM;
                    stat = (byte)((stat & ~STAT_MODE_FLAG) | mode);
                    ly = 0;
                    
                    CheckLYCoincidence();
                    
                    // Trigger STAT interrupt if enabled
                    if ((stat & STAT_OAM_INTERRUPT) != 0)
                    {
                        SM83.RequestInterrupt(SM83.InterruptFlags.LCDStat);
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
        // Only request the interrupt if it's enabled in the IE register
        if ((memoryBus.ReadByte(0xFFFF) & 0x02) != 0)
        {
            SM83.RequestInterrupt(SM83.InterruptFlags.LCDStat);
        }
    }

    private void RequestVBlankInterrupt()
    {
        // Only request the interrupt if it's enabled in the IE register
        if ((memoryBus.ReadByte(0xFFFF) & 0x01) != 0)
        {
            SM83.RequestInterrupt(SM83.InterruptFlags.VBlank);
        }
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
    
    public byte[,] GetTileDataDebug(int startTile, int endTile)
    {
        if (startTile < 0) startTile = 0;
        if (endTile > 384) endTile = 384;
    
        int tilesWide = 16;
        int tilesPerRow = Math.Min(tilesWide, endTile - startTile);
        int rows = (int)Math.Ceiling((double)(endTile - startTile) / tilesWide);
    
        byte[,] tileImage = new byte[tilesPerRow * 8, rows * 8];
    
        for (int tileIndex = startTile; tileIndex < endTile; tileIndex++)
        {
            int tileOffset = tileIndex - startTile;
            int tileX = (tileOffset % tilesWide) * 8;
            int tileY = (tileOffset / tilesWide) * 8;
        
            for (int y = 0; y < 8; y++)
            {
                ushort tileAddress = (ushort)(0x8000 + tileIndex * 16);
                ushort rowAddress = (ushort)(tileAddress + y * 2);
            
                byte lowByte = vram[rowAddress - 0x8000];
                byte highByte = vram[rowAddress + 1 - 0x8000];
            
                for (int x = 0; x < 8; x++)
                {
                    int bitPos = 7 - x;
                    int colorValue = ((highByte >> bitPos) & 0x01) << 1 | ((lowByte >> bitPos) & 0x01);
                
                    if (tileX + x < tileImage.GetLength(0) && tileY + y < tileImage.GetLength(1))
                    {
                        tileImage[tileX + x, tileY + y] = (byte)colorValue;
                    }
                }
            }
        }
    
        return tileImage;
    }

    private void RenderBackground()
    {
        if ((lcdc & LCDC_BG_DISPLAY) == 0)
        {
            // If background is disabled, fill with color 0
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                frameBuffer[x, ly] = 0;
            }
            return;
        }
    
        bool unsignedTileIndexing = (lcdc & LCDC_BG_WINDOW_TILE_DATA) != 0;
        ushort tileDataBase = unsignedTileIndexing ? (ushort)0x8000 : (ushort)0x9000;
    
        ushort bgMapBase = (lcdc & LCDC_BG_MAP) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
    
        int yPos = (ly + scy) & 0xFF;  // Ensure proper wrapping at 256 pixels
    
        for (int x = 0; x < SCREEN_WIDTH; x++)
        {
            int xPos = (x + scx) & 0xFF;  // Ensure proper wrapping at 256 pixels
        
            byte pixelColor = GetTilePixel(
                bgMapBase,
                tileDataBase,
                unsignedTileIndexing,
                xPos,
                yPos,
                false
            );
        
            frameBuffer[x, ly] = pixelColor;
        }
    }

    private void RenderWindow()
    {
        // Check if window is enabled and visible on this scanline
        if ((lcdc & LCDC_WINDOW_ENABLE) == 0 || ly < wy || wx > 166)
        {
            return;
        }
    
        bool unsignedTileIndexing = (lcdc & LCDC_BG_WINDOW_TILE_DATA) != 0;
        ushort tileDataBase = unsignedTileIndexing ? (ushort)0x8000 : (ushort)0x9000;
        ushort windowMapBase = (lcdc & LCDC_WINDOW_MAP) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
    
        // Calculate window Y position (relative to window origin)
        int windowY = ly - wy;
    
        // For each screen x-coordinate
        for (int x = 0; x < SCREEN_WIDTH; x++)
        {
            // Check if this point is within the window area
            // wx - 7 is the actual left edge of the window
            if (x >= (wx - 7))
            {
                // Calculate the x position relative to window origin
                int windowX = x - (wx - 7);
            
                // Get the pixel color from the window
                byte pixelColor = GetTilePixel(
                    windowMapBase,
                    tileDataBase,
                    unsignedTileIndexing,
                    windowX,
                    windowY,
                    true
                );
            
                // Draw the pixel
                frameBuffer[x, ly] = pixelColor;
            }
        }
    }
    
    private byte GetTilePixel(ushort mapBase, ushort tileDataBase, bool unsignedTileIndexing, 
        int pixelX, int pixelY, bool isWindow)
    {
        int tileCol = pixelX / 8;
        int tileRow = pixelY / 8;
    
        int xOffset = pixelX % 8;
        int yOffset = pixelY % 8;
    
        // Ensure we don't access out of bounds
        if (tileRow >= 32 || tileCol >= 32) return 0;
    
        // Get the tile index from the tile map
        ushort mapAddress = (ushort)(mapBase + (tileRow * 32 + tileCol));
        int tileIndex = vram[mapAddress - 0x8000];
    
        // Calculate the actual tile address based on addressing mode
        ushort tileAddress;
        if (unsignedTileIndexing)
        {
            // 8000 addressing mode (tiles 0-255)
            tileAddress = (ushort)(0x8000 + tileIndex * 16);
        }
        else
        {
            // 8800 addressing mode (tiles -128 to 127)
            // In this mode, tile index is treated as signed
            tileAddress = (ushort)(0x9000 + ((sbyte)tileIndex) * 16);
        }
    
        // Calculate address of the specific pixel row within the tile
        ushort pixelRowAddress = (ushort)(tileAddress + yOffset * 2);
    
        // Read the two bytes that form the pixel row
        byte lowByte = vram[pixelRowAddress - 0x8000];
        byte highByte = vram[pixelRowAddress + 1 - 0x8000];
    
        // Extract the specific pixel's color value (2 bits)
        int bitPosition = 7 - xOffset;  // Pixels are stored MSB first
        int colorValue = ((highByte >> bitPosition) & 0x01) << 1 | ((lowByte >> bitPosition) & 0x01);
    
        // Map the color value through the background palette
        int mappedColor = (bgp >> (colorValue * 2)) & 0x03;
    
        return (byte)mappedColor;
    }

    private void RenderSprites()
{
    // Skip if sprites are disabled
    if ((lcdc & LCDC_OBJ_ENABLE) == 0) return;
    
    int spriteHeight = (lcdc & LCDC_OBJ_SIZE) != 0 ? 16 : 8;
    
    // First, collect all sprites that are visible on this scanline
    // A Game Boy can display up to 40 sprites in total
    List<int> visibleSpriteIndices = new List<int>();
    
    for (int i = 0; i < 40; i++)
    {
        int oamAddress = i * 4;
        int spriteY = oam[oamAddress] - 16;  // Y position is offset by 16
        
        // Check if sprite is on this scanline
        if (ly >= spriteY && ly < spriteY + spriteHeight && spriteY >= 0)
        {
            visibleSpriteIndices.Add(i);
            
            // Game Boy can only display 10 sprites per scanline
            if (visibleSpriteIndices.Count >= 10)
                break;
        }
    }
    
    // Process sprites in order from highest to lowest priority
    // On Game Boy, this is determined by X-coordinate (lower X has priority with ties going to lower OAM index)
    visibleSpriteIndices.Sort((a, b) =>
    {
        int xA = oam[a * 4 + 1];
        int xB = oam[b * 4 + 1];
        return xA == xB ? a.CompareTo(b) : xA.CompareTo(xB);
    });
    
    // Draw the sprites (from lowest to highest priority)
    for (int i = visibleSpriteIndices.Count - 1; i >= 0; i--)
    {
        int spriteIdx = visibleSpriteIndices[i];
        int oamAddress = spriteIdx * 4;
        
        int y = oam[oamAddress] - 16;        // Y position (offset by 16)
        int x = oam[oamAddress + 1] - 8;     // X position (offset by 8)
        byte tileIndex = oam[oamAddress + 2]; // Tile index
        byte attributes = oam[oamAddress + 3]; // Attributes
        
        // Extract sprite attributes
        bool behindBG = (attributes & 0x80) != 0;   // BG and Window over OBJ (0=No, 1=BG and Window colors 1-3 over the OBJ)
        bool yFlip = (attributes & 0x40) != 0;      // Y flip
        bool xFlip = (attributes & 0x20) != 0;      // X flip
        bool usePalette1 = (attributes & 0x10) != 0; // Palette number (0=OBP0, 1=OBP1)
        
        // For 8x16 sprites, the lower bit of the tile index is ignored
        if (spriteHeight == 16)
        {
            tileIndex &= 0xFE;
        }
        
        // Calculate which row of the sprite we're drawing
        int row = ly - y;
        if (yFlip)
        {
            row = spriteHeight - 1 - row;
        }
        
        // Get the correct tile row data
        int tileRow = row % 8;  // Which row within the current 8x8 tile
        
        // For 8x16 sprites, we might be in the second tile
        if (spriteHeight == 16 && row >= 8)
        {
            tileIndex++;  // Move to the next tile
        }
        
        // Calculate the address of the tile data
        ushort tileAddress = (ushort)(0x8000 + tileIndex * 16);
        ushort rowAddress = (ushort)(tileAddress + tileRow * 2);
        
        // Read the tile data for this row
        byte lowByte = vram[rowAddress - 0x8000];
        byte highByte = vram[(rowAddress + 1) - 0x8000];
        
        // Select the palette
        byte palette = usePalette1 ? obp1 : obp0;
        
        // Draw each pixel of the sprite row
        for (int pixelX = 0; pixelX < 8; pixelX++)
        {
            // Skip pixels that would be off-screen
            if (x + pixelX < 0 || x + pixelX >= SCREEN_WIDTH)
                continue;
            
            // Calculate which bit of the tile data to use (accounting for x-flip)
            int bitPos = xFlip ? pixelX : (7 - pixelX);
            
            // Get the color value (0-3) for this pixel
            int colorValue = ((highByte >> bitPos) & 0x01) << 1 | ((lowByte >> bitPos) & 0x01);
            
            // Color 0 is transparent for sprites
            if (colorValue == 0)
                continue;
            
            // Check sprite priority
            if (behindBG)
            {
                // If background priority bit is set, sprite goes behind non-zero BG colors
                byte bgPixel = frameBuffer[x + pixelX, ly];
                if (bgPixel != 0)  // If BG pixel is not color 0 (transparent)
                    continue;
            }
            
            // Apply the palette and draw the pixel
            int colorIndex = (palette >> (colorValue * 2)) & 0x03;
            frameBuffer[x + pixelX, ly] = (byte)colorIndex;
        }
    }
}

    private byte lastVRAMValue;

    public byte ReadVRAM(ushort address)
    {
        if (mode == MODE_TRANSFER) return lastVRAMValue; // Return last read value instead of 0xFF
        lastVRAMValue = vram[address - 0x8000];
        return lastVRAMValue;
    }

    public void WriteVRAM(ushort address, byte value)
    {
        if (mode == MODE_TRANSFER) return;
        vram[address - 0x8000] = value;
        
        if (address >= 0x8000 && address < 0x9800)
        {
            UpdateTileData(address);
        }
    }

    public byte ReadOAM(ushort address)
    {
        if (mode == MODE_OAM || mode == MODE_TRANSFER) return 0xFF;
        return oam[address - 0xFE00];
    }

    public void WriteOAM(ushort address, byte value)
    {
        if (mode == MODE_OAM || mode == MODE_TRANSFER) return;
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
        // Calculate which tile and row this address affects
        int relativeAddress = address - 0x8000;
        int tileIndex = relativeAddress / 16;
        int rowIndex = (relativeAddress % 16) / 2;
    
        if (tileIndex >= 384) return; // Out of range
    
        // Calculate the base address of this tile
        int tileBaseAddress = tileIndex * 16;
    
        // Get the two bytes for this row
        byte lowByte = vram[tileBaseAddress + rowIndex * 2];
        byte highByte = vram[tileBaseAddress + rowIndex * 2 + 1];
    
        // Update the 8 pixels in this row
        for (int x = 0; x < 8; x++)
        {
            int bitPos = 7 - x;
            int colorValue = ((highByte >> bitPos) & 0x01) << 1 | ((lowByte >> bitPos) & 0x01);
            tileData[tileIndex, rowIndex * 8 + x] = (byte)colorValue;
        }
    }
    
    public string DumpTileMapInfo(bool isWindow)
    {
        bool unsignedTileIndexing = (lcdc & LCDC_BG_WINDOW_TILE_DATA) != 0;
        ushort mapBase = isWindow ? 
            ((lcdc & LCDC_WINDOW_MAP) != 0 ? (ushort)0x9C00 : (ushort)0x9800) :
            ((lcdc & LCDC_BG_MAP) != 0 ? (ushort)0x9C00 : (ushort)0x9800);
    
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Tile Map Base: 0x{mapBase:X4}");
        sb.AppendLine($"Tile Data Mode: {(unsignedTileIndexing ? "8000 (unsigned)" : "8800 (signed)")}");
    
        // Dump the first few rows of the tile map
        for (int row = 0; row < 4; row++)
        {
            sb.Append($"Row {row}: ");
            for (int col = 0; col < 16; col++)
            {
                int mapAddress = mapBase + row * 32 + col;
                int tileIndex = vram[mapAddress - 0x8000];
                sb.Append($"{tileIndex:X2} ");
            }
            sb.AppendLine();
        }
    
        return sb.ToString();
    }
}