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
			BitmapImage coverImage = new BitmapImage();
			Stream coverFileStream = ZipFileUtilities.GetFileStreamFromZIPFile(_comicBookFileName, _filesInComicBook[0]);
			if (coverFileStream != null) {
				coverImage.SetSource(coverFileStream);
				coverFileStream.Close();
			}

			return coverImage;
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
