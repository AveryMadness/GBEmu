using System.Threading.Channels;

namespace GBEmu;

public static class AsyncLogger
{
    private static readonly Channel<string> _logChannel = Channel.CreateUnbounded<string>();
    private static readonly CancellationTokenSource _cts = new();
    private static readonly Task _logTask;

    static AsyncLogger()
    {
        _logTask = Task.Run(async () =>
        {
            await foreach (var log in _logChannel.Reader.ReadAllAsync(_cts.Token))
            {
                await WriteLogAsync(log);
            }
        });
    }

    public static void Log(string message)
    {
        _logChannel.Writer.TryWrite(message);
    }

    private static async Task WriteLogAsync(string message)
    {
        string logMessage = $"{DateTime.UtcNow:HH:mm:ss.fff} | {message}";
        Console.WriteLine(logMessage);

        // Optionally, log to a file asynchronously
        await File.AppendAllTextAsync("log.txt", logMessage + Environment.NewLine);
    }

    public static void StopLogging()
    {
        _cts.Cancel();
    }
}