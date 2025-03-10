using System.Text;
using System.Threading.Channels;

namespace GBEmu;

public class AsyncLogger : IDisposable
{
    private readonly Channel<string> _logChannel;
    private readonly StreamWriter _writer;
    private readonly Task _logProcessor;
    private bool _disposed;
    public static AsyncLogger asyncLogger = new AsyncLogger("logs/gbemu.log");
    
    public AsyncLogger(string filePath)
    {
        _logChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });
        _writer = new StreamWriter(filePath, append: true, Encoding.UTF8) { AutoFlush = true };
        _logProcessor = Task.Run(ProcessLogQueue);
    }
    
    public void Log(string message)
    {
        if (!_logChannel.Writer.TryWrite($"{DateTime.UtcNow:O} {message}"))
        {
            Console.WriteLine("Logger is full, dropping message");
        }
    }
    
    private async Task ProcessLogQueue()
    {
        await foreach (var logEntry in _logChannel.Reader.ReadAllAsync())
        {
            await _writer.WriteLineAsync(logEntry);
        }
    }
    
    public async Task FlushAsync()
    {
        _logChannel.Writer.Complete();
        await _logProcessor;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            FlushAsync().GetAwaiter().GetResult();
            _writer.Dispose();
        }
    }
}