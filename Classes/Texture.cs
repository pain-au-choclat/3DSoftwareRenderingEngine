using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace _3DSoftwareRenderingEngine.Classes
{
    public class Texture
    {
        private byte[] _internalBuffer;
        private readonly int _width;
        private readonly int _height;

        public Texture(string filename, int width, int height)
        {
            _width = width;
            _height = height;
            Load(filename);
        }

        async void Load(string filename)
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync(filename);
            //var file = await location.GetFileAsync(Path.Combine($"{location.Path}\\Assets", filename));

            using (var stream = await file.OpenReadAsync())
            {
                var bmp = new WriteableBitmap(_width, _height);
                bmp.SetSource(stream);

                _internalBuffer = bmp.PixelBuffer.ToArray();
            }
        }

        public Color Map(float tu, float tv)
        {
            if (_internalBuffer == null)
            {
                return Color.White;
            }

            int u = Math.Abs((int)(tu * _width) % _width);
            int v = Math.Abs((int)(tv * _height) % _height);

            int pos = (u + v * _width) * 4;
            byte b = _internalBuffer[pos];
            byte g = _internalBuffer[pos + 1];
            byte r = _internalBuffer[pos + 2];
            byte a = _internalBuffer[pos + 3];

            return Color.FromArgb(a, r, g, b);
        }
    }
}
