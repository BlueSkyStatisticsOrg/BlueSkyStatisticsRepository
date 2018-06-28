
namespace BSky.Lifetime.Interfaces
{
    public interface IBSkyLicService
    {
        #region Properties
        bool ValidLic { get; }
        string MaxVersion { set; }
        string LicMessage { get; }
        int DaysLeft { get; }
        string LicType { get; }
        bool hasLicStatusChanged { get; } //if 24hr timer has modified left days, this is flag is set to true. If not it will be false
        
        #endregion

        #region Methods

        //Checkout
        int CheckLicStatus(bool ShowDaysLeftReminderPopup);//string maxVersion, ref int days, ref int statcode); 

        //Install Demo
        void InstallDemoLic(int days);//string maxVersion, int days, ref int statcode);

        //Get Error Message
        string GetLicError(int statcode);

        //Check license and install demo if required make use of methods above. 
        //traildays is option as default is given 
        void LicenseCheck(int trialdays=3);
        #endregion
    }
}
