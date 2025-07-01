using System;
using Balaso;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.Events;

public class AppTrackingTransparencyManagerWithFacebook : MonoBehaviour
{
    protected static AppTrackingTransparencyManagerWithFacebook Instance;
#if UNITY_IOS
    public static ATTResult result { get; private set; }
#endif

    public UnityEvent onGameReadyToInit;
    //public UnityEvent onFacebookInitialized;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Duplicate instance " + GetType());
            Destroy(this);
            return;
        }

        Instance = this;
        FacebookInit();
    }

    void FacebookInit()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback);
        }
        else
        {
            Debug.LogError("Already initialized, signal an app activation App Event");
            FB.ActivateApp();
        }
    }

    void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...

            StartATT();
            //onFacebookInitialized?.Invoke();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }


    void MoveToGameInit()
    {
        if (onGameReadyToInit != null)
            functionMustCallFromMainThread = new Action(onGameReadyToInit.Invoke);
        AfterATTResult();
    }

    /// <summary>
    /// Issue : Game fail to run when we call Unity function from other thread, Ex : Scene loading, Audio play etc.
    /// </summary>
    Action functionMustCallFromMainThread = null;

    void Update()
    {
        if (functionMustCallFromMainThread != null)
        {
            functionMustCallFromMainThread.Invoke();
            functionMustCallFromMainThread = null;
        }
    }






    void StartATT()
    {

#if UNITY_IOS
        var currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;
        result = new ATTResult()
        {
            authorizationStatus = AppTrackingTransparency.TrackingAuthorizationStatus,
            isUpdated = false
        };

        AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
        Debug.Log("Request AppTrackingTransparency");



        AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
        Debug.Log(string.Format("Current authorization status: {0}", currentStatus.ToString()));

        if (currentStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
        {
            Debug.Log("Requesting authorization...");
            AppTrackingTransparency.RequestTrackingAuthorization();
            result.isUpdated = false;
        }
        else
        {
            result.isUpdated = true;
            result.authorizationStatus = currentStatus;
            MoveToGameInit();
        }
#else
        //For Android and other platforms
        MoveToGameInit();
#endif
    }

#if UNITY_IOS

    /// <summary>
    /// Callback invoked with the user's decision
    /// </summary>
    /// <param name="status"></param>
    private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status)
    {

        result.isUpdated = true;
        result.authorizationStatus = status;

        switch (status)
        {
            case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                Debug.Log("AuthorizationStatus: NOT_DETERMINED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                Debug.Log("AuthorizationStatus: RESTRICTED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.DENIED:
                Debug.Log("AuthorizationStatus: DENIED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                Debug.Log("AuthorizationStatus: AUTHORIZED");
                break;
        }

        // Obtain IDFA
        Debug.Log(string.Format("IDFA: {0}", AppTrackingTransparency.IdentifierForAdvertising()));

        // START THE GAME FROM HERE
        MoveToGameInit();
    }

    static void SetAdvertiserTrackingEnabled(bool enable)
    {
        UnityEngine.Debug.Log("FacebookAdvertiserTrackingUtility : " + enable);
        FB.Mobile.SetAdvertiserTrackingEnabled(enable);
    }

#endif

    void AfterATTResult()
    {

#if UNITY_IOS
        var result = AppTrackingTransparencyManager.result;
        if (result != null && result.isUpdated && result.authorizationStatus == Balaso.AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
        {
            //By default is false in facebook SDK
            SetAdvertiserTrackingEnabled(true);
        }
#endif
    }

}
