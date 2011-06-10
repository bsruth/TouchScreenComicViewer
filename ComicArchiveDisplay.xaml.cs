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

namespace TouchScreenComicViewer {
	public class ComicListItem {
		public string ItemText { get; set; }
		public BitmapImage ItemBMP { get; set; }
	}

	public partial class ComicArchiveDisplay : UserControl {
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		MainPage comicDisplay;
		public ComicArchiveDisplay() {
			InitializeComponent();
		}

		private void ComicArchiveDisplayPage_Loaded(object sender, RoutedEventArgs e) {
			ComicArchiveListBox.Items.Clear();
			List<string> comics = IsoStorageUtilities.GetIsolatedStorageFilesWithExtension(COMIC_ARCHIVE_ZIP_EXT);
			//ComicArchiveListBox.ItemsSource = comics;
			foreach (string comic in comics) {
				ComicListItem cli = new ComicListItem();
				cli.ItemText = comic;
				//now get the cover image
				Stream comicStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(comic);
				string[] comicFiles = ZipFileUtilities.GetZipContents(comicStream);
				comicStream.Close();
				if (comicFiles.GetLength(0) > 0) {
					Stream coverFileStream = ZipFileUtilities.GetFileStreamFromZIPFile(comic, comicFiles[0]);
					if (coverFileStream != null) {
						cli.ItemBMP = new BitmapImage();
						cli.ItemBMP.SetSource(coverFileStream);
						coverFileStream.Close();
					}
				}

				ComicArchiveListBox.Items.Add(cli);
			}
			comicDisplay = new MainPage(this);
		}

		private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			ComicListItem selectedItem = ((ComicListItem)ComicArchiveListBox.SelectedItem);
			comicDisplay.SetComic(selectedItem.ItemText);
			((UserControlContainer)Application.Current.RootVisual).SwitchControl(comicDisplay);
		}

	}
}