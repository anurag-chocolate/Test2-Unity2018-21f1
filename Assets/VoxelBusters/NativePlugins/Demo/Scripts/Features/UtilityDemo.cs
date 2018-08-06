using UnityEngine;
using System.Collections;
using Vdopia;

namespace VoxelBusters.NativePlugins.Demo
{
    public class UtilityDemo : NPDemoBase
    {

        VdopiaPlugin plugin;

        #region Properties

        [SerializeField]
        private int m_applicationBadgeNumber = 2;
        private string API_KEY = "XqjhRR";

        #endregion

        #region Unity Methods

        protected override void Start()
        {
            base.Start();

            // Set additional info texts
            AddExtraInfoTexts(
                "For using RateMyApp feature, you have to enable it in NPSettings->Utility Settings.");

            plugin = VdopiaPlugin.GetInstance();       //Initialize Plugin Instance

            if (Application.platform == RuntimePlatform.Android)
            {
                plugin = VdopiaPlugin.GetInstance();       //Initialize Plugin Instance

                if (plugin != null)
                {

                    VdopiaListener.GetInstance().VdopiaAdDelegateEventHandler += onVdopiaEventReceiver;

                    //Set USER parameter used for better ad targeting and higher yield (Not mandatory)
                    //Developer can pass empty string for any Param like ""
                    //Param 1 : Age
                    //Param 2 : BirthDate (dd/MM/yyyy)
                    //Param 3 : Gender (m/f/u)
                    //Param 4 : Marital Status (single/married/unknown)
                    //Param 5 : Ethinicty (example : Africans/Asian/Russians)
                    //Param 6 : DMA Code (in String format)
                    //Param 7 : Postal Code (in String format)
                    //Param 8 : Current Postal Code (in String format)
                    //Param 9 : Location latitude in string format
                    //Param 10 : Location longitude in string format

                    plugin.SetAdRequestUserData("23", "23/11/1990", "m", "single", "Asian", "999", "123123", "321321", "", "");

                    //Set APP parameter used better Ad targeting and higher yield
                    //Developer can pass empty string for any Param like ""
                    //Param 1 : App Name
                    //Param 2 : Publisher Name
                    //Param 3 : App Domain
                    //Param 4 : Publisher Domain
                    //Param 5 : PlayStore URL of the App
                    //Param 6 : App Category (IAB category)

                    plugin.SetAdRequestAppData("UnityDemo", "Chocolate", "unity-demo.com", "chocolateplatform.com", "", "IAB9");

                    //Set Test Mode parameter used for Getting Test AD (Not mandatory)
                    //Param 1 : boolean : true if test mode enabled else false
                    //Param 2 : Hash ID (If you are testing Facebook/Google Partner Test Ad you can get from ADB Logcat)
                    //plugin.SetAdRequestTestMode(true, "XXXXXXXXXXXXXXXX");

                    //Initialize Chocolate Platform Ads Sdk!
                    plugin.ChocolateInit(API_KEY);


                }
                else
                {
                    Debug.Log("Vdopia Plugin Initialize Error.");
                }

            }
        }

        void onVdopiaEventReceiver(string adType, string eventName)     //Ad Event Receiver
        {
            Debug.Log("Ad Event Received : " + eventName + " : For Ad Type : " + adType);
            if (eventName.Equals("INTERSTITIAL_LOADED"))
            {

                showInterstitial();

            }
            else if (eventName.Equals("REWARD_AD_LOADED"))
            {

                showReward();

            }
        }

        private void loadInterstitial()
        {
            Debug.Log("Load Interstitial...");
            if (Application.platform == RuntimePlatform.Android && plugin != null)
            {
                //Param 1: AdUnit Id (This is your SSP App ID you received from your account manager or obtained from the portal)
                plugin.LoadInterstitialAd(API_KEY);
            }
        }

        private void showInterstitial()
        {
            Debug.Log("Show Interstitial...");
            if (Application.platform == RuntimePlatform.Android && plugin != null)
            {
                //Make sure Interstitial Ad is loaded before call this method
                plugin.ShowInterstitialAd();

                //New!  Optional.  Silently pre-fetch the next interstitial ad without making
                //any callbacks.  The pre-fetched ad will remain in cache until you call
                //the next LoadInterstitialAd.
                plugin.PrefetchInterstitialAd(API_KEY);
            }
        }

        private void requestReward()       //called when btnRequestReward Clicked
        {
            Debug.Log("Request Reward...");
            if (Application.platform == RuntimePlatform.Android && plugin != null)
            {
                //Param 1: AdUnit Id (This is your SSP App ID you received from your account manager or obtained from the portal)
                plugin.RequestRewardAd(API_KEY);
            }
        }

        private void showReward()           //called when btnShowReward Clicked
        {
            Debug.Log("Show Reward...");

            //Make sure Ad is loaded before call this method
            if (Application.platform == RuntimePlatform.Android && plugin != null)
            {
                //Parma 1: Secret Key (Get it from Vdopia Portal: Required if server-to-server callback enabled)
                //Parma 2: User name – this is the user ID of your user account system
                //Param 3: Reward Currency Name or Measure
                //Param 4: Reward Amount
                plugin.ShowRewardAd("qj5ebyZ0F0vzW6yg", "Chocolate1", "coin", "30");

                //Pre-fetch:  Silently pre-fetch the next reward ad without making
                //any callbacks.  The pre-fetched ad will remain in cache until you call
                //the next LoadRewardAd.
                plugin.PrefetchRewardAd(API_KEY);
            }

        }

        #endregion

        #region GUI Methods

        protected override void DisplayFeatureFunctionalities()
        {
            base.DisplayFeatureFunctionalities();

            if (GUILayout.Button("Get UUID"))
            {
                string _uuid = GetUUID();

                AddNewResult("New UUID = " + _uuid + ".");
            }

            if (GUILayout.Button("Open Store Link"))
            {
                string _appIdentifier = NPSettings.Application.StoreIdentifier;

                AddNewResult("Opening store link with application id = " + _appIdentifier + ".");
                OpenStoreLink(_appIdentifier);
            }

            if (GUILayout.Button("Ask For Review Now"))
            {
                AskForReviewNow();
            }

            if (GUILayout.Button("Set Application Icon Badge Number"))
            {
                SetApplicationIconBadgeNumber();
            }

            if (GUILayout.Button("Get Bundle Version"))
            {
                AddNewResult("Application's bundle version is " + NPBinding.Utility.GetBundleVersion() + ".");
                loadInterstitial();
            }

            if (GUILayout.Button("Get Bundle Identifier"))
            {
                AddNewResult("Application's bundle identifier is " + NPBinding.Utility.GetBundleIdentifier() + ".");
                requestReward();
            }
        }

        #endregion

        #region API Methods

        private string GetUUID()
        {
            return NPBinding.Utility.GetUUID();
        }

        private void OpenStoreLink(string _applicationID)
        {
            NPBinding.Utility.OpenStoreLink(_applicationID);
        }

        private void AskForReviewNow()
        {
            if (NPSettings.Utility.RateMyApp.IsEnabled)
            {
                NPBinding.Utility.RateMyApp.AskForReviewNow();
            }
            else
            {
                AddNewResult("Enable RateMyApp feature in NPSettings.");
            }
        }

        private void SetApplicationIconBadgeNumber()
        {
            NPBinding.Utility.SetApplicationIconBadgeNumber(m_applicationBadgeNumber);
        }

        #endregion
    }
}