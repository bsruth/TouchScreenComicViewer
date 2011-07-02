using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.Generic;

namespace TouchScreenComicViewer{
	public class ComicBook : INotifyPropertyChanged {
		//members
		private string _comicBookFileName;
		private Stream _comicBookFileStream;
		private List<string> _filesInComicBook = new List<string>();
		private BitmapImage _coverImage = null;
		private int _currentPageIndex = 0;
		private static string[] VALID_IMAGE_FILE_EXT = { ".jpg" };
		public int CurrentPageNumber {
			get { return _currentPageIndex + 1;}
			protected set { 
				_currentPageIndex = value;
				RaisePropertyChanged("CurrentPageNumber");
			}
		}

		public int TotalPages { get { return _filesInComicBook.Count; } }

		//*****************************************
		public ComicBook(string comicBookFileName) {
			_comicBookFileName = comicBookFileName;
			if (IsFileStreamOpen() == false) {
				OpenFileStream();
			}
		}

		//*****************************************
		public string GetComicFileName() 
		{
			return _comicBookFileName;
		}

		//*****************************************
		public BitmapImage GetCoverImage()
		{
			if (_coverImage == null)
			{
				_coverImage = GetImageFromComicFile(_filesInComicBook[0]);
			}
			return _coverImage;
		}

		//*****************************************
		public BitmapImage GetNextPageImage() {
			int nextPageIndex = _currentPageIndex + 1;
			if (nextPageIndex >= TotalPages) {
				nextPageIndex = 0;
			}

			BitmapImage pageImage = GetImageFromComicFile(_filesInComicBook[nextPageIndex]);
			if (pageImage != null) {
				CurrentPageNumber = nextPageIndex;
			}

			return pageImage;

		}

		//*****************************************
		public BitmapImage GetCurrentPageImage() {
			BitmapImage pageImage = GetImageFromComicFile(_filesInComicBook[_currentPageIndex]);
			if (pageImage != null) {
				CurrentPageNumber = _currentPageIndex;
			}

			return pageImage;

		}

		//*****************************************
		public BitmapImage GetPreviousPageImage() {
			int prevPageIndex = _currentPageIndex - 1;
			if (prevPageIndex < 0) {
				prevPageIndex = TotalPages - 1;
			}
			BitmapImage pageImage = GetImageFromComicFile(_filesInComicBook[prevPageIndex]);
			if (pageImage != null) {
				CurrentPageNumber = prevPageIndex;
			}

			return pageImage;

		}

		//*****************************************
		private BitmapImage GetImageFromComicFile(string imageName)
		{
			if (IsFileStreamOpen() == false) {
				if (OpenFileStream() == false) {
					return null;
				}
			}

			BitmapImage imageFile = null;
			Stream coverFileStream = ZipFileUtilities.GetFileStreamFromZIPFile(_comicBookFileName, imageName);
			if (coverFileStream != null) {
				imageFile = new BitmapImage();
				imageFile.SetSource(coverFileStream);
				coverFileStream.Close();
			}
			return imageFile;
		}

		//*****************************************
		private bool IsFileStreamOpen()
		{
			if( (_comicBookFileStream == null) || (_comicBookFileStream.CanRead == false) )
			{
				return false;
			} else {
					 return true;
			}
		}

		//*****************************************
		private bool OpenFileStream() 
		{
			if (IsFileStreamOpen() == false) {
				_comicBookFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(_comicBookFileName);
				if (_comicBookFileStream == null) {
					return false;
				}
				string[] filesInArchive = ZipFileUtilities.GetZipContents(_comicBookFileStream);
				_filesInComicBook.Clear();
				//remove all non-image files from the list of files
				foreach (string fileName in filesInArchive) {
					foreach (string ext in VALID_IMAGE_FILE_EXT) {
						if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) {
							_filesInComicBook.Add(fileName);
							break;
						}
					}
				}
				if (_filesInComicBook.Count <= 0) {
					_comicBookFileStream.Close();
					return false;
				}
			}

			//TODO: we need to close the stream for now since the zip file utilities require
			//the stream to close all the time
			_comicBookFileStream.Close();
			return true;
		}



		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		void RaisePropertyChanged(string propertyName) {
			var handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}      

		#endregion
	}
}
