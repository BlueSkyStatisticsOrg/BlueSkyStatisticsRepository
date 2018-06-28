
namespace BSky.Statistics.Common
{
    public class UAError
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }
        public string StackTrace { get; set; }

        public UAError() { }
        public UAError(string Message) { this.Message = Message; }
    }
}
