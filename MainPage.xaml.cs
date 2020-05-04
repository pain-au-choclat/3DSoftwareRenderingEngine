using _3DSoftwareRenderingEngine.Classes;
using System;
using System.Numerics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace _3DSoftwareRenderingEngine
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device _device;
        private Mesh[] _meshes;
        DateTime _previousDate;
        readonly Camera _camera = new Camera();

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap(640, 480);

            _device = new Device(bmp);

            frontBuffer.Source = bmp;

            _device = new Device(bmp);

            _meshes = _device.LoadJSONFileAsync("untitled.txt");

            _camera.Position = new Vector3(0, 0, 10.0f);
            _camera.Target = Vector3.Zero;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            // Calculate FPS
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - _previousDate).TotalMilliseconds;
            _previousDate = now;

            fps.Text = string.Format("{0:0.00} fps", currentFps);

            // Redering loop
            _device.Clear(0, 0, 0, 255);

            foreach (var mesh in _meshes)
            {
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
            }

            _device.Render(_camera, _meshes);
            _device.Present();
        }

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
