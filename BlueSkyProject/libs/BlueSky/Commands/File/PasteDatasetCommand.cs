using BlueSky.CommandBase;
using BlueSky.Services;
using BSky.Interfaces.Interfaces;
using BSky.Lifetime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueSky.Commands.File
{
    class PasteDatasetCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        protected override void OnExecute(object param)
        {
            PasteClipboardDataset pasteds = new PasteClipboardDataset();
            pasteds.PasteDatasetFromClipboard(null, true);
        }

        protected override void OnPostExecute(object param)
        {
        }
    }
}
