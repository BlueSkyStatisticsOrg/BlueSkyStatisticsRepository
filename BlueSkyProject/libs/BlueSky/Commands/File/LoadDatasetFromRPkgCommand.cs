using BlueSky.CommandBase;
using BSky.Lifetime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueSky.Commands.File
{
    class LoadDatasetFromRPkgCommand : BSkyCommandBase
    {
        protected override void OnPreExecute(object param)
        {
        }

        protected override void OnExecute(object param)
        {
            Window1 window = LifetimeService.Instance.Container.Resolve<Window1>();
            LoadDatasetFromRPkgWindow ldfrp = new LoadDatasetFromRPkgWindow();
            ldfrp.Owner=window;
            ldfrp.ShowDialog();

        }

        protected override void OnPostExecute(object param)
        {
        }
    }
}
