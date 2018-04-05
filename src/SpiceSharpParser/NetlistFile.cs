namespace SpiceSharpParser
{
    public class NetlistFile
    {
        public NetlistFile(string fileNameWithExtension, string directoryPath, string content)
        {
            Content = content;
            FileNameWithExtension = fileNameWithExtension;
            DirectoryPath = directoryPath;
        }

        public string Content { get; }

        public string DirectoryPath { get; }

        public string FileNameWithExtension { get; }
    }
}
