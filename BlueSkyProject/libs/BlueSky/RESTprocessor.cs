//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace BlueSky
{
    class RESTprocessor
    {

        public Task GetRequest(string URL, QProDatasetInfo qpdsi)
        {
            HttpClient client = null;
            string errmg = string.Empty;
            //QProDatasetInfo qpdsi = null;
            try
            {
                //URL = "https://hivelabs.questionpro.com/a/ds/?t=5mnaDTMyMncTl24qn3Q%2BGgH8%2BI4tMoEcFUCnqiDY4NNwOIPEVWyCBq4xpKZdHA55";
                client = new HttpClient();
                //client.BaseAddress = new Uri(URL);
                HttpResponseMessage response = client.GetAsync(URL).Result;
                //string response2 =  client.GetStringAsync(apikeyparam).Result;  

                if (response.IsSuccessStatusCode)
                {
                    //qpdsi = new QProDatasetInfo();
                    qpdsi.Filename = response.Content.Headers.ContentDisposition.FileName; ;
                    qpdsi.ApiKey = response.Headers.GetValues("apikey").FirstOrDefault(); ;
                    qpdsi.DatasetId = response.Headers.GetValues("datasetid").FirstOrDefault(); ;
                    qpdsi.SurveyId = response.Headers.GetValues("surveyid").FirstOrDefault(); ;
                    qpdsi.UserId = response.Headers.GetValues("userid").FirstOrDefault(); ;
                    //qpdsi.DatasetName = "";
                    qpdsi.ErrorMsg = string.Empty;

                    ////get CSV filename
                    //string csvfname = response.Content.Headers.ContentDisposition.FileName;
                    ////reading CSV  content
                    ////html.Text = response.Content.ReadAsStringAsync().Result;

                    //string apikey = response.Headers.GetValues("apikey").FirstOrDefault();
                    //string datasetid = response.Headers.GetValues("datasetid").FirstOrDefault();
                    //string surveyid = response.Headers.GetValues("surveyid").FirstOrDefault();
                    //string userid = response.Headers.GetValues("userid").FirstOrDefault();
                }
                else
                {
                    errmg = string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                errmg = ex.Message + "\n" + ex.StackTrace;
            }
            finally
            {
                if (client != null)
                    client.Dispose();
                qpdsi.ErrorMsg = errmg;
            }
            return Task.CompletedTask;
        }

        /*
        private async Task PutRequest()
        {
            String htmltext = html.Text;// html.Text - does not need jsonconvert //
            htmltext = @"{""analysisHtmlText"": """ + html.Text.Replace("\"", "\\\"").Replace("\'", "\\\"").Replace("\r\n", "") + @"""}";
            HttpClient client = null;

            try
            {
                //var json = JsonConvert.SerializeObject(htmltext);
                //var data = new StringContent(json, Encoding.UTF8, "application/json");
                var data = new StringContent(htmltext, Encoding.UTF8, "application/json");//WebUtility.HtmlEncode(json)
                if (data != null) Console.WriteLine("");

                string apkey = apikey.Text.Trim();
                string survid = surveyid.Text.Trim();
                string usrid = userid.Text.Trim();
                string dataid = datasetid.Text.Trim();
                var url = "https://hivelabs.questionpro.com/a/api/v2/surveys/" + survid +
                    "/datapads/" + dataid + "/?apiKey=" + apkey;
                client = new HttpClient();

                var response = await client.PutAsync(url, data);
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }

        }

    */


    }
}
