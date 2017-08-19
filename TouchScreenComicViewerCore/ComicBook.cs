using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouchScreenComicViewerCore{
	public class ComicBook : INotifyPropertyChanged {
		//members
		private string _comicBookFileName;
		private List<string> _filesInComicBook = new List<string>();
        private byte[] _coverImageBuffer = null;
        private byte[] _currentImageBuffer = null;
		private int _currentPageIndex = 0;
        private Func<string, string, MemoryStream> _decompressFunction;


        private List<byte[]> _cachedComicImages = new List<byte[]>();


        #region Properties
        
        public int CurrentPageNumber {
			get { return _currentPageIndex + 1;}
			protected set { 
				_currentPageIndex = value;
				RaisePropertyChanged("CurrentPageNumber");
			}
		}


        public byte[] CurrentImageBuffer
        {
            get
            {
                return _currentImageBuffer;
            }

            private set
            {
                if(value != null && _currentImageBuffer != value)
                {
                    _currentImageBuffer = value;
                    RaisePropertyChanged("CurrentImageBuffer");
                }
            }
        }

        public byte[] CoverImage
        {
            get
            {
                if(_coverImageBuffer == null)
                {
                    LoadCover();
                }
                return _coverImageBuffer;
            }

            private set
            {
                if (value != null && _coverImageBuffer != value)
                {
                    _coverImageBuffer = value;
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
                    _currentImageBuffer = _coverImageBuffer;
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
            CurrentImageBuffer = _cachedComicImages[pageNumber];
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
                Task.Factory.StartNew(() =>
                {
                    foreach (var comicFile in _filesInComicBook)
                    {
                        using (var comicStream = _decompressFunction(_comicBookFileName, comicFile))
                        {
                            _cachedComicImages.Add(comicStream.ToArray());
                        }
                    }
                });
            }
        }

        public void OpenComic()
        {
            CacheImagesInComic();
        }

        public void CloseComic()
        {
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
