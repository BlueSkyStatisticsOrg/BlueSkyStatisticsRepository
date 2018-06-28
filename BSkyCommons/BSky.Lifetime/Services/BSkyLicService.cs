using System;
using BSky.Lifetime.Interfaces;
using System.Runtime.InteropServices;
using System.IO;
using Reprise;
using System.Windows;

namespace BSky.Lifetime.Services
{
    public class BSkyLicService : IBSkyLicService
    {
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();
        bool _validLic;
        string _maxversion;
        string _licmessage;
        int _daysleft;
        int _statuscode;
        string _licType;
        bool _hasLicStatChanged;
        DateTime _startdate; //when app is launched.
        int TrialDays = 30;//30 days trial

        #region Properties
        public bool ValidLic
        {
            get 
            {

                if ((DateTime.Now - _startdate).Days >= 1)//if app is continuously running for 1 days or more
                {
                    //MessageBox.Show("After " + (DateTime.Now - _startdate).Days + " day(s). Checking Lic and resetting timer for next day.");
                    CheckLicStatus(true);//check license each day; This will set _validLic to true or false based of license status.
                    _startdate = DateTime.Now; // reset your timer for next 1 day.
                    _hasLicStatChanged = true;//'Days left' changed. Probably decreased.
                }
                else
                {
                    _hasLicStatChanged = false; // no change in 'Days left'
                }
                
                return _validLic; 
            }
        }

        public string MaxVersion
        {
            set
            {
                // Set Max version only once at app launch time. 
                //For whole session this version will be used where ever or whenever license is checked
                if (_maxversion == null || _maxversion.Trim().Length < 1)
                {
                    _maxversion = value;
                    _startdate = DateTime.Now;//current datetime. ie at launch
                    //MessageBox.Show("Setting Timer at Lauch.");
                }
            }
        }

        public string LicMessage
        {
            get
            {
                return _licmessage;
            }
        }

        public int DaysLeft
        {
            get
            {
                return _daysleft;
            }
        }

        public string LicType
        {
            get 
            {
                _licType = "(Trial Version).";
                string[] licfiles = System.IO.Directory.GetFiles(@".\", "*.lic", System.IO.SearchOption.TopDirectoryOnly);
                if (licfiles.Length > 0)
                {
                    if (File.Exists(licfiles[0]) && _validLic)//checking for just one file
                    {
                        _licType = "(Full Version).";
                    }
                }                
                return _licType; 
            }
        }

        public bool hasLicStatusChanged { get { return _hasLicStatChanged; } } 
        #endregion

        #region Methods

        //get message in LicMessage property and number of days in DaysLeft property
        public int CheckLicStatus(bool ShowDaysLeftReminderPopup)
        {
            int dayslft = 0;
            string maxver = _maxversion;
            _validLic = false;
            int statcode = -9999;


            try
            {
                statcode = RLMEZ.rlmez_checkout(maxver, ref dayslft);
                _daysleft = dayslft-1;
            }
            catch (Exception ex)
            {
                _licmessage = "Checkout Failed. Probably DLLs missing."+ ex.Message;
                return statcode;
            }
            if (_daysleft == -1 && statcode == 0)//valid permanent license. Never Expires
            {
                _licmessage = "Permanent License!!";// No Expiry Date!";
                _validLic = true;             
            }
            else if (_daysleft >= 0)//Valid License (Trial of Full Version)
            {
                _licmessage = "License will expire in " + (_daysleft)+" day(s)";
                _validLic = true;
            }
            else//Lic not valid
            {
                _licmessage = GetLicError(statcode);
            }

            //Show popup for specific number of days left, only if license is valid and popup is allowed to be displayed
            if (statcode == 0 && _validLic && ShowDaysLeftReminderPopup)//valid Lic
            {
                ShowDaysLeftReminder();
            }
            return statcode;
        }

        //get message in LicMessage property
        public void InstallDemoLic(int days)
        {
            _validLic = false;
            string message = string.Empty;
            int statcode = RLMEZ.rlmez_demo(_maxversion, 30);//trial days

            switch (statcode)
            {
                case RLMEZ.RLM_EL_DEMOEXP:
                    message = "Trial License Has Expired";
                    break;
                case RLMEZ.RLM_EL_EXPIRED:
                    message = "Authorization Has Expired";
                    break;
                case RLMEZ.RLM_EH_DEMOEXISTS:
                    message= "Trial Already Exists";
                    break;
                case RLMEZ.RLM_EH_EVAL_EXPIRED:
                    message = "RLM Evaluation Expired";
                    break;
                case 0:
                    message = "Trial Installed Successfully";
                    _validLic = true;
                    break;
                default:
                    message = "Error Installing Trial";
                    break;
            }
            _licmessage = message;
            //return statcode;
        }

        public string GetLicError(int statcode)
        {
            byte[] errbytes = new byte[1000];
            IntPtr errmsg = RLMEZ.rlmez_errstring(statcode, errbytes);
            string errstring = Marshal.PtrToStringAnsi(errmsg);
            return errstring;
        }
        

        //we can include this methods here and can handle everthing from here. 
        //This same method definition is commented in App.xaml.cs but call is made there to perform license check in the beginning when app is launched.
        //But we can put this method here and can call it from anywhere
        public void LicenseCheck(int days=3)
        {
            string errormsg = string.Empty;
            //int days = 3; //3 days trial.
            string s1 = "BlueSky Application needs a valid license.";
            string s2 = "\nFor help, go to Help -> Licensing.";
            string s3 = "\nContact BlueSky at ( support@blueskystatistics.com ).";
            string s4 = "\n App will exit now.";

            int statcode = CheckLicStatus(true);
            if (statcode == -9999)
            {
                errormsg = LicMessage;
                MessageBox.Show(errormsg + "\nError code -9999. DLL issue!"+s3+s4, "DLL Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            if (statcode == 0 && _validLic)//valid Lic, Do nothing
            {
                //ShowDaysLeftReminder();//this already ran in CheckLicSataus() so no need to run it again
            }
            else if (statcode == -1)//no license installed. Then install Trial
            {
                MessageBoxResult mbr = MessageBox.Show("No License Found! \nDo you want to install " + TrialDays + " days trial?", "Install Trial:", MessageBoxButton.YesNo, MessageBoxImage.Hand);
                if (mbr == MessageBoxResult.Yes)
                {
                    InstallDemoLic(30);// 30 Day Trial
                    if (!_validLic)
                    {
                        errormsg = LicMessage;
                        MessageBox.Show(errormsg + "\n"+s1+s2+s3, "License Info:", MessageBoxButton.OK, MessageBoxImage.Stop);
                        //Environment.Exit(0);
                    }
                }
                else
                {
                    MessageBox.Show(s1+s2+s3, "License Help:", MessageBoxButton.OK, MessageBoxImage.Stop);
                    //Environment.Exit(0);
                }
            }
            else //No valid license :  show help message to fix license issue.
            {
                MessageBox.Show(s1+s2+s3, "License Help:", MessageBoxButton.OK, MessageBoxImage.Information);
                //Environment.Exit(0);
            }
        }


        private void ShowDaysLeftReminder()
        {
            ////remider daysleft from config//////
            bool isReminderDayToday = false;
            string[] reminderdays = null; ;
            string strdaysleft = confService.GetConfigValueForKey("daysleftreminder");//23nov2012 
            if (strdaysleft!= null && strdaysleft.Trim().Length > 0)
            {
                reminderdays = strdaysleft.Split(',');
            }
            if(reminderdays!=null)
            {
                foreach (string s in reminderdays)
                {
                    if (DaysLeft.ToString().Trim().Equals(s.Trim()))
                    {
                        isReminderDayToday = true;
                        break;
                    }
                }
            }

            //only show popup when these many number of days are left.
            if (DaysLeft == 0 || DaysLeft == 1 || isReminderDayToday)
            {
                MessageBox.Show(LicMessage + " : " + LicType, "License Status", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

    }
}
