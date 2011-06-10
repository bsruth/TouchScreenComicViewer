﻿using System;
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

namespace TouchScreenComicViewer {
	public class ComicListItem {
		public string ItemText { get; set; }
		public BitmapImage ItemBMP { get; set; }
	}

	public partial class ComicArchiveDisplay : UserControl {
		
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		private const long QUOTA_SIZE = 500 * 1024 * 1024; //TODO: const for quota size until I find a better solution

		//*****************************************
		public ComicArchiveDisplay() {
			InitializeComponent();
		}

		//*****************************************
		private void ComicArchiveDisplayPage_Loaded(object sender, RoutedEventArgs e) {
			RefreshComicList();
		}

		//*****************************************
		private void RefreshComicList() {
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
		}

		//*****************************************
		private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			ComicListItem selectedItem = ((ComicListItem)ComicArchiveListBox.SelectedItem);
			ComicViewer.SetComic(selectedItem.ItemText);
			ComicViewer.Visibility = System.Windows.Visibility.Visible;
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

					if (IsoStorageUtilities.CopyFileToIsoStorage(file) == false) {
						//TODO: error message
						return;
					}
				}

				RefreshComicList();
			}
		}

	}
}