namespace student_app.Logging
{
    public class ActionCallFileLoggingOptions
    {
        public bool Enabled { get; set; } = true;

        public string Directory { get; set; } = "Logs";

        public string FileNamePrefix { get; set; } = "function-calls";

        public int MaxFileSizeMb { get; set; } = 20;

        public int MaxFileCount { get; set; } = 10;

        public int MaxPayloadLength { get; set; } = 10000;
    }
}
