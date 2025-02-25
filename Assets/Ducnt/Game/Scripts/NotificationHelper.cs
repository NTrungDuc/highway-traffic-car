using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
using UnityEngine.iOS;
#endif
using System.Collections;
public class NotificationHelper : MonoBehaviour
{
    static List<string> Handled_Ids = new List<string>();
    string _Channel_Id = "notify_daily_reminder";
    string _Icon_Small = "icon"; //this is setup under Project Settings -> Mobile Notifications
    string _Icon_Large = "logo"; //this is setup under Project Settings -> Mobile Notifications
    string _Channel_Title = "Highway Racer";
    string _Channel_Description = "Conquer it all!";
    private int _IdChannelOffline;
    private bool checkInit;

    private void Awake()
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = false;
    }

    private void PermissionCallbacks_PermissionDenied(string obj)
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
    }

    private void PermissionCallbacks_PermissionGranted(string obj)
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
        IntialNoti();
    }

    private void Start()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        if(GetVersionAndroid() >= 13){
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS",callbacks);
                PlayerPrefsSave.IsAcceptNotiAndroid13 = false;
                return;
            }
        }
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
        IntialNoti();
#elif !UNITY_EDITOR && UNITY_IOS
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
        IntialNoti();
#elif UNITY_EDITOR
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
#endif
    }


    void IntialNoti()
    {
        checkInit = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        //always remove any currently displayed notifications
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        //check if this was openened from a notification click
        var notification_intent_data = AndroidNotificationCenter.GetLastNotificationIntent();
        //if the notification intent is not null and we have not already seen this notification id, do something
        //using a static List to store already handled notification ids
        if (notification_intent_data != null && NotificationHelper.Handled_Ids.Contains(notification_intent_data.Id.ToString()) == false)
        {
            Handled_Ids.Add(notification_intent_data.Id.ToString());

            //this logic assumes only one type of notification is shown
            //show high scores when the user clicks the notification                
            //UnityEngine.SceneManagement.SceneManager.LoadScene("HighScores");
            return;
        }

        //dont do anything further if the user has disabled notifications
        //this assumes you have additional ui to enabled/disable this preference
        var allow_notifications = PlayerPrefs.GetString("notifications");
        if (allow_notifications?.ToLower() == "false")
        {
            return;
        }
#endif

        Setup_Notifications();
    }

    internal void Setup_Notifications()
    {
#if UNITY_ANDROID
        //initialize the channel
        Initialize();
        Debug.Log("Setup Nortification");
        //schedule the next notification
        Schedule_Daily_Reminder();
#endif
#if UNITY_IOS
        StartCoroutine(RequestAuthorization());
#endif
    }

#if UNITY_IOS
    IEnumerator RequestAuthorization()
    {
        var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);

        while (!req.IsFinished)
        {
            yield return null;
        };

        ScheduleNotificationIOS();
    }

    // void OnApplicationPause(bool pauseStatus)
    // {
    //     if (pauseStatus)
    //     {
    //         //schedule the next notification
    //         this.ScheduleNotificationIOS();
    //     }
    // }

    void ScheduleNotificationIOS()
    {
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();

        //create new schedule
        string[] titles = {
            "Start a new race🏁🥇🥈🥉",
            "Lunch time has come🏎️🚗🔥🏍️",
            "Overcome all challenges🏆🏆🏆🏆"
        };
        string[] bodies = {
            "🥇🏁🏆A new start, a new race, become a legend🥇🏁🏆",
            "🚗🔥🏍️Lunch break, have fun and relax with highway driver🚗🔥🏍️",
            "🏆🏆Challenge, fight, win, be the champion, become the legend of the city🏆🏆"
        };

        string title = "";
        string body = "";

        // send notification in the next 7 days at 12:30PM
        for (int i = 1; i <= 7; i++)
        {
            System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
            title = titles[rand.Next(titles.Length)];
            body = bodies[rand.Next(bodies.Length)];

            //show at the specified time - 12:30 AM
            //you could also always set this a certain amount of hours ahead, since this code resets the schedule, this could be used to prompt the user to play again if they haven't played in a while
            DateTime delivery_time12 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            DateTime delivery_time8 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
            DateTime delivery_time21 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);

            int add_8 = 0;
            int add_12 = 0;
            int add_21 = 0;


            if (delivery_time12 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_12 = 1;
            }
            if (delivery_time8 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_8 = 1;
            }
            if (delivery_time21 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_21 = 1;
            }
            // else if ((delivery_time - DateTime.Now).TotalHours <= 0)
            // {
            //     //optional
            //     //if too close to current time (<= 4 hours away), push delivery date forward 1 day
            //     delivery_time = delivery_time.AddDays(i);
            // }

            delivery_time8 = delivery_time8.AddDays(i + add_8);
            delivery_time12 = delivery_time12.AddDays(i + add_12);
            delivery_time21 = delivery_time21.AddDays(i + add_21);

            Debugers.Log("Schedule notification ...");

            ScheduleNotification(titles[0], bodies[0], delivery_time8);
            ScheduleNotification(titles[1], bodies[1], delivery_time12);
            ScheduleNotification(titles[2], bodies[2], delivery_time21);
        }


    }
#endif

#if UNITY_ANDROID
    void Initialize()
    {
        //add our channel
        //a channel can be used by more than one notification
        //you do not have to check if the channel is already created, Android OS will take care of that logic
        Debug.Log("minh_noti_Version Android : " + GetVersionAndroid());
        if (GetVersionAndroid() >= 8.0f)
        {
            AndroidNotificationChannel androidChannel = new AndroidNotificationChannel(this._Channel_Id, this._Channel_Title, this._Channel_Description, Importance.Default);
            AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
        }
    }

    private float GetVersionAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
        {
            AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
            return float.Parse(version.GetStatic<string>("RELEASE"));
        }
        catch (Exception e)
        {
            return 0;
        }
#endif
        return 0;
    }


    void Schedule_Daily_Reminder()
    {
        if (!checkInit) return;
        //since this is the only notification I have, I will cancel any currently pending notifications
        //if I create more types of notifications, additional logic will be needed
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        //create new schedule
        string[] titles = {
            "Start a new race🏁🥇🥈🥉",
            "Lunch time has come🏎️🚗🔥🏍️",
            "Overcome all challenges🏆🏆🏆🏆"
        };
        string[] bodies = {
            "🥇🏁🏆A new start, a new race, become a legend🥇🏁🏆",
            "🚗🔥🏍️Lunch break, have fun and relax with highway driver🚗🔥🏍️",
            "🏆🏆Challenge, fight, win, be the champion, become the legend of the city🏆🏆"
        };

        string title = "";
        string body = "";

        // Calculate delivery time for daily notification at 12:30PM
        DateTime currentDateTime = DateTime.Now;
        //DateTime deliveryTimeDaily = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 12, 30, 0);

        // Create and schedule daily notification
        //ScheduleNotification(titles[2], bodies[2], deliveryTimeDaily);


        for (int i = 0; i < 7; i++)
        {
            //System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
            title = titles[2];
            body = bodies[2];

            //show at the specified time - 12 AM
            //you could also always set this a certain amount of hours ahead, since this code resets the schedule, this could be used to prompt the user to play again if they haven't played in a while

            DateTime delivery_time12 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            DateTime delivery_time8 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
            DateTime delivery_time21 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);

            int add_8 = 0;
            int add_12 = 0;
            int add_21 = 0;


            if (delivery_time12 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_12 = 1;
            }
            if (delivery_time8 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_8 = 1;
            }
            if (delivery_time21 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_21 = 1;
            }
            // else if ((delivery_time - DateTime.Now).TotalHours <= 0)
            // {
            //     //optional
            //     //if too close to current time (<= 4 hours away), push delivery date forward 1 day
            //     delivery_time = delivery_time.AddDays(i);
            // }

            delivery_time8 = delivery_time8.AddDays(i + add_8);
            delivery_time12 = delivery_time12.AddDays(i + add_12);
            delivery_time21 = delivery_time21.AddDays(i + add_21);


            //delivery_time = DateTime.Now.AddSeconds(10);

            ScheduleNotification(titles[0], bodies[0], delivery_time8);
            ScheduleNotification(titles[1], bodies[1], delivery_time12);
            ScheduleNotification(titles[2], bodies[2], delivery_time21);

        }

        // offline
        // DateTime deliveryTime60min = currentDateTime.AddMinutes(60);
        // DateTime deliveryTime720min = currentDateTime.AddMinutes(720);
        // DateTime deliveryTime1440min = currentDateTime.AddMinutes(1440);

        // ScheduleNotification(titles[0], bodies[0], deliveryTime60min);
        // ScheduleNotification(titles[1], bodies[1], deliveryTime720min);
        // ScheduleNotification(titles[2], bodies[2], deliveryTime1440min);
    }

    int ScheduleNotification(string title, string body, DateTime deliveryTime)
    {
        if (!checkInit) return -1;
        if (deliveryTime > DateTime.Now)
        {
            Debug.Log("Schedule notification: " + deliveryTime.ToString());
            var scheduled_notification_id = AndroidNotificationCenter.SendNotification(
            new AndroidNotification()
            {
                Title = title,
                Text = body,
                FireTime = deliveryTime,
                SmallIcon = this._Icon_Small,
                LargeIcon = this._Icon_Large
            },
            this._Channel_Id);
            return scheduled_notification_id;
        }

        return -1;
    }

    // private void OnApplicationPause(bool pauseStatus)
    // {
    //     if (!checkInit) return;
    //     if (pauseStatus)
    //     {
    //         try
    //         {
    //             _IdChannelOffline = ScheduleNotification("",
    //                 "", DateTime.Now.AddHours(3));
    //         }
    //         catch (Exception e)
    //         { }
    //     }
    //     else
    //     {
    //         try
    //         {
    //             AndroidNotificationCenter.CancelScheduledNotification(_IdChannelOffline);
    //         }
    //         catch (Exception e)
    //         { }
    //     }
    // }
#endif

#if UNITY_IOS
    void ScheduleNotification(string title, string body, DateTime deliveryTime)
    {
        if (!checkInit) return;
        if (deliveryTime > DateTime.Now)
        {

            iOSNotification notify = new()
            {
                Trigger = new iOSNotificationCalendarTrigger()
                {
                    Year = deliveryTime.Year,
                    Month = deliveryTime.Month,
                    Day = deliveryTime.Day,
                    Hour = deliveryTime.Hour,
                    Minute = deliveryTime.Minute,
                    Second = deliveryTime.Second,
                    Repeats = false
                },
                Title = title,
                Body = body,
                Badge = 1
            };

            iOSNotificationCenter.ScheduleNotification(notify);

            return;
        }

    }
#endif
}
