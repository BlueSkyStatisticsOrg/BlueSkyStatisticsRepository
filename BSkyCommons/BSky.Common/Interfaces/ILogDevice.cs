
namespace BSky.Statistics.Common
{
    public interface ILogDevice
    {
        void WriteLine(string text);
        void WriteUserCommands(string text);//12Aug2016
        string FileName { get; set; } //Full path filename
        int MaxFileSize { get; set; } //ad. Maximum file size for each file.
        int MaxBackupFiles { get; set; } //ad. Maximum numbers of backup files.
    }
}
