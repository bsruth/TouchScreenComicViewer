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

namespace TouchScreenComicViewer {
	public class ComicBook {
		//members
		private string _comicBookFileName;
		private Stream _comicBookFileStream;
		private string[] _filesInComicBook;
		private int _currentPageIndex = 0;

		public int CurrentPageNumber { get { return _currentPageIndex + 1; } }
		public int TotalPages { get { return _filesInComicBook.Length; } }

		//*****************************************
		public ComicBook(string comicBookFileName) {
			_comicBookFileName = comicBookFileName;
		}

		//*****************************************
		public string GetComicFileName() 
		{
			return _comicBookFileName;
		}

		//*****************************************
		public BitmapImage GetCoverImage()
		{
			if (IsFileStreamOpen() == false){
				if (OpenFileStream() == false) {
					return null;
				}
			}
			BitmapImage coverImg = GetImageFromComicFile(_filesInComicBook[0]);
			return coverImg;
		}

		//*****************************************
		public BitmapImage GetNextPageImage() {
			int nextPageIndex = _currentPageIndex + 1;
			if (nextPageIndex >= TotalPages) {
				nextPageIndex = 0;
			}

			BitmapImage pageImage = GetImageFromComicFile(_filesInComicBook[nextPageIndex]);
			if (pageImage != null) {
				_currentPageIndex = nextPageIndex;
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
				_currentPageIndex = prevPageIndex;
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
				_filesInComicBook = ZipFileUtilities.GetZipContents(_comicBookFileStream);
				if (_filesInComicBook.Length <= 0) {
					_comicBookFileStream.Close();
					return false;
				}
			}

			//TODO: we need to close the stream for now since the zip file utilities require
			//the stream to close all the time
			_comicBookFileStream.Close();
			return true;
		}


	}
}
