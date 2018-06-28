using System;
using System.Linq;
using System.Xml;

namespace BSky.Statistics.Common
{

    public static class XmlDocumentDecorator
    {
        public class DecoratorTags
        {
            public const string OutputDecorator = "OutputDecorator";
        }
        public static void AddDocumentNode(XmlNode parent, ComponentRenderType outputType, string value)
        {
            XmlElement text = parent.OwnerDocument.CreateElement(UADataType.UAString.ToString());
            text.InnerText = value;
            text.SetAttribute(DecoratorTags.OutputDecorator, ComponentRenderType.Title.ToString());
            parent.InsertBefore(text, parent.FirstChild);

        }
        public static void Decorate(XmlNode root)
        {
            if (Enum.GetNames(typeof(UADataType)).Contains(root.Name))
            {
                UADataType dataType = (UADataType)Enum.Parse(typeof(UADataType), root.Name, true);

                XmlElement element = root as XmlElement;
                switch (dataType)
                {
                    case UADataType.UAString:
                    case UADataType.UAInt:
                    case UADataType.UADouble:
                        element.SetAttribute(DecoratorTags.OutputDecorator, ComponentRenderType.BodyText.ToString());
                        break;
                    case UADataType.UAIntMatrix:
                    case UADataType.UAStringMatrix:
                    case UADataType.UADoubleMatrix:
                        element.SetAttribute(DecoratorTags.OutputDecorator, ComponentRenderType.Grid.ToString());
                        break;


                }
            }
            foreach (XmlNode nd in root.ChildNodes)
            {
                Decorate(nd);
            }


        }
    }
}
