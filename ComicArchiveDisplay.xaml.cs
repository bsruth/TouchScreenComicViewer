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
	public class ComicListItem {
		public string ItemText { get; set; }
		public BitmapImage ItemBMP { get; set; }
	}

	public partial class ComicArchiveDisplay : UserControl {
		
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		private const long QUOTA_SIZE = 500 * 1024 * 1024; //TODO: const for quota size until I find a better solution

		ComicArchiveManager mComicArchiveMgr;
		//*****************************************
		public ComicArchiveDisplay() {
			InitializeComponent();

			mComicArchiveMgr = new ComicArchiveManager();
			blah = new TranslateTransform();
		}

		//*****************************************
		private void ComicArchiveDisplayPage_Loaded(object sender, RoutedEventArgs e) {
			RefreshComicList();
		}

		//*****************************************
		private void RefreshComicList() {

			ComicArchiveWrapPanel.Children.Clear();


			List<string> comics = mComicArchiveMgr.GetAvailableComics();

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

									ComicListItem cli = new ComicListItem();
									cli.ItemText = comicString;
									//now get the cover image
									cli.ItemBMP = mComicArchiveMgr.GetComicCover(comicString);
									ComicCoverTile cct = new ComicCoverTile();
									cct.Height = 150;
									cct.Width = cct.Height * cli.ItemBMP.PixelWidth / cli.ItemBMP.PixelHeight;
									cct.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
									cct.DataContext = cli;
									cct.MouseLeftButtonUp += new MouseButtonEventHandler(ComicCover_MouseLeftButtonUp);
									ComicArchiveWrapPanel.Children.Add(cct);
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
		private void ComicCover_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

			ComicCoverTile selectedComicCoverTile = (ComicCoverTile)sender;
			string selectedComicFile = ((ComicListItem)(selectedComicCoverTile.DataContext)).ItemText;
			ComicBook openedComic = mComicArchiveMgr.OpenComic(selectedComicFile);
			if (openedComic != null) {
				ComicViewer.SetComic(openedComic);
				LastComicLabel.Content = selectedComicFile;
				ComicViewer.Visibility = System.Windows.Visibility.Visible;

				myStoryboard.Completed += (ex, a) => { 
					ComicViewer.LayoutRoot.Background = new SolidColorBrush(Colors.Black);
					//reset the ComicViewer to stretch mode
					ComicViewer.Width = Double.NaN;
					ComicViewer.Height = Double.NaN;

				};

				ComicViewer.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);

				//adjust the animation so that it seems to come from the tile that
				//was clicked
				ZoomX.From = selectedComicCoverTile.ActualWidth;
				ZoomX.To = this.ActualWidth;
				ZoomY.From = selectedComicCoverTile.ActualHeight;
				ZoomY.To = this.ActualHeight;
				GeneralTransform objGeneralTransform = ((ComicCoverTile)sender).TransformToVisual(Application.Current.RootVisual as UIElement);
				Point point = objGeneralTransform.Transform(new Point(0, 0));
				XLoc.From = point.X - (ComicArchiveWrapPanel.ActualWidth / 2) + ((ComicArchiveScrollViewer.Margin.Left + ComicArchiveScrollViewer.Margin.Right) / 2);
				XLoc.To = 0;
				YLoc.From = point.Y - (ComicArchiveWrapPanel.ActualHeight / 2) + ((ComicArchiveScrollViewer.Margin.Top + ComicArchiveScrollViewer.Margin.Bottom) / 2);
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
				if (!iso2.IncreaseQuotaTo(QUOTA_SIZE)) //50MB
				{
					throw new Exception("Can't store the image.");
				} else {
					return;
				}
			}

			if (dlg.ShowDialog() == true) {
				foreach (FileInfo file in dlg.Files) {
					this.mComicArchiveMgr.AddComicToArchive(file);
				}

				RefreshComicList();
			}
		}

	}
}