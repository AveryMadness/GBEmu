using System.Runtime.InteropServices;

namespace GBEmu;

public class SerialMonitor
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    private static StreamWriter consoleWriter;

    public static void StartSerialMonitor()
    {
        AllocConsole(); // Create a new console window
        consoleWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(consoleWriter);
        Console.Title = "Serial Monitor";
        Console.WriteLine("Serial Monitor Started...\n");
    }

    public static void LogSerialData(byte data)
    {
        char receivedChar = (char)data;
        Console.Write(receivedChar); // Print character to new console
    }
}