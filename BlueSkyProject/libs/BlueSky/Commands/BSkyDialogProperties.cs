
namespace BlueSky.Commands
{
    public class BSkyDialogProperties
    {
        //dialog properties
        private bool isCommandOnly; // its dialog or command(no dialog)
        private bool isXMLDefined;// XML template is defined or not
        private bool isBatchCommand;// its batch command or single command dialog
        private bool handleSplits; //if TRUE, dialog command(s) will run on slices if there is split. If FALSE, whole dataset will be considered.
        private string requiredRpackages;//semi colon separated
        private string modelClasses;//semi colon separated
        //Per command Properties
        private bool isGridRefresh;// After all command gets executed, refresh grid or not.
        private bool isStatusBarRefresh;//After commands gets executed, refresh status bar. Mainly for splits
        private bool isGraphic; //is graphic command
        private bool isMacroUpdate; // it should update macro too to make things work. Like splits/removesplits

        
        private bool isSparkR; //28Jul2017 if requiredRpackages contains SparkR then dialog is a Spark related dialog

        //dialog commands
        private string[] commands; //single or multiple commads from dialog command string
        private string dialogtitle; //Title given at the time of dialog creation

        public bool IsCommandOnly
        {
            get { return isCommandOnly; }
            set { isCommandOnly = value; }
        }
        public bool IsXMLDefined 
        {
            get { return isXMLDefined; }
            set { isXMLDefined = value; } 
        }
        public bool IsBatchCommand 
        {
            get { return isBatchCommand; }
            set { isBatchCommand = value; }
        }
        public bool IsGridRefresh 
        {
            get { return isGridRefresh; }
            set { isGridRefresh = value; } 
        }

        public bool IsStatusBarRefresh
        {
            get { return isStatusBarRefresh; }
            set { isStatusBarRefresh = value; }
        }

        public bool IsGraphic
        {
            get { return isGraphic; }
            set { isGraphic = value; }
        }

        public bool IsMacroUpdate
        {
            get { return isMacroUpdate; }
            set { isMacroUpdate = value; }
        }

        public bool HandleSplits
        {
            get { return handleSplits; }
            set { handleSplits = value; }
        }

        public string RequiredRPackages //30Apr2015 required R packages for dialog to run.
        {
            get { return requiredRpackages; }
            set { requiredRpackages = value; }
        }

        public string ModelClasses
        {
            get { return modelClasses; }
            set { modelClasses = value; }
        }

        public string[] Commands
        {
            get { return commands; }
            set { commands = value; }
        }

        public string DialogTitle 
        {
            get { return dialogtitle; }
            set { dialogtitle = value; }
        }

        //some readonly properties those are reverse of above properties. 
        //So user will have option to use different name(no other benefit)
        public bool IsDialog
        {
            get { return !isCommandOnly; }
            //set { isDialog = value; }
        }


        public bool IsSingleCommand
        { 
            get { return !isBatchCommand; } 
        }

        //28Jul2017 To see if dialog is Spark dialog. Spark dialog requires SparkR R package.
        public bool IsSparkR //30Apr2015 required R packages for dialog to run.
        {
            get { return isSparkR; }
            set
            {
                if (requiredRpackages.Contains("SparkR"))
                    isSparkR = true;
                else
                    isSparkR = false;
            }
        }

    }
}
