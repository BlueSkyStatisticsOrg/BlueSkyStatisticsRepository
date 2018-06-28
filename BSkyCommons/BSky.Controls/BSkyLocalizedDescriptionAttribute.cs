using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;

namespace BSky.Controls
{
    public class BSkyLocalizedDescriptionAttribute : DescriptionAttribute
    {
        readonly ResourceManager resrcMngr;
        readonly string resrcKey;

        public BSkyLocalizedDescriptionAttribute(string ResrcKey, Type ResrcType)
        {
            resrcMngr = new ResourceManager(ResrcType);
            resrcKey = ResrcKey;
        }

        public override string Description
        {
            get
            {
                string description = resrcMngr.GetString(resrcKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", resrcKey) : description;
            }
        }
    }

    public class BSkyLocalizedCategoryAttribute : CategoryAttribute
    {
        readonly ResourceManager resrcMngr;
        readonly string resrcKey;

        public BSkyLocalizedCategoryAttribute(string ResrcKey, Type ResrcType)
        {
            resrcMngr = new ResourceManager(ResrcType);
            resrcKey = ResrcKey;
        }

        protected override string GetLocalizedString(string value)
        {
                string category = resrcMngr.GetString(resrcKey);
                return string.IsNullOrWhiteSpace(category) ? string.Format("[[{0}]]", resrcKey) : category;
        }

    }
}
