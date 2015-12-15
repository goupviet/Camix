using ScreenWorks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Camix
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string OutPath;

        public MainWindow()
        {
            InitializeComponent();

            AvailableWebCams = new ObservableCollection<WebCam>(WebCam.Enumerate());

            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, (s, e) => Refresh()));

            if (AvailableWebCams.Count > 0) SelectedWebCam = AvailableWebCams[0];

            DataContext = this;

            OutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Camix");
        }

        void Refresh()
        {
            AvailableWebCams.Clear();

            foreach (var Cam in WebCam.Enumerate())
                AvailableWebCams.Add(Cam);

            if (AvailableWebCams.Count > 0) SelectedWebCam = AvailableWebCams[0];
        }

        public ObservableCollection<WebCam> AvailableWebCams { get; private set; }

        public WebCam SelectedWebCam = null;

        public WebCam _SelectedWebCam
        {
            get { return SelectedWebCam; }
            set
            {
                if (SelectedWebCam != value)
                {
                    SelectedWebCam = value;
                    OnPropertyChanged("_SelectedWebCam");
                }
            }
        }

        public ImageFormat SelectedImageFormat = ImageFormat.Png;

        public ImageFormat[] ImageFormats
        {
            get
            {
                return new ImageFormat[]
                {
                    ImageFormat.Png,
                    ImageFormat.Jpeg,
                    ImageFormat.Bmp,
                    ImageFormat.Tiff,
                    ImageFormat.Wmf,
                    ImageFormat.Exif,
                    ImageFormat.Gif,
                    ImageFormat.Icon,
                    ImageFormat.Emf
                };
            }
        }

        public ImageFormat _SelectedImageFormat
        {
            get { return SelectedImageFormat; }
            set
            {
                if (SelectedImageFormat != value)
                {
                    SelectedImageFormat = value;
                    OnPropertyChanged("_SelectedImageFormat");
                }
            }
        }

        void OnPropertyChanged(string e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(e));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void Capture(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(OutPath)) Directory.CreateDirectory(OutPath);

            if (SelectedWebCam != null)
            {
                string FileName = null;
                string Extension = SelectedImageFormat == ImageFormat.Icon ? "ico"
                    : SelectedImageFormat == ImageFormat.Jpeg ? "jpg"
                    : SelectedImageFormat.ToString().ToLower();
                bool SaveToClipboard = ToClipboard.IsChecked.Value;

                if (!SaveToClipboard)
                    FileName = Path.Combine(OutPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "." + Extension);

                Bitmap BMP = SelectedWebCam.TakePicture();

                // Save to Disk or Clipboard
                if (BMP != null)
                {
                    if (SaveToClipboard)
                    {
                        BMP.WriteToClipboard(SelectedImageFormat == ImageFormat.Png);
                        Status.Content = "Image Saved to Clipboard";
                    }
                    else
                    {
                        try
                        {
                            BMP.Save(FileName, SelectedImageFormat);
                            Status.Content = "Image Saved to Disk";
                        }
                        catch (Exception E)
                        {
                            Status.Content = "Not Saved. " + E.Message;
                            return;
                        }
                    }
                }
                else Status.Content = "Not Saved - Image taken was Empty";
            }
        }
    }
}
