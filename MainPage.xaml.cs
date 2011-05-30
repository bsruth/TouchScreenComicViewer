using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Interop;
using System.Text;
using System.Windows.Resources;

namespace TouchScreenComicViewer
{
    public partial class MainPage : UserControl
    {
        private const long QUOTA_SIZE = 52428800; //TODO: const for quota size until I find a better solution
        private const double _originalWidth = 970;
        private const double _originalHeight = 1470;
        private const double _originalAspectRatio =
                _originalWidth / _originalHeight;
        private List<string> fileList = new List<string>();
        private string compressedFilename = "";
        private int currentIndex = 0;
        private int totalPages = 0;
        private Point startPoint; //start point for beginning of a mouse down event
        private const int swipePixelLength = 50; //number of pixels needed to trigger a swipe event.
        //private double scaleX = 1.0;
        //private double scaleY = 1.0;
        private bool touchEventActive = false;

        //*****************************************
        public MainPage()
        {

            InitializeComponent();

            SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);
            Application.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
            //Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);


        }

        //*****************************************
        //Can't use touch events since they don't work in full screen mode
        //converted the touch events into mouse events
        /*void Touch_FrameReported(object sender, TouchFrameEventArgs args) {
            TouchPoint primaryTouchPoint = args.GetPrimaryTouchPoint(null);


            // Inhibit mouse promotion
                        if (primaryTouchPoint != null && primaryTouchPoint.Action == TouchAction.Down)
                        {

                        //    args.SuspendMousePromotionUntilTouchUp();
                        }

            TouchPointCollection touchPoints =
                args.GetTouchPoints(null);

            foreach (TouchPoint tp in touchPoints) {
                int id = tp.TouchDevice.Id;

                switch (tp.Action) {
                    case TouchAction.Down:
                        this.startPoint = tp;
                                                this.touchEventActive = true;
                        break;

                    case TouchAction.Move:
                                                if (this.touchEventActive == false)
                                                {
                                                        break;
                                                }
                                                if (tp.Position.X < (this.startPoint.Position.X - 100))
                                                {
                                                        //swipe left
                                                        currentIndex--;
                                                        if (currentIndex < 0)
                                                        {
                                                                currentIndex = this.fileList.Count - 1;
                                                        }
                                                        string data = String.Empty;
                                                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                                                        {
                                                                IsolatedStorageFileStream file = isf.OpenFile(this.fileList[currentIndex], FileMode.Open);
                                                                System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                                                                bmp.SetSource(file);
                                                                this.MainDisplayImage.Source = bmp;
                                                                this.MainDisplayImage.Visibility = System.Windows.Visibility.Visible;
                                                                file.Close();
                                                                this.touchEventActive = false;
                                                        }

                                                }
                                                else if (tp.Position.X > (this.startPoint.Position.X + 100))
                                                {
                                                        //swipe right
                                                        currentIndex++;
                                                        if (currentIndex >= this.fileList.Count)
                                                        {
                                                                currentIndex = 0;
                                                        }
                                                        string data = String.Empty;
                                                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                                                        {
                                                                IsolatedStorageFileStream file = isf.OpenFile(this.fileList[currentIndex], FileMode.Open);
                                                                System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                                                                bmp.SetSource(file);
                                                                this.MainDisplayImage.Source = bmp;
                                                                this.MainDisplayImage.Visibility = System.Windows.Visibility.Visible;
                                                                file.Close();
                                                                this.touchEventActive = false;
                                                        }

                                                }
                                                break;
                    case TouchAction.Up:
                                                this.touchEventActive = false;
                        break;
                }
            }
        }*/

        //*****************************************
        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < _originalWidth ||
                    e.NewSize.Height < _originalHeight)
            {
                // don't shrink
                PageScale.ScaleX = 1.0;
                PageScale.ScaleY = 1.0;
            }
            else
            {
                // resize keeping aspect ratio the same
                if (e.NewSize.Width / e.NewSize.Height > _originalAspectRatio)
                {
                    // height is our constraining property
                    PageScale.ScaleY = e.NewSize.Height / _originalHeight;
                    PageScale.ScaleX = PageScale.ScaleY;
                }
                else
                {
                    // either width is our constraining property, or the user
                    // managed to nail our aspect ratio perfectly.
                    PageScale.ScaleX = e.NewSize.Width / _originalWidth;
                    PageScale.ScaleY = PageScale.ScaleX;
                }
            }
        }

        //*****************************************
        private void Content_FullScreenChanged(object sender, EventArgs e)
        {
            if (Application.Current.Host.Content.IsFullScreen == true)
            {
                this.FullScreenBtn.Content = "Exit Full Screen";
            }
            else
            {
                this.FullScreenBtn.Content = "Full Screen";
            }
        }


        //*****************************************
        private void OpenComicBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Zipped Comic Books (*.CBZ)|*.CBZ";

            //TODO: I need to find a way to open a file and be
            //able to adjust the quota without having to prompt
            //the user multiple times.
            IsolatedStorageFile iso2 = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso2.Quota < QUOTA_SIZE)
            {
                if (!iso2.IncreaseQuotaTo(QUOTA_SIZE)) //50MB
                {
                    throw new Exception("Can't store the image.");
                }
                else
                {
                    return;
                }
            }

            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (Stream fileStream = dlg.File.OpenRead())
                    {
                        this.fileList = GetZipContents(fileStream).ToList<string>();
                        fileStream.Close();
                        this.compressedFilename = dlg.File.Name;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                if (CopyFileToIsoStorage(dlg.File) == false)
                {
                    //TODO: error message
                    return;
                }
                currentIndex = 0;
                DisplayImage(currentIndex);
            }

            this.totalPages = this.fileList.Count;
            this.totalPagesLbl.Content = (totalPages);
            this.currentPageNumLbl.Content = (currentIndex + 1);
        }

        //*****************************************
        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
            if (Application.Current.Host.Content.IsFullScreen == true)
            {
                this.FullScreenBtn.Content = "Exit Full Screen";
            }
            else
            {
                this.FullScreenBtn.Content = "Full Screenfff";
            }

        }

        //*****************************************
        private void ExitProgramBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        //*****************************************
        private void CloseMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.FullScreenBtn.Visibility == System.Windows.Visibility.Visible)
            {
                this.FullScreenBtn.Visibility = System.Windows.Visibility.Collapsed;
                this.MenuGrid.Height = this.CloseMenuBtn.Height;
                System.Windows.Media.SolidColorBrush opacityBrush = new SolidColorBrush(Color.FromArgb(55, 0, 0, 0));
                this.CloseMenuBtn.OpacityMask = opacityBrush;
                this.CloseMenuBtn.Content = "Show Menu";

            }
            else
            {
                this.FullScreenBtn.Visibility = System.Windows.Visibility.Visible;
                this.MenuGrid.Height = 280;
                System.Windows.Media.SolidColorBrush opacityBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                this.CloseMenuBtn.OpacityMask = opacityBrush;
                this.CloseMenuBtn.Content = "Close Menu";
            }
        }

        //*****************************************
        private void MainDisplayImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.startPoint = e.GetPosition(null);
            this.touchEventActive = true;
        }

        //*****************************************
        void MainDisplayImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.touchEventActive = false;
        }

        //*****************************************
        private void MainDisplayImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.touchEventActive == false)
            {
                return;
            }
            if (e.GetPosition(null).X < (this.startPoint.X - swipePixelLength))
            {
                //swipe left
                currentIndex++;
                if (currentIndex >= this.fileList.Count)
                {
                    currentIndex = 0;
                }
                this.currentPageNumLbl.Content = (currentIndex + 1).ToString();
                DisplayImage(currentIndex);
                touchEventActive = false;

            }
            else if (e.GetPosition(null).X > (this.startPoint.X + swipePixelLength))
            {
                //swipe right
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = this.fileList.Count - 1;
                }
                this.currentPageNumLbl.Content = (currentIndex + 1).ToString();
                DisplayImage(currentIndex);
                touchEventActive = false;
            }
        }

        /// <summary>
        /// Reads the file names from the header of the zip file
        /// </summary>
        /// <param name="zipStream">The stream to the zip file</param>
        /// <returns>An array of file names stored within the zip file. These file names may also include relative paths.</returns>
        public static string[] GetZipContents(System.IO.Stream zipStream)
        {

            List<string> names = new List<string>();
            BinaryReader reader = new BinaryReader(zipStream);
            while (reader.ReadUInt32() == 0x04034b50)
            {

                // Skip the portions of the header we don't care about
                reader.BaseStream.Seek(14, SeekOrigin.Current);
                uint compressedSize = reader.ReadUInt32();
                uint uncompressedSize = reader.ReadUInt32();
                int nameLength = reader.ReadUInt16();
                int extraLength = reader.ReadUInt16();
                byte[] nameBytes = reader.ReadBytes(nameLength);
                names.Add(Encoding.UTF8.GetString(nameBytes, 0, nameLength));
                reader.BaseStream.Seek(extraLength + compressedSize, SeekOrigin.Current);

            }
            // Move the stream back to the begining
            zipStream.Seek(0, SeekOrigin.Begin);
            return names.ToArray();

        }


        //*****************************************
        //returns the number of bytes removed from the store
        private long RemoveOldestFileFromIsoStore(IsolatedStorageFile isoStore)
        {

            string[] fileListing = isoStore.GetFileNames();
            if (fileListing.Length == 0)
            {
                //nothing to delete
                return 0;
            }

            string oldestFileName = fileListing[0];
            DateTimeOffset oldestAccessDate = isoStore.GetLastAccessTime(fileListing[0]);
            foreach (string file in fileListing)
            {
                DateTimeOffset accessDate = isoStore.GetLastAccessTime(file);
                if (accessDate < oldestAccessDate)
                {
                    oldestAccessDate = accessDate;
                    oldestFileName = file;
                }
            }

            IsolatedStorageFileStream oldestFile = isoStore.OpenFile(oldestFileName, FileMode.Open);
            long numBytesRemoved = oldestFile.Length;
            oldestFile.Close();
            isoStore.DeleteFile(oldestFileName);
            return numBytesRemoved;
        }

        //*****************************************
        private void DisplayImage(int imageIndex)
        {
            //string data = String.Empty;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream file = isf.OpenFile(this.compressedFilename, FileMode.Open);
                StreamResourceInfo zipInfo = new StreamResourceInfo(file, null);
                StreamResourceInfo streamInfo = Application.GetResourceStream(zipInfo, new Uri(this.fileList[imageIndex], UriKind.Relative));
                Stream fileStream = streamInfo.Stream;
                System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.SetSource(fileStream);
                this.MainDisplayImage.Source = bmp;
                this.MainDisplayImage.Visibility = System.Windows.Visibility.Visible;
                file.Close();
            }
        }

        //*****************************************
        private bool CopyFileToIsoStorage(FileInfo fileToCopy)
        {
            // Save all selected files into application's isolated storage
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            
                
                //don't copy a file if it is already in the cache
                if (iso.FileExists(fileToCopy.Name) == false)
                {
                    Int64 spaceToAdd = fileToCopy.Length;

                    //the new file isn't big enough to fit, we need to increase
                    //the quota
                    if (iso.AvailableFreeSpace < spaceToAdd)
                    {
                        //try and increase without removing any files from cache
                        if (iso.IncreaseQuotaTo(iso.AvailableFreeSpace + spaceToAdd) == false)
                        {
                            //try and increase quota so the new file barely fits if all other data
                            //cleared
                            if (iso.IncreaseQuotaTo(spaceToAdd + 1) == false)
                            {
                                return false; //total failure
                            }
                        }
                    }

                    //there is enough space, but we need to remove some files
                    //evidently we couldn't allocate any more space
                    if (iso.AvailableFreeSpace < spaceToAdd)
                    {
                        //see if we can free up space by removing some files.
                        long numBytesRemoved = 0;
                        do
                        {
                            numBytesRemoved = RemoveOldestFileFromIsoStore(iso);
                        } while ((numBytesRemoved > 0) && (iso.AvailableFreeSpace < spaceToAdd));

                    }

                    

                    //finally, copy file
                    using (Stream fileStream = fileToCopy.OpenRead())
                    {
                        using (IsolatedStorageFileStream isoStream =
                                new IsolatedStorageFileStream(fileToCopy.Name, FileMode.Create, iso))
                        {

                            // Read and write the data block by block until finish
                            while (true)
                            {
                                byte[] buffer = new byte[100001];
                                int count = fileStream.Read(buffer, 0, buffer.Length);
                                if (count > 0)
                                {
                                    isoStream.Write(buffer, 0, count);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            return true;
        }
    } //main page class
}// namespace