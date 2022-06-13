namespace EnterpriseChat.Common
{
    public class Logger : ILogger
    {
        private readonly string _name;
        private readonly TextWriter _textWriter;

        public Logger(string name, TextWriter textWriter)
        {
            _name = name;
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        }

        public void WriteLine() => _textWriter.WriteLine();
        public void WriteLine(string? value) => _textWriter.WriteLine("{0}{1}", _name, value);
        public void WriteLine(string format, object? arg0) => _textWriter.WriteLine(_name + format, arg0);
        public void WriteLine(string format, object? arg0, object? arg1) => _textWriter.WriteLine(_name + format, arg0, arg1);
        public void WriteLine(string format, object? arg0, object? arg1, object? arg2) => _textWriter.WriteLine(_name + format, arg0, arg1, arg2);
        public void WriteLine(string format, params object?[] arg) => _textWriter.WriteLine(_name + format, arg);

    }
}
