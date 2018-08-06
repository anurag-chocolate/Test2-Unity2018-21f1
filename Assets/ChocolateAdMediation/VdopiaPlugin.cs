using UnityEngine;
using System;

namespace Vdopia
{
    public class VdopiaPlugin
    {
        // AD Types Interstitial and Reward Used to identify adtype in callback
        public static readonly string INTERSTITIAL_AD_TYPE = "INTERSTITIAL";
        public static readonly string REWARD_AD_TYPE = "REWARD";

        // AD Event Message For Interstitial and Reward Used to identify event in callback
        public static readonly string INTERSTITIAL_AD_LOADED = "INTERSTITIAL_LOADED";
        public static readonly string INTERSTITIAL_AD_FAILED = "INTERSTITIAL_FAILED";
        public static readonly string INTERSTITIAL_AD_SHOWN = "INTERSTITIAL_SHOWN";
        public static readonly string INTERSTITIAL_AD_DISMISSED = "INTERSTITIAL_DISMISSED";
        public static readonly string INTERSTITIAL_AD_CLICKED = "INTERSTITIAL_CLICKED";

        public static readonly string REWARD_AD_LOADED = "REWARD_AD_LOADED";
        public static readonly string REWARD_AD_FAILED = "REWARD_AD_FAILED";
        public static readonly string REWARD_AD_SHOWN = "REWARD_AD_SHOWN";
        public static readonly string REWARD_AD_SHOWN_ERROR = "REWARD_AD_SHOWN_ERROR";
        public static readonly string REWARD_AD_DISMISSED = "REWARD_AD_DISMISSED";
        public static readonly string REWARD_AD_COMPLETED = "REWARD_AD_COMPLETED";

        //Native Plugin Instance to call Native Method
        private AndroidJavaObject VDONativePlugin;

        //Singleton Plugin Instance to call method of this class
        private static VdopiaPlugin instance;

        public static VdopiaPlugin GetInstance()
        {
            if (instance == null)
            {
                instance = new VdopiaPlugin();
            }

            return instance;
        }

        private VdopiaPlugin()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                //Initialize VdopiaPlugin
                if (VDONativePlugin == null)
                {
                    using (var pluginClass = new AndroidJavaClass("com.vdopia.unity.plugin.VdopiaPlugin"))
                    {
                        VDONativePlugin = pluginClass.CallStatic<AndroidJavaObject>("GetInstance");
                    }
                }

                //Setting Context and Listener to Plugin
                if (VDONativePlugin != null)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");

                    VDONativePlugin.Call("SetActivity", currentActivity);
                    VDONativePlugin.Call("SetUnityAdListener", VdopiaListener.GetInstance());
                }
                else
                {
                    Debug.Log("Unable to Initialize VdopiaPlugin...");
                }
            }
        }

        //This method calls Native Method to Set Targeting Params related to the User
        public void SetAdRequestUserData(String age, String birthDate, String gender, String marital,
                                           String ethnicity, String dmaCode, String postal, String curPostal,
											String lat, String lon)
        {
            Debug.Log("SetAdRequestUserData...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
				VDONativePlugin.Call("SetAdRequestUserParams", age, birthDate, gender, marital,
					ethnicity, dmaCode, postal, curPostal, lat, lon);
            }
        }

        //This method calls Native Method to Set Targeting Params related to the App
        public void SetAdRequestAppData(String appName, String pubName,
                                          String appDomain, String pubDomain,
                                          String storeUrl, String iabCategory)
        {
            Debug.Log("SetAdRequestAppData...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                VDONativePlugin.Call("SetAdRequestAppParams", appName, pubName, appDomain, pubDomain,
                                                            storeUrl, iabCategory);
            }
        }

        //This method calls Native Method to Set Test Mode (For Test Ad of Facebook/Google Partner)
        public void SetAdRequestTestMode(bool isTestMode, String testID)
        {
            Debug.Log("SetAdRequestTestMode...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                VDONativePlugin.Call("SetTestModeEnabled", isTestMode, testID);
            }
        }

        //Initializes Chocolate Platform Ads SDK.  There are NO callbacks for this.
        public void ChocolateInit(String apiKey)
        {
           Debug.Log("ChocolateInit...");
           if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
           {
               VDONativePlugin.Call("ChocolateInit", apiKey);
           }

           return;
        }

        //Prefetches interstitial ad in background and caches it.  There are NO callbacks for this.
        public void PrefetchInterstitialAd(String apiKey)
        {
           Debug.Log("Prefetch Interstitial...");
           if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
           {
               VDONativePlugin.Call("PrefetchInterstitialAd", apiKey);
           }

           return;
        }

        //This method calls Native Method to Load Interstitial Ad
        public void LoadInterstitialAd(String apiKey)
        {
            Debug.Log("Load Interstitial...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                VDONativePlugin.Call("LoadInterstitialAd", apiKey);
            }

            return;
        }

        //This method calls Native Method to Show Interstitial Ad
        public void ShowInterstitialAd()
        {
            Debug.Log("Show Interstitial...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                VDONativePlugin.Call("ShowInterstitialAd");
            }

            return;
        }

        //Prefetches rewarded ad in background and caches it.  There are NO callbacks for this.
        public void PrefetchRewardAd(String apiKey)
        {
           Debug.Log("Prefetch Reward...");
           if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
           {
               VDONativePlugin.Call("PrefetchRewardAd", apiKey);
           }

           return;
        }

        //This method calls Native Method to Load Reward Ad
        public void RequestRewardAd(String apiKey)
        {
            Debug.Log("Request Reward...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                VDONativePlugin.Call("LoadRewardAd", apiKey);
            }

            return;
        }

        //This method calls Native Method to Show Reward Ad
        public void ShowRewardAd(String secret, String userId, String rewardName, String rewardAmount)
        {
            Debug.Log("Show Reward...");
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                VDONativePlugin.Call("ShowRewardAd", secret, userId, rewardName, rewardAmount);
            }

            return;
        }

        //This method calls Native Method to Check Reward Ad Availability
        //Returns true if Available and ready else return false
        public bool IsRewardAdAvailableToShow()
        {
            Debug.Log("Check Reward...");
            bool isAvailable = false;
            if (Application.platform == RuntimePlatform.Android && VDONativePlugin != null)
            {
                isAvailable = VDONativePlugin.Call<bool>("IsRewardAdAvailableToShow");
            }

            Debug.Log("Is Reward..." + isAvailable);
            return isAvailable;
        }

		public void SetGDPR(String isSubjectToGDPR, String iabConsentString) {
			Debug.Log("SetGDPR...");
			try {
				VDONativePlugin.Call("SetGDPR", isSubjectToGDPR, iabConsentString);
			}catch (Exception e) {
				Debug.LogError ("SetGDPR() failed: " + e);
			}
		}

		public bool IsSubjectToGDPR() {
			bool isSubject = false;
			try {
				isSubject = VDONativePlugin.Call<bool>("IsSubjectToGDPR");
			}catch (Exception e) {
				Debug.LogError("IsSubjectToGDPR failed: " + e);
			}
			return isSubject;
		}

		public bool IsGDPRConsentAvailable() {
			bool isAvailable = false;
			try {
				isAvailable = VDONativePlugin.Call<bool>("IsGDPRConsentAvailable");
			}catch (Exception e) {
				Debug.LogError("IsGDPRConsentAvailable failed: " + e);
			}
			return isAvailable;
		}

		public String GetGDPRConsentString() {
			String consentString = "";
			try {
				consentString = VDONativePlugin.Call<String>("GetGDPRConsentString");
			}catch (Exception e) {
				Debug.LogError("GetGDPRConsentString failed: " + e);
			}
			return consentString;
		}

    }
}