using UnityEngine;

namespace Vdopia
{
    //Class used for receive Vdopia Ad Event from Java to Unity using AndroidJavaProxy implementation
    public class VdopiaListener : AndroidJavaProxy
    {
        //Delegate for passing Vdopia Ad Events to Publisher
        public delegate void VdopiaAdDelegate(string adType, string eventName);
        public event VdopiaAdDelegate VdopiaAdDelegateEventHandler;

        //Singleton listener instance to set the delegate for Ad Event
        private static VdopiaListener instance;

        //VdopiaListener is Unity Plugin Listener used by publisher to get the Ad Event
        //base("com.vdopia.unity.plugin.IVdopiaAdListener") is the JAVA SDK Interface
        //which has onVdopiaAdEvent callback method called by Java Plugin when Ad Event occur
        private VdopiaListener() : base("com.vdopia.unity.plugin.VdopiaAdUnityListener")
        {
            Debug.Log("VdopiaListener Initialized...");
        }

        public static VdopiaListener GetInstance()
        {
            if (instance == null)
            {
                instance = new VdopiaListener();
            }

            return instance;
        }

        //This method is called when JAVA Plugin send the Ad Event to the Unity Plugin
        //And passes the callback to the publisher if delegate is set
        void onVdopiaAdEvent(string adType, string eventName)   //Received Ad Event from Java
        {
            Debug.Log("Vdopia Ad Event In Unity : " + eventName + " : For Ad Type : " + adType);
            if (VdopiaAdDelegateEventHandler != null)
            {
                VdopiaAdDelegateEventHandler(adType, eventName);      //Pass Ad Event to Publisher
            }
        }
    }
}