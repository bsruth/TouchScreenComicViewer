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
		private BitmapImage _currentCachedImage = null;
        private BitmapImage _nextImage = null;
        private BitmapImage _previousImage = null;
		private int _currentPageIndex = 0;
		private static string[] VALID_IMAGE_FILE_EXT = { ".jpg", ".png" };
		public int CurrentPageNumber {
			get { return _currentPageIndex + 1;}
			protected set { 
				_currentPageIndex = value;
				RaisePropertyChanged("CurrentPageNumber");
			}
		}

		public int TotalPages { get { return _filesInComicBook.Count; } }

		public string ComicBookTitle { get { return _comicBookFileName;} }

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
				if(_filesInComicBook.Count < 1) {
					return null;
				}
				_coverImage = GetImageFromComicFile(_filesInComicBook[0]);
			}
			_currentCachedImage = _coverImage;
			return _coverImage;
		}

        private void SetupPageCache()
        {
            _nextImage = GetImageFromComicFile(_filesInComicBook[GetNextPageIndex()]);
            _previousImage = GetImageFromComicFile(_filesInComicBook[GetPreviousPageIndex()]);
        }

		//*****************************************
		public BitmapImage GetNextPageImage() {

            BitmapImage pageImage = _nextImage;
            int nextPageIndex = GetNextPageIndex();
            if (pageImage == null)
            {
                pageImage = GetImageFromComicFile(_filesInComicBook[nextPageIndex]);                
            }

            CurrentPageNumber = nextPageIndex;
			_currentCachedImage = pageImage;

            SetupPageCache();

			return pageImage;

		}

        private int GetNextPageIndex()
        {
            int nextPageIndex = _currentPageIndex + 1;
            if (nextPageIndex >= TotalPages)
            {
                nextPageIndex = 0;
            }

            return nextPageIndex;
        }
		//*****************************************
		public BitmapImage GetCurrentPageImage() {
			return _currentCachedImage;

		}

		//*****************************************
		public BitmapImage GetPreviousPageImage() {

            int prevPageIndex = GetPreviousPageIndex();
            BitmapImage pageImage = _previousImage;
             
            if (_previousImage == null)
            {
                GetImageFromComicFile(_filesInComicBook[prevPageIndex]);
			}

            CurrentPageNumber = prevPageIndex;
			_currentCachedImage = pageImage;

            SetupPageCache();

			return pageImage;

		}

        private int GetPreviousPageIndex()
        {
            int prevPageIndex = _currentPageIndex - 1;
            if (prevPageIndex < 0)
            {
                prevPageIndex = TotalPages - 1;
            }

            return prevPageIndex;
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
