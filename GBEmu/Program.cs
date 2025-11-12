// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Image = SFML.Graphics.Image;

namespace GBEmu;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct OpenFileName
{
    public int lStructSize;
    public IntPtr hwndOwner;
    public IntPtr hInstance;
    public string lpstrFilter;
    public string lpstrCustomFilter;
    public int nMaxCustFilter;
    public int nFilterIndex;
    public string lpstrFile;
    public int nMaxFile;
    public string lpstrFileTitle;
    public int nMaxFileTitle;
    public string lpstrInitialDir;
    public string lpstrTitle;
    public int Flags;
    public short nFileOffset;
    public short nFileExtension;
    public string lpstrDefExt;
    public IntPtr lCustData;
    public IntPtr lpfnHook;
    public string lpTemplateName;
    public IntPtr pvReserved;
    public int dwReserved;
    public int flagsEx;
}


public class Program
{
    public static MemoryBus MemoryBus;
    public static PPU Ppu;
    public static APU Apu;
    private static RenderWindow window;
    private static Texture texture;
    private static Sprite sprite;
    private static Image frameImage;
    private static Font debugFont;
    public static Text debugText;
    private static bool running = true;
    
    public const int CPU_CYCLES_PER_FRAME = 70224;
    public static int[] timerFrequencies = { 1024, 16, 64, 256 };


    public const bool UseGameboyDoctor = false;
    public const bool SkipBoot = false;
    
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    private static string ShowDialog()
    {
        var ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = "GameBoy ROMS (*.gb)\0*.gb\0All Files (*.*)\0*.*\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "Select ROM";
        if (GetOpenFileName(ref ofn))
            return ofn.lpstrFile;
        return string.Empty;
    }

    public static void Main(string[] args)
    {
        MainAsync(args);
    }
    
    public static void MainAsync(string[] args)
    {
        string fileName = ShowDialog();
        FileStream fileStream = new FileStream(fileName, FileMode.Open);
        
        fileStream.Seek(0x147, SeekOrigin.Begin);
        byte[] cartTypeBuffer = new byte[1]; 
        fileStream.Read(cartTypeBuffer, 0, 1);
        
        byte type = cartTypeBuffer[0];
        CartridgeType cartridgeType = (CartridgeType)type;
        
        Console.WriteLine($"Cartridge Type: {cartridgeType}");
        fileStream.Flush();
        fileStream.Close();
        
        string title = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(fileName), 0x0134, 16).TrimEnd('\0');

        Cartridge cartridge = new Cartridge(File.ReadAllBytes(fileName), cartridgeType);
        if (cartridge.HasSaveRam())
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + $"/{title}.sav"))
            {
                byte[] saveRam = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + $"/{title}.sav");
                cartridge.LoadSaveRam(saveRam);
            }
        }
        
        Ppu = new PPU();
        InputController inputController = new InputController();

        Apu = new APU();

        byte[] bootRom = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "/dmg_boot.bin");

        MemoryBus memoryBus = new MemoryBus(bootRom, cartridge, Ppu, inputController, Apu);
        Ppu.SetMemoryBus(memoryBus);
        
        SerialMonitor.StartSerialMonitor();
        
        window = new RenderWindow(new VideoMode(160, 144), "GBEmu");
        window.SetView(new View(new FloatRect(0, 0, 160, 144)));
        window.Closed += (sender, e) =>
        {
            running = false;
            cartridge.SaveRam();
        };

        window.KeyPressed += (sender, e) =>
        {
            if (e.Code == Keyboard.Key.Z)
            {
                inputController.PressButton(GameBoyButton.A);
            }
            
            if (e.Code == Keyboard.Key.X)
            {
                inputController.PressButton(GameBoyButton.B);
            }

            if (e.Code == Keyboard.Key.Enter)
            {
                inputController.PressButton(GameBoyButton.Start);
            }
            
            if (e.Code == Keyboard.Key.RShift)
            {
                inputController.PressButton(GameBoyButton.Select);
            }
            
            if (e.Code == Keyboard.Key.Up)
            {
                inputController.PressButton(GameBoyButton.Up);
            }
            
            if (e.Code == Keyboard.Key.Down)
            {
                inputController.PressButton(GameBoyButton.Down);
            }
            
            if (e.Code == Keyboard.Key.Left)
            {
                inputController.PressButton(GameBoyButton.Left);
            }
            
            if (e.Code == Keyboard.Key.Right)
            {
                inputController.PressButton(GameBoyButton.Right);
            }
        };
        
        window.KeyReleased += (sender, e) =>
        {
            if (e.Code == Keyboard.Key.Z)
            {
                inputController.ReleaseButton(GameBoyButton.A);
            }
            
            if (e.Code == Keyboard.Key.X)
            {
                inputController.ReleaseButton(GameBoyButton.B);
            }

            if (e.Code == Keyboard.Key.Enter)
            {
                inputController.ReleaseButton(GameBoyButton.Start);
            }
            
            if (e.Code == Keyboard.Key.RShift)
            {
                inputController.ReleaseButton(GameBoyButton.Select);
            }
            
            if (e.Code == Keyboard.Key.Up)
            {
                inputController.ReleaseButton(GameBoyButton.Up);
            }
            
            if (e.Code == Keyboard.Key.Down)
            {
                inputController.ReleaseButton(GameBoyButton.Down);
            }
            
            if (e.Code == Keyboard.Key.Left)
            {
                inputController.ReleaseButton(GameBoyButton.Left);
            }
            
            if (e.Code == Keyboard.Key.Right)
            {
                inputController.ReleaseButton(GameBoyButton.Right);
            }
        };

        frameImage = new Image((uint)160, (uint)144, SFML.Graphics.Color.White);
        texture = new Texture(160, 144);
        sprite = new Sprite(texture);
        sprite.Scale = new Vector2f(1f, 1f);
        debugFont = new Font(AppDomain.CurrentDomain.BaseDirectory + "/Bytesized-Regular.ttf");
        debugText = new Text("Input: 00000000", debugFont, 10)
        {
            FillColor = Color.Green,
            Position = new Vector2f(5, 5)
        };

        SM83.MemoryBus = memoryBus;
        SM83.Registers.Reset();
        SM83.Stack = new Stack(memoryBus);

        if (UseGameboyDoctor)
        {
            GameboyDoctor.SetupRegisters();
        }
        else if (SkipBoot)
        {
            SM83.MemoryBus.useBootRom = false;
            SM83.ProgramCounter = 0x100;
        }
        
        while (running)
        {
            window.DispatchEvents();
            SM83.Cycles = 0;
            RunFrame();
        }

        window.Close();
    }
    
    private static void HandleSerialTransfer()
    {
        byte sc = SM83.MemoryBus.ReadByte(0xFF02);

        if ((sc & 0x80) != 0) // If transfer start bit (bit 7) is set
        {
            byte data = SM83.MemoryBus.ReadByte(0xFF01); // Read SB register
            SerialMonitor.LogSerialData(data); // Log it to monitor

            // Simulate transfer completion
            SM83.MemoryBus.WriteByte(0xFF02, (byte)(sc & 0x7F)); // Clear transfer bit
        }
    }


    private static int divCycles = 0;
    private static int timerCycles = 0;
    public static void RunFrame()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int cyclesThisFrame = 0;
        
        while (cyclesThisFrame < CPU_CYCLES_PER_FRAME)
        {
            int previousCycles = SM83.Cycles;
            if (SM83.IsHalted)
            {
                if ((SM83.IF & SM83.IE) != 0 || SM83.IF != 0)
                {
                    SM83.IsHalted = false;
                }
                else
                {
                    SM83.Cycles += 4;
                }
            }
            else
            {
                SM83.ExecuteNextInstruction();
                SM83.HandleInterrupts();
            }
            
            int elapsedCycles = SM83.Cycles - previousCycles;
            Ppu.Step(elapsedCycles);
            Apu.Step(elapsedCycles);
            
            divCycles += elapsedCycles;
            bool timerEnabled = (SM83.TAC & 0b00000100) != 0;

            if (timerEnabled)
            {
                int clockSelect = SM83.TAC & 0b00000011;
                int timerThreshold = timerFrequencies[clockSelect];

                timerCycles += elapsedCycles;

                while (timerCycles >= timerThreshold)
                {
                    timerCycles -= timerThreshold;

                    if (SM83.TIMA == 0xFF)
                    {
                        SM83.TIMA = SM83.TMA;
                        SM83.RequestInterrupt(SM83.InterruptFlags.Timer);
                    }
                    else
                    {
                        SM83.TIMA++;
                    }
                }
            }

            if (divCycles >= 256)
            {
                divCycles -= 256;
                MemoryBus.AllowDivWrite = true;
                SM83.DIV++;
                MemoryBus.AllowDivWrite = false;
            }
            
            cyclesThisFrame += elapsedCycles;
        }

        double targetFrameTime = 1000.0 / 59.7;

        while (stopwatch.Elapsed.TotalMilliseconds < targetFrameTime)
        {
            Thread.SpinWait(100);
        }
        
        Apu.DebugChannelStatus();
    }

    public static void RenderFrame(byte[,] frameBuffer)
    {
        for (int y = 0; y < 144; y++)
        {
            for (int x = 0; x < 160; x++)
            {
                byte color = frameBuffer[x, y];
                SFML.Graphics.Color pixelColor = color switch
                {
                    0 => new SFML.Graphics.Color(255, 255, 255),
                    1 => new SFML.Graphics.Color(192, 192, 192),
                    2 => new SFML.Graphics.Color(96, 96, 96),
                    _ => new SFML.Graphics.Color(0, 0, 0)
                };
                frameImage.SetPixel((uint)x, (uint)y, pixelColor);
            }
        }
        
        texture.Update(frameImage);
        window.Clear();
        window.Draw(sprite);
        window.Display();
    }
}