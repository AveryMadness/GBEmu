// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Image = SFML.Graphics.Image;

namespace GBEmu;

public class Program
{
    public static MemoryBus MemoryBus;
    public static GPU gpu;
    private static RenderWindow window;
    private static Texture texture;
    private static Sprite sprite;
    private static Image frameImage;
    private static bool running = true;
    
    public const int CPU_CYCLES_PER_FRAME = 70224;
    private const double ClockSpeed = 4194304.0;

    public const bool UseGameboyDoctor = true;

    public static void Main(string[] args)
    {
        MainAsync(args);
    }
    
    public static async void MainAsync(string[] args)
    {
        FileStream fileStream = new FileStream("cpu_instrs.gb", FileMode.Open);
        
        fileStream.Seek(0x147, SeekOrigin.Begin);
        byte[] cartTypeBuffer = new byte[1]; 
        fileStream.Read(cartTypeBuffer, 0, 1);
        
        byte type = cartTypeBuffer[0];
        CartridgeType cartridgeType = (CartridgeType)type;
        
        Console.WriteLine($"Cartridge Type: {cartridgeType}");
        fileStream.Flush();
        fileStream.Close();

        Cartridge cartridge = new Cartridge(File.ReadAllBytes("cpu_instrs.gb"), cartridgeType);
        gpu = new GPU();
        InputController inputController = new InputController();

        byte[] bootRom = File.ReadAllBytes("dmg_boot.bin");

        MemoryBus memoryBus = new MemoryBus(bootRom, cartridge, gpu, inputController);
        gpu.SetMemoryBus(memoryBus);
        
        window = new RenderWindow(new VideoMode(160, 144), "GBEmu");
        window.Closed += (sender, e) => running = false;

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

        SM83.MemoryBus = memoryBus;
        SM83.Registers.Reset();
        SM83.Stack = new Stack(memoryBus);

        if (UseGameboyDoctor)
        {
            GameboyDoctor.SetupRegisters();
        }
        
        while (running)
        {
            window.DispatchEvents();
            SM83.Cycles = 0;
            await RunFrame();
        }

        window.Close();
    }

    public static async Task RunFrame()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int cyclesThisFrame = 0;
        
        while (cyclesThisFrame < CPU_CYCLES_PER_FRAME)
        {
            int previousCycles = SM83.Cycles;
            SM83.HandleInterrupts();
            await SM83.ExecuteNextInstruction();
            int elapsedCycles = SM83.Cycles - previousCycles;
            gpu.Step(elapsedCycles);
            cyclesThisFrame += elapsedCycles;
        }

        double targetFrameTime = 1000.0 / 59.7;

        while (stopwatch.Elapsed.TotalMilliseconds < targetFrameTime)
        {
            Thread.SpinWait(100);
        }
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