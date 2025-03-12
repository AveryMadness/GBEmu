namespace GBEmu;

public static class GameboyDoctor
{
    public static bool BlockLCDLY = false;
    public static string LogStack = "";
    public static void SetupRegisters()
    {
        SM83.Registers.A = 0x01;
        SM83.Registers.F = 0xB0;
        SM83.Registers.B = 0x00;
        SM83.Registers.C = 0x13;
        SM83.Registers.D = 0x00;
        SM83.Registers.E = 0xD8;
        SM83.Registers.H = 0x01;
        SM83.Registers.L = 0x4D;
        SM83.Stack.SP = 0xFFFE;
        SM83.ProgramCounter = 0x100;
        SM83.MemoryBus.useBootRom = false;
        File.WriteAllText("gbemu.log", "");
    }

    public static async Task WriteLogAfterInstruction()
    {
        
    }
}