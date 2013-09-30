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
        private BitmapImage _coverImage = new BitmapImage();
        private BitmapImage _currentPageImage = new BitmapImage();
		private int _currentPageIndex = 0;
		private static string[] VALID_IMAGE_FILE_EXT = { ".jpg", ".png" };

        private List<MemoryStream> _cachedComicImages = new List<MemoryStream>();


        #region Properties
        
        public int CurrentPageNumber {
			get { return _currentPageIndex + 1;}
			protected set { 
				_currentPageIndex = value;
				RaisePropertyChanged("CurrentPageNumber");
			}
		}


        public BitmapImage CurrentPageImage
        {
            get
            {
                return _currentPageImage;
            }

            private set
            {
                if(value != null && _currentPageImage != value)
                {
                    _currentPageImage = value;
                    RaisePropertyChanged("CurrentPageImage");
                }
            }
        }

        public BitmapImage CoverImage
        {
            get
            {
                return _coverImage;
            }

            private set
            {
                if(value != null && _coverImage != value)
                {
                    _coverImage = value;
                    RaisePropertyChanged("CoverImage");
                }
            }
        }

		public int TotalPages { get { return _filesInComicBook.Count; } }

		public string ComicBookTitle { get { return _comicBookFileName;} }


        #endregion

        //*****************************************
		public ComicBook(string comicBookFileName) {
			_comicBookFileName = comicBookFileName;
            _filesInComicBook = GetFilesInComicBook();

            if (_filesInComicBook.Count > 1)
            {
                using (var coverStream = GetImageFromComicFile(_filesInComicBook[0]))
                {
                    CoverImage.SetSource(coverStream);
                    CurrentPageImage.SetSource(coverStream);
                }
            }
            else
            {
               //TODO: Show image saying "NO IMAGES"
            }
		}


		//*****************************************
		public string GetComicFileName() 
		{
			return _comicBookFileName;
		}

		//*****************************************
		public void GoToNextPage() {

            int nextPageIndex = GetNextPageIndex();
          
            CurrentPageNumber = nextPageIndex;
            CurrentPageImage.SetSource(_cachedComicImages[nextPageIndex]);
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
        public void GoToPreviousPage()
        {

            int prevPageIndex = GetPreviousPageIndex();
            CurrentPageNumber = prevPageIndex;
            CurrentPageImage.SetSource(_cachedComicImages[prevPageIndex]);
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
		private MemoryStream GetImageFromComicFile(string imageName)
		{
			MemoryStream imageFileStream = null;
			Stream coverFileStream = ZipFileUtilities.GetFileStreamFromZIPFile(_comicBookFileName, imageName);
			if (coverFileStream != null) {
                imageFileStream = new MemoryStream();
                coverFileStream.CopyTo(imageFileStream);      
			}
            return imageFileStream;
		}

        private void CacheImagesInComic()
        {
            new System.Threading.Thread(() =>
            {
                if (_cachedComicImages.Count == 0)
                {
                    foreach (var comicFile in _filesInComicBook)
                    {
                        var comicStream = GetImageFromComicFile(comicFile);
                        _cachedComicImages.Add(comicStream);
                    }
                }
            }).Start();
        }

        public void OpenComic()
        {
            CacheImagesInComic();
        }

        public void CloseComic()
        {
            foreach (var comicStream in _cachedComicImages)
            {
                comicStream.Close();
            }
            _cachedComicImages.Clear();
        }

        /// <summary>
        /// Gets a list of all supported comic image files in the comic archive.
        /// </summary>
        /// <returns></returns>
        private List<string> GetFilesInComicBook()
        {
            var filesInComicBook = new List<string>();
            var comicBookFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(_comicBookFileName);
            if (comicBookFileStream == null)
            {
                return filesInComicBook;
            }

            string[] filesInArchive = ZipFileUtilities.GetZipContents(comicBookFileStream);
            filesInComicBook.Clear();
            //remove all non-image files from the list of files
            foreach (string fileName in filesInArchive)
            {
                foreach (string ext in VALID_IMAGE_FILE_EXT)
                {
                    if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        filesInComicBook.Add(fileName);
                        break;
                    }
                }
            }

            comicBookFileStream.Close();

            return filesInComicBook;

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
