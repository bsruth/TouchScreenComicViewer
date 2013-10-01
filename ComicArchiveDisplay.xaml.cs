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
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Windows.Threading;

namespace TouchScreenComicViewer {

	public partial class ComicArchiveDisplay : UserControl {
		
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		private const long QUOTA_SIZE = 500 * 1024 * 1024; //TODO: const for quota size until I find a better solution

		private ComicCoverTile _currentOpenComicCoverTile;

		ComicArchiveManager mComicArchiveMgr;
		//*****************************************
		public ComicArchiveDisplay() {
			InitializeComponent();

			mComicArchiveMgr = new ComicArchiveManager();
			blah = new TranslateTransform();
			ComicViewer.ComicClosed += new RoutedEventHandler(ComicViewer_ComicClosed);
			LoadingComicsDisp.FileLoadingCompleted += (senderFL, eFL) =>
			{
                this.ComicArchiveList.Visibility = System.Windows.Visibility.Visible;
				this.openComicBtn.Visibility = System.Windows.Visibility.Visible;
				this.exitBtn.Visibility = System.Windows.Visibility.Visible;
				RefreshComicList();
			};
		}

		//*****************************************
		public void CloseComicAnimationCompleted(object sender, EventArgs e) {
			ComicViewer.Visibility = System.Windows.Visibility.Collapsed;
			myStoryboard.Completed -= CloseComicAnimationCompleted;
		}
		//*****************************************
		void ComicViewer_ComicClosed(object sender, RoutedEventArgs e) {

			ComicViewer.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);

			myStoryboard.Completed += CloseComicAnimationCompleted;
			//adjust the animation so that it seems to come from the tile that
			//was clicked
			ZoomX.To = _currentOpenComicCoverTile.ActualWidth;
			ZoomX.From = this.ActualWidth;
			ZoomY.To = _currentOpenComicCoverTile.ActualHeight;
			ZoomY.From = this.ActualHeight;
			GeneralTransform objGeneralTransform = _currentOpenComicCoverTile.TransformToVisual(this as UIElement);
			Point point = objGeneralTransform.Transform(new Point(-this.ActualWidth/2, -this.ActualHeight/2));
            XLoc.To = point.X + (_currentOpenComicCoverTile.ActualWidth / 2);
			XLoc.From = 0;
            YLoc.To = point.Y + (_currentOpenComicCoverTile.ActualHeight / 2);
			YLoc.From = 0;

			try {
				myStoryboard.Begin();
			} catch (Exception ex) {
				string blah = ex.ToString();
			}
		}

		//*****************************************
		private void ComicArchiveDisplayPage_Loaded(object sender, RoutedEventArgs e) {
			RefreshComicList();
		}

		//*****************************************
		private void RefreshComicList() {

            ComicArchiveList.Items.Clear();


			List<string> comics = mComicArchiveMgr.GetAvailableComics();

			LoadingProgressBar.Value = 0;
			LoadingProgressBar.Maximum = comics.Count;
			if(comics.Count == 0 ){
				LoadingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
			} else {
				LoadingProgressBar.Visibility = System.Windows.Visibility.Visible;
			}

			//the threading is so that one comic loads at a time and
			//the UI doesn't look like it locked up as the list is refreshed
			BackgroundWorker comicLoader = new BackgroundWorker();
			Dispatcher myDisp = Application.Current.RootVisual.Dispatcher;
			DispatcherSynchronizationContext myDispSync = new DispatcherSynchronizationContext(myDisp); //needed to dispatch synchronously
			comicLoader.WorkerReportsProgress = false;
			comicLoader.DoWork += (sender, e) =>
			{
				foreach (string comic in comics) {
					try {
						BackgroundWorker worker = sender as BackgroundWorker;

						//wait on the UI to finish loading this comic
						//before we move on to the next comic in the list
						myDispSync.Send((obj) =>
							{
								try {
									string comicString = obj as string;
                                    var currentComic = mComicArchiveMgr.GetComic(comicString);
                                    ComicArchiveList.Items.Add(currentComic);
									LoadingProgressBar.Value += 1;
									if (LoadingProgressBar.Value == LoadingProgressBar.Maximum) {
										LoadingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
									}
								} catch (Exception ex) {
									string exceptionString = ex.ToString();
								}
							}, comic);

					} catch (Exception ex) {
						string exceptionString = ex.ToString();
					}
				}
			};

			comicLoader.RunWorkerAsync();
			



			LastComicLabel.Content = mComicArchiveMgr.GetLastOpenedComic();
		}

		//*****************************************
		private void ComicCover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			ComicCoverTile selectedComicCoverTile = (ComicCoverTile)sender;
			//shift the tile so it looks like it was pushed
			TranslateTransform quickShift = new TranslateTransform();
			quickShift.X = 3;
			quickShift.Y = 3;
			selectedComicCoverTile.RenderTransform = quickShift;
			

		}

		//*****************************************
		private void OpenComicAnimationCompleted(Object sender, EventArgs e) {
			ComicViewer.LayoutRoot.Background = new SolidColorBrush(Colors.Black);
			//reset the ComicViewer to stretch mode
			ComicViewer.Width = Double.NaN;
			ComicViewer.Height = Double.NaN;
			myStoryboard.Completed -= OpenComicAnimationCompleted;
		}

		//*****************************************
		private void ComicCover_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

			_currentOpenComicCoverTile = (ComicCoverTile)sender;

			//put the tile back where it was
			TranslateTransform quickShift = new TranslateTransform();
			quickShift.X = -3;
			quickShift.Y = -3;
			_currentOpenComicCoverTile.RenderTransform = quickShift;

			string selectedComicFile = ((ComicBook)(_currentOpenComicCoverTile.DataContext)).GetComicFileName();
			ComicBook openedComic = mComicArchiveMgr.OpenComic(selectedComicFile);
			if (openedComic != null) {
                openedComic.OpenComic();
				ComicViewer.SetComic(openedComic);
				LastComicLabel.Content = selectedComicFile;
				ComicViewer.Visibility = System.Windows.Visibility.Visible;

				myStoryboard.Completed += OpenComicAnimationCompleted;


				ComicViewer.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);

				//adjust the animation so that it seems to come from the tile that
				//was clicked
				ZoomX.From = _currentOpenComicCoverTile.ActualWidth;
				ZoomX.To = this.ActualWidth;
				ZoomY.From = _currentOpenComicCoverTile.ActualHeight;
				ZoomY.To = this.ActualHeight;
				GeneralTransform objGeneralTransform = ((ComicCoverTile)sender).TransformToVisual(this as UIElement);
				Point point = objGeneralTransform.Transform(new Point(-this.ActualWidth/2, -this.ActualHeight/2 ));
                XLoc.From = point.X + (_currentOpenComicCoverTile.ActualWidth/2);
				XLoc.To = 0;
                YLoc.From = point.Y + (_currentOpenComicCoverTile.ActualHeight/2);
				YLoc.To = 0;

				try {
					myStoryboard.Begin();
				} catch (Exception ex) {
					string blah = ex.ToString();
				}
			}
		}

		//*****************************************
		private void OpenComicBtn_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Zipped Comic Books (*.CBZ)|*.CBZ";
			dlg.Multiselect = true;

			//TODO: I need to find a way to open a file and be
			//able to adjust the quota without having to prompt
			//the user multiple times.
			IsolatedStorageFile iso2 = IsolatedStorageFile.GetUserStoreForApplication();
			if (iso2.Quota < QUOTA_SIZE) {
				if (!iso2.IncreaseQuotaTo(QUOTA_SIZE))
				{
					throw new Exception("Can't store the image.");
				} else {
					return;
				}
			}

			if (dlg.ShowDialog() == true) {

				IEnumerator<FileInfo> fileEnumerator = dlg.Files.GetEnumerator();
                this.ComicArchiveList.Visibility = System.Windows.Visibility.Collapsed;
				this.openComicBtn.Visibility = System.Windows.Visibility.Collapsed;
				this.exitBtn.Visibility = System.Windows.Visibility.Collapsed;
				LoadingComicsDisp.Visibility = System.Windows.Visibility.Visible;
				LoadingComicsDisp.LoadFiles(fileEnumerator, mComicArchiveMgr);
			}
		}

		private void CloseBtn_Clicked(object sender, RoutedEventArgs e) {
			System.Windows.Browser.HtmlPage.Window.Invoke("close");
		}

	}
}