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
using System.Windows.Threading;

namespace TouchScreenComicViewer{
	public class ComicBook : INotifyPropertyChanged {
		//members
		private string _comicBookFileName;
		private Stream _comicBookFileStream;
		private List<string> _filesInComicBook = new List<string>();
        private BitmapImage _coverImage = null;
        private byte[] _coverImageBuffer = null;
        private BitmapImage _currentPageImage = null;
		private int _currentPageIndex = 0;
        private Func<string, string, MemoryStream> _decompressFunction;


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
                if (_coverImage == null)
                {
                    LoadCover();
                }
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
		public ComicBook(string comicBookFileName, List<string> filesInComicBook, Func<string, string, MemoryStream> decompressFunction) {
			_comicBookFileName = comicBookFileName;
            _filesInComicBook = filesInComicBook;
            _decompressFunction = decompressFunction;
            LoadCover();
		}

        private void LoadCover()
        {
            if (_filesInComicBook.Count > 1)
            {
                using (var coverStream = _decompressFunction(_comicBookFileName, _filesInComicBook[0]))
                {
                    _coverImageBuffer = coverStream.ToArray();
                }
               
                //Dispatcher myDisp = Application.Current.RootVisual.Dispatcher;
                var disp = Deployment.Current.Dispatcher;
                DispatcherSynchronizationContext myDispSync = new DispatcherSynchronizationContext(disp); //needed to dispatch synchronously                  
                myDispSync.Send((obj) =>
                {

                    using (MemoryStream coverStream = new MemoryStream(_coverImageBuffer))
                    {
                        CoverImage = new BitmapImage();
                        CurrentPageImage = new BitmapImage();
                        CoverImage.SetSource(coverStream);
                        CurrentPageImage.SetSource(coverStream);
                    }
                }, null);
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
            GoToPage(nextPageIndex);
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

        private void GoToPage(int pageNumber)
        {
            CurrentPageNumber = pageNumber;
            var imageBuffer = _cachedComicImages[pageNumber].ToArray();
            using (MemoryStream imageStream = new MemoryStream(imageBuffer))
            {
                CurrentPageImage.SetSource(imageStream);
            }
        }
		//*****************************************
        public void GoToPreviousPage()
        {

            int prevPageIndex = GetPreviousPageIndex();
            GoToPage(prevPageIndex);
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

        private void CacheImagesInComic()
        {
            if (_cachedComicImages.Count == 0)
            {
                new System.Threading.Thread(() =>
                {

                    foreach (var comicFile in _filesInComicBook)
                    {
                        var comicStream = _decompressFunction(_comicBookFileName, comicFile);// GetImageFromComicFile(comicFile);
                        _cachedComicImages.Add(comicStream);
                    }

                }).Start();
            }
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
