#if WINDOWS
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace multiply_choice_trainer.Platforms.Windows
{
    public static class FolderPickerImplementation
    {
        public static async Task<string> PickFolderAsync()
        {
            var folderPicker = new FolderPicker();
            var hwnd = WindowNative.GetWindowHandle(Microsoft.Maui.Controls.Application.Current.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            return folder?.Path;
        }
    }
}
#endif

#if ANDROID

using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.Content;

namespace multiply_choice_trainer.Platforms.Android
{
    public class FolderPickerImplementation
    {
        private readonly MainActivity _activity;
        private TaskCompletionSource<string> _taskCompletionSource;

        public FolderPickerImplementation(MainActivity activity)
        {
            _activity = activity;
        }

        public Task<string> PickFolderAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<string>();

            Intent intent = new Intent(Intent.ActionOpenDocumentTree);
            _activity.StartActivityForResult(intent, 9999);

            return _taskCompletionSource.Task;
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 9999 && resultCode == Result.Ok)
            {
                _taskCompletionSource.SetResult(data.Data.Path);
            }
            else
            {
                _taskCompletionSource.SetResult(null);
            }
        }
    }
}

#endif