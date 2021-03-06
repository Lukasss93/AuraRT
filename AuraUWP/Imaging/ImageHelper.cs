using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;

namespace AuraUWP.Imaging
{
    public class ImageHelper
    {
        public static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

            bitmapImage.SetSource(stream);

            return bitmapImage;
        }

        /// <summary>
        /// Esegue il rendering di un FrameworkElement
        /// </summary>
        public async Task<WriteableBitmap> Screenshot(FrameworkElement ele)
        {
            // Render some UI to a RenderTargetBitmap
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(ele);

            // Get the pixel buffer and copy it into a WriteableBitmap
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var width = renderTargetBitmap.PixelWidth;
            var height = renderTargetBitmap.PixelHeight;
			var wbmp = await BitmapFactory.FromPixelBuffer(pixelBuffer, width, height);

            return wbmp;
        }

        /// <summary>
        /// Salva un WriteableBitmap come jpeg nel telefono
        /// </summary>
        public async Task SaveWriteableBitmapAsJpeg(WriteableBitmap bitmap, string name)
        {
            // Create file in Pictures library and write jpeg to it
            var outputFile = await KnownFolders.PicturesLibrary.CreateFileAsync(name + ".jpg", CreationCollisionOption.GenerateUniqueName);
            using(var writeStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await EncodeWriteableBitmap(bitmap, writeStream, BitmapEncoder.JpegEncoderId);
            }
        }

        private static async Task EncodeWriteableBitmap(WriteableBitmap bmp, IRandomAccessStream writeStream, Guid encoderId)
        {
            // Copy buffer to pixels
            byte[] pixels;
            using(var stream = bmp.PixelBuffer.AsStream())
            {
                pixels = new byte[(uint)stream.Length];
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            // Encode pixels into stream
            var encoder = await BitmapEncoder.CreateAsync(encoderId, writeStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
               (uint)bmp.PixelWidth, (uint)bmp.PixelHeight,
               96, 96, pixels);
            await encoder.FlushAsync();
        }

        public static async Task SaveImageAsyncFromUri(string filename, StorageFolder folder, Uri url, CreationCollisionOption option)
        {

            if(folder != null)
            {
                StorageFile file = await folder.CreateFileAsync(filename, option);

                HttpClient client = new HttpClient();
                byte[] fileContent = await client.GetByteArrayAsync(url);

                Stream fileStream = await file.OpenStreamForWriteAsync();
                fileStream.Write(fileContent, 0, fileContent.Length);
                fileStream.Flush();
                fileStream.Dispose();
            }
        }
    }
}
