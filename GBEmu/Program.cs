﻿// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Raylib_cs;
using System.Numerics;
using SFML.Graphics;
using SFML.Window;
using Color = Raylib_cs.Color;
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

    public static void Main(string[] args)
    {
        MainAsync(args);
    }
    
    public static async void MainAsync(string[] args)
    {
        FileStream fileStream = new FileStream("rom.gb", FileMode.Open);
        
        fileStream.Seek(0x147, SeekOrigin.Begin);
        byte[] cartTypeBuffer = new byte[1]; 
        fileStream.Read(cartTypeBuffer, 0, 1);
        
        byte type = cartTypeBuffer[0];
        CartridgeType cartridgeType = (CartridgeType)type;
        
        Console.WriteLine($"Cartridge Type: {cartridgeType}");
        fileStream.Flush();
        fileStream.Close();

        Cartridge cartridge = new Cartridge(File.ReadAllBytes("rom.gb"), cartridgeType);
        gpu = new GPU();
        InputController inputController = new InputController();

        byte[] bootRom = File.ReadAllBytes("dmg_boot.bin");

        MemoryBus memoryBus = new MemoryBus(bootRom, cartridge, gpu, inputController);
        gpu.SetMemoryBus(memoryBus);
        
        window = new RenderWindow(new VideoMode(160, 144), "GBEmu");
        window.Closed += (sender, e) => running = false;

        frameImage = new Image((uint)160, (uint)144, SFML.Graphics.Color.White);
        texture = new Texture(160, 144);
        sprite = new Sprite(texture);

        SM83.MemoryBus = memoryBus;
        SM83.Registers.Reset();
        //SM83.ProgramCounter = 0x100;
        SM83.Stack = new Stack(memoryBus);
        
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
            await SM83.ExecuteNextInstruction();
            SM83.HandleInterrupts();
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