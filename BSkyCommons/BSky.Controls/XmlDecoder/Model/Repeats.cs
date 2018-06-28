
namespace BSky.XmlDecoder
{
    public class InnerRepeat : Base
    {
        public string Condition { get; set; }
        public string RepeatOn { get; set; }
        public string VariableName { get; set; }

        public override void Initialize(System.Xml.XmlNode input)
        {
            Condition = input.Attributes[NodeNames.CONDITION].Value;
            RepeatOn = input.Attributes[NodeNames.REPEAT_ON].Value;
            VariableName = input.Attributes[NodeNames.VAR_NAME].Value;
        }

    }

    

}
