#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using NekrasovskyAPP.Platforms.Android;

namespace NekrasovskyAPP
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const int NotificationPermissionRequestCode = 9001;

        /// <summary>
        /// Текущая активность для доступа из DeviceLockService (блокировка выхода из приложения).
        /// </summary>
        public static MainActivity? Instance { get; private set; }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
            RequestNotificationPermissionIfNeeded();
            StartAlarmService();
        }

        protected override void OnDestroy()
        {
            Instance = null;
            base.OnDestroy();
        }

        /// <summary>
        /// Включить закрепление экрана (пользователь не может выйти из приложения без разблокировки с сервера).
        /// </summary>
        public void EnterLockTask()
        {
            StartLockTask();
        }

        /// <summary>
        /// Отключить закрепление экрана (по команде с админ-панели).
        /// </summary>
        public void ExitLockTask()
        {
            StopLockTask();
        }

        private void RequestNotificationPermissionIfNeeded()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
            {
                return;
            }

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications)
                != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(
                    this,
                    new[] { Manifest.Permission.PostNotifications },
                    NotificationPermissionRequestCode);
            }
        }

        private void StartAlarmService()
        {
            var intent = new Intent(this, typeof(AlarmForegroundService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }
        }
    }
}
#endif
