﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using PixivFS;
using PixivFSCS;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace PixivFSUWP.Data
{
    public static class OverAll
    {
        public static PixivBaseAPI GlobalBaseAPI = new PixivBaseAPI();
        public const string passwordResource = "PixivFSUWPPassword";
        public const string refreshTokenResource = "PixivFSUWPRefreshToken";

        public static RecommendIllustsCollection RecommendList { get; private set; } = new RecommendIllustsCollection();
        public static BookmarkIllustsCollection BookmarkList { get; set; }

        public static void RefreshRecommendList()
        {
            RecommendList.StopLoading();
            RecommendList.Clear();
            RecommendList = new RecommendIllustsCollection();
        }

        static async Task<MemoryStream> downloadImage(string Uri)
        {
            var resStream = await Task.Run(() => new PixivAppAPI(GlobalBaseAPI).csfriendly_no_auth_requests_call_stream("GET",
                  Uri, new List<Tuple<string, string>>() { ("Referer", "https://app-api.pixiv.net/").ToTuple() })
                  .ResponseStream);
            var memStream = new MemoryStream();
            await resStream.CopyToAsync(memStream);
            memStream.Position = 0;
            return memStream;
        }

        public static async Task<BitmapImage> LoadImageAsync(string Uri)
        {
            var toret = new BitmapImage();
            var memStream = await downloadImage(Uri);
            await toret.SetSourceAsync(memStream.AsRandomAccessStream());
            memStream.Dispose();
            return toret;
        }

        public static async Task<WriteableBitmap> LoadImageAsync(string Uri, int Width, int Height)
        {
            var toret = new WriteableBitmap(Width, Height);
            var memStream = await downloadImage(Uri);
            await toret.SetSourceAsync(memStream.AsRandomAccessStream());
            memStream.Dispose();
            return toret;
        }

        public static async Task<byte[]> ImageToBytes(WriteableBitmap Source)
        {
            byte[] toret;
            using (var stream = Source.PixelBuffer.AsStream())
            {
                toret = new byte[stream.Length];
                await stream.ReadAsync(toret, 0, toret.Length);
            }
            return toret;
        }

        public static async Task<WriteableBitmap> BytesToImage(byte[] Source, int Width, int Height)
        {
            WriteableBitmap toret = new WriteableBitmap(Width, Height);
            using (var stream = toret.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(Source, 0, Source.Length);
            }
            return toret;
        }

        //展示一个新的窗口
        public static async Task ShowNewWindow(Type Page, object Parameter)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(Page, Parameter);
                Window.Current.Content = frame;
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }
    }
}
