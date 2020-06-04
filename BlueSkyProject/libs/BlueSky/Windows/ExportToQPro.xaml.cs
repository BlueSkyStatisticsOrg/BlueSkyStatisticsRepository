using BSky.ConfService.Intf.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueSky.Windows
{
    /// <summary>
    /// Interaction logic for ExportToQPro.xaml
    /// </summary>
    
    public partial class ExportToQPro : Window
    {
        private ILoggerService logService = LifetimeService.Instance.Container.Resolve<ILoggerService>();
        private IConfigService confService = null;
        OutputWindow ow = null;
        public ExportToQPro(OutputWindow owin)
        {
            InitializeComponent();
            ow = owin;
            confService = LifetimeService.Instance.Container.Resolve<IConfigService>();
            List<string> keys = QproHandler.GetKeys();
            datasetcombo.ItemsSource = keys;
        }

        private void Datasetcombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = datasetcombo.SelectedValue as string;
            logService.WriteToLogLevel("QP Selected dataset for PUT:"+selected, LogLevelEnum.Info);

            QProDatasetInfo qpdsinfo = QproHandler.GetQPDatasetInfo(selected);

            //Note no slash at the end https://hivelabs.questionpro.com/a/api/v2
            baseurltxt.Text =  confService.GetConfigValueForKey("qpbaseputurl");

            apikeytxt.Text = qpdsinfo.ApiKey;
            datasetidtxt.Text = qpdsinfo.DatasetId;
            surveyidtxt.Text = qpdsinfo.SurveyId;
            useridtxt.Text = qpdsinfo.UserId;

            detailsGrid.Visibility = Visibility.Visible;
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!RequiredFieldsAvailable())
            {
                logService.WriteToLogLevel("Cannot export to QuestionPro. One or more required fields empty. ", LogLevelEnum.Info);
                return;
            }
            /*
            string htmlfilename = "qpupload.html";
            string tempfullpathfilename = System.IO.Path.Combine(BSkyAppData.RoamingUserBSkyTempPath, htmlfilename);

            ow.DumpAllAnalyisOuput(tempfullpathfilename, C1.WPF.FlexGrid.FileFormat.Html, false);

            string htmltext = File.ReadAllText(tempfullpathfilename, System.Text.Encoding.UTF8);
            */

            string htmltext = ow.DumpAllAnalyisOuput_QPro();

            PutRequestAsync(htmltext);//this statement does not block the main thread but runs separately
                                      //we can use 'await' in the above statement to make sure PUT finishes  
                                      //and then the control comes back and then we do not need Thread.Sleep().

            Thread.Sleep(3000);//wait 3 seconds. No actual need. Just to be safe.
                               //Not sure what happens if user is trying to upload a HUGE output and if control comes back user
                               //just after 3 sec, user may try to do something that modifies the output window,
                               //which is probably in a process of PUT uploading due to HUGE output.
                               //for HUGE output uploading, we must use 'await' in PutRequestAsync and
                               //then no need to Thread.Sleep(), this will block user interaction with UI
                               //making sure that user do not do anything to generate more output.

            this.Close();
        }

        private bool RequiredFieldsAvailable()
        {
            bool filled = true; //mandatory fields are filled.
            if (string.IsNullOrEmpty(baseurltxt.Text.Trim()) ||
                string.IsNullOrEmpty(apikeytxt.Text.Trim()) ||
                string.IsNullOrEmpty(surveyidtxt.Text.Trim()) ||
                string.IsNullOrEmpty(datasetidtxt.Text.Trim())
                )
                filled = false;
            return filled;
        }

        private async Task PutRequestAsync(string htmltxt)
        {
            //remove \r ,\n , \t and replace \ by \\ and " by '
            string htmlfromoutputwin = htmltxt
                                        .Replace("\r", "")
                                        .Replace("\n", "")
                                        .Replace("\t", "")
                                        .Replace("\"", "\'")
                                        .Replace("\\", "\\\\");
            string finalhtmltext = string.Empty;// html.Text - does not need jsonconvert //
            finalhtmltext = @"{""analysisHtmlText"": """ + htmlfromoutputwin + @"""}";
            HttpClient client = null;

            try
            {
                //var json = JsonConvert.SerializeObject(htmltext);
                //var data = new StringContent(json, Encoding.UTF8, "application/json");
                var data = new StringContent(finalhtmltext, Encoding.UTF8, "application/json");//WebUtility.HtmlEncode(json)
                if (data != null) Console.WriteLine("");

                string baseurlforput = baseurltxt.Text;//https://hivelabs.questionpro.com/a/api/v2
                string apkey = apikeytxt.Text.Trim();
                string survid = surveyidtxt.Text.Trim();
                string usrid = useridtxt.Text.Trim();
                string dataid = datasetidtxt.Text.Trim();
                var url = baseurlforput+"/surveys/" + survid +
                    "/datapads/" + dataid + "/?apiKey=" + apkey;
                client = new HttpClient();

                logService.WriteToLogLevel("Sending data to QP.", LogLevelEnum.Info);
                var response = await client.PutAsync(url, data);

                string result = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    logService.WriteToLogLevel("Put Successful!", LogLevelEnum.Info);
                }
                else
                {
                    string errmsg = "Reason:" + response.ReasonPhrase + " Status code:" + response.StatusCode;
                    logService.WriteToLogLevel("QP PUT request failed!!"+errmsg, LogLevelEnum.Info);
                    logService.WriteToLogLevel(result, LogLevelEnum.Info);
                    MessageBox.Show("Export to QuestionPro failed.", "Request failed!", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                logService.WriteToLogLevel("QP Error Message:"+ ex.Message, LogLevelEnum.Error);
                logService.WriteToLogLevel("QP Stack Trace:"+ ex.StackTrace, LogLevelEnum.Error);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                    logService.WriteToLogLevel("QP connection closed.", LogLevelEnum.Info);
                }
            }

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Baseurltxt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                tb.IsReadOnly = false;
            }
        }

        private void Baseurltxt_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                tb.IsReadOnly = true;
            }
        }


    }
}
