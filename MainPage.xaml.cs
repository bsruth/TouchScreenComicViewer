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
using System.Windows.Media.Imaging;

namespace TouchScreenComicViewer
{
	public partial class MainPage : UserControl
	{
		private const double _originalWidth = 970;
		private const double _originalHeight = 1470;
		private const double _originalAspectRatio =
						_originalWidth / _originalHeight;

		private Point _startPoint; //start point for beginning of a mouse down event
		private Point _panStartPoint; //modified start point for panning translation
		private const int swipePixelLength = 50; //number of pixels needed to trigger a swipe event.

		public event RoutedEventHandler ComicClosed;
		private bool _isZoomed = false;
		private bool touchEventActive = false;

		private ComicBook _currentComicBook;

		//*****************************************
		public MainPage()
		{

			InitializeComponent();

			SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);
			Application.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
			this.ViewingMenu.ComicClosedButtonClicked += new RoutedEventHandler(CloseComicBtn_Click);

			System.Windows.Media.SolidColorBrush opacityBrush = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
			this.PrevButton.OpacityMask = opacityBrush;
			this.NextButton.OpacityMask = opacityBrush;

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
		public void SetComic(ComicBook comicToOpen)
		{

			_currentComicBook = comicToOpen;
			DataContext = _currentComicBook;
			DisplayImage(_currentComicBook.GetCurrentPageImage());
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
				this.FullScreenBtn.Content = "Full Screen";
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
				System.Windows.Media.SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
				this.MenuGrid.Background = backgroundBrush;
				System.Windows.Media.SolidColorBrush opacityBrush = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
				this.CloseMenuBtn.OpacityMask = opacityBrush;
				this.CloseMenuBtn.Content = "Show Menu";

			}
			else
			{
				this.FullScreenBtn.Visibility = System.Windows.Visibility.Visible;
				this.MenuGrid.Height = 335;
				System.Windows.Media.SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromArgb(157, 0,0, 0));
				this.MenuGrid.Background = backgroundBrush;
				System.Windows.Media.SolidColorBrush opacityBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
				this.CloseMenuBtn.OpacityMask = opacityBrush;
				this.CloseMenuBtn.Content = "Close Menu";
			}
		}

		//*****************************************
		private void PrevButton_Click(object sender, RoutedEventArgs e) {
			GoToPreviousPage();
		}

		//*****************************************
		private void NextButton_Click(object sender, RoutedEventArgs e) {
			GoToNextPage();
		}

		//*****************************************
		private void MainDisplayImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this._startPoint = e.GetPosition(null);
			if (_isZoomed) {
				_panStartPoint = _startPoint;
				_panStartPoint.X -= ImagePan.X;
				_panStartPoint.Y -= ImagePan.Y;
			}
			this.touchEventActive = true;
		}

		//*****************************************
		void MainDisplayImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (ViewingMenu.IsMenuExpanded() == true) {
				ViewingMenu.CloseMenuWithAnimation();
			} else {

				if (this._startPoint == e.GetPosition(null)) {
					ZoomImage(!_isZoomed, e.GetPosition(MainDisplayImage));
				}
			}
			this.touchEventActive = false;
		}

		//*****************************************
		private void ZoomImage(bool zoom, Point zoomPoint)
		{
			if (_isZoomed != zoom) {
				ImageZoomScale.CenterX = zoomPoint.X;
				ImageZoomScale.CenterY = zoomPoint.Y;
				if (_isZoomed == false) {
					ImageZoomScale.ScaleX *= 3;
					ImageZoomScale.ScaleY *= 3;
				} else {
					ImageZoomScale.ScaleX /= 3;
					ImageZoomScale.ScaleY /= 3;
				}
				ImageZoomScale.Transform(zoomPoint);
				_isZoomed = zoom;
			}

			ImagePan.X = 0;
			ImagePan.Y = 0;
		}

		//*****************************************
		private void MainDisplayImage_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.touchEventActive == false)
			{
				return;
			}
			if (_isZoomed) {
				Point mousePosition = e.GetPosition(null);
				System.Diagnostics.Debug.WriteLine("X: " + mousePosition.X + " Y: " + mousePosition.Y);
				//pan zoomed image
				PanImage(_panStartPoint, mousePosition);
			} else {
				//change pages
				if (e.GetPosition(null).X < (this._startPoint.X - swipePixelLength)) {
					//swipe left
					GoToNextPage();
					touchEventActive = false;

				} else if (e.GetPosition(null).X > (this._startPoint.X + swipePixelLength)) {
					//swipe right
					GoToPreviousPage();
					touchEventActive = false;
				}
			}
		}

		//*****************************************
		private void GoToNextPage() 
		{
			//swipe left
			BitmapImage comicImage = _currentComicBook.GetNextPageImage();
			if (comicImage != null) {
				DisplayImage(comicImage);
			}
		}

		//*****************************************
		private void GoToPreviousPage()
		{
			BitmapImage comicImage = _currentComicBook.GetPreviousPageImage();
			if (comicImage != null) {
				DisplayImage(comicImage);
			}
		}

		//*****************************************
		private void PanImage(Point startPoint, Point endPoint) {
			ImagePan.X = (endPoint.X - startPoint.X);
			ImagePan.Y = (endPoint.Y - startPoint.Y);
			ImagePan.Transform(new Point(0,0));
		}

		//*****************************************
		private void DisplayImage(BitmapImage imageToDisplay)
		{
			if (_isZoomed) {
				ZoomImage(false, new Point(0, 0));
				ZoomImage(true, new Point(50, 0));
			}

			this.MainDisplayImage.Source = imageToDisplay;
			this.MainDisplayImage.Visibility = System.Windows.Visibility.Visible;
		}

		//*****************************************
		private void CloseComicBtn_Click(object sender, RoutedEventArgs e) {
			if (_isZoomed) {
				ZoomImage(false, new Point(0, 0));
			}

            _currentComicBook.CloseComic();

			var handler = ComicClosed;
			if (handler != null) {
				handler(sender, e);
			}
		}

		private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e) {
			touchEventActive = false;
		}
		
	} //main page class
}// namespace