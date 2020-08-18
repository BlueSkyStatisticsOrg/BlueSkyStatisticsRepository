using BlueSky.CommandBase;
using BSky.Lifetime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueSky.Commands.Help
{
    class RFunctionHelpCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        protected override void OnExecute(object param)
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            RFunctionHelp RpkgHelp = new RFunctionHelp();
            RpkgHelp.Owner = window;
            RpkgHelp.ShowDialog();

        }

        protected override void OnPostExecute(object param)
        {
        }
    }
}
