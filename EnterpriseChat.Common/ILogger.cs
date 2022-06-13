namespace EnterpriseChat.Common
{
    public interface ILogger
    {
        void WriteLine();
        void WriteLine(string format, object? arg0);
        void WriteLine(string format, object? arg0, object? arg1);
        void WriteLine(string format, object? arg0, object? arg1, object? arg2);
        void WriteLine(string format, params object?[] arg);
        void WriteLine(string? value);
    }
}