
using BlueSky.CommandBase;

namespace BlueSky.Commands.Help
{
    class BlueSkyWebsiteCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {

        }

        protected override void OnExecute(object param)
        {
            System.Diagnostics.Process.Start("www.blueskystatistics.com");
        }

        protected override void OnPostExecute(object param)
        {

        }
    }
}
