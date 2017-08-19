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
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

namespace TouchScreenComicViewer {

    public class ComicCoverViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private BitmapImage _coverImage = null;
        public ComicCoverViewModel(ComicBook comic)
        {
            Comic = comic;
            var disp = Deployment.Current.Dispatcher;
            DispatcherSynchronizationContext myDispSync = new DispatcherSynchronizationContext(disp); //needed to dispatch synchronously                  
            myDispSync.Send((obj) =>
            {
                CoverImage = new BitmapImage();
            }, null);
            using (MemoryStream imageStream = new MemoryStream(comic.CoverImage))
            {
                CoverImage.SetSource(imageStream);
            }
        }

        public ComicBook Comic { get; set; }

        public BitmapImage CoverImage
        {
            get
            {
                if (_coverImage == null)
                {
                    using (MemoryStream imageStream = new MemoryStream(Comic.CoverImage))
                    {
                        _coverImage.SetSource(imageStream);
                    }
                }
                return _coverImage;
            }

            private set
            {
                if (value != null && _coverImage != value)
                {
                    _coverImage = value;
                    OnPropertyChanged("CoverImage");
                }
            }
        }

        public string ComicBookTitle { get { return Comic.ComicBookTitle; } }
    }
    public class ComicArchiveManager : INotifyPropertyChanged
    {
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		const string COMIC_ARCHIVE_META_FILE = "comic_archive_meta.txt";
		const string LAST_COMIC_META_STRING = "lastcomic";
		private string _lastComicOpened = "";
        private static string[] VALID_IMAGE_FILE_EXT = { ".jpg", ".png" };


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Properties
        public ObservableCollection<ComicCoverViewModel> ComicArchiveList
        {
            get;
            set;
        }


        public string LastComicOpened
        {
            get
            {
                return _lastComicOpened;
            }
            private set
            {
                if (_lastComicOpened != value)
                {
                    _lastComicOpened = value;
                    OnPropertyChanged("LastComicOpened");
                }
            }
        }

        #endregion
        public ComicArchiveManager() {

            ComicArchiveList = new ObservableCollection<ComicCoverViewModel>();
            
            IsoStorageUtilities.FileRemoved += (fileRemoved, ev) =>
			{
				RemoveComicFromArchive(fileRemoved as string);
			};


			if (IsoStorageUtilities.DoesFileExist(COMIC_ARCHIVE_META_FILE) == false) {
				IsoStorageUtilities.CreateIsolatedStorageFile(COMIC_ARCHIVE_META_FILE);
			}

            LastComicOpened = GetLastOpenedComic();

			List<string> comicBookFileNames = IsoStorageUtilities.GetIsolatedStorageFilesWithExtension(COMIC_ARCHIVE_ZIP_EXT);
			foreach (string fileName in comicBookFileNames) {
				ComicBook newComic = new ComicBook(fileName, GetFilesInComicBook(fileName), GetImageFromComicFile);
                ComicArchiveList.Add(new ComicCoverViewModel(newComic));
			}

		}

        /// <summary>
        /// Gets a list of all supported comic image files in the comic archive.
        /// </summary>
        /// <returns></returns>
        private List<string> GetFilesInComicBook(string comicBookFileName)
        {
            var filesInComicBook = new List<string>();
            var comicBookFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(comicBookFileName);
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

        public MemoryStream GetImageFromComicFile(string comicFileName, string imageName)
        {
            MemoryStream imageFileStream = null;
            Stream coverFileStream = ZipFileUtilities.GetFileStreamFromZIPFile(comicFileName, imageName);
            if (coverFileStream != null)
            {
                imageFileStream = new MemoryStream();
                coverFileStream.CopyTo(imageFileStream);
            }
            return imageFileStream;
        }

        //*****************************************
        public List<string> GetAvailableComics() {
			List<string> comicBookList = new List<string>();
            foreach (ComicCoverViewModel comic in ComicArchiveList)
            {
				comicBookList.Add(comic.Comic.GetComicFileName());
			}
			comicBookList.Sort();
			return comicBookList;
		}

        public ComicCoverViewModel GetComic(string comicFileName)
        {
            ComicCoverViewModel requestedComic = null;
            foreach (ComicCoverViewModel item in ComicArchiveList)
            {
                if (item.Comic.GetComicFileName() == comicFileName)
                {
                    requestedComic = item;
                    break;
                }

            }

            return requestedComic;
        }

		//*****************************************
		public bool AddComicToArchive(FileInfo comicFile) {

			if (IsoStorageUtilities.CopyFileToIsoStorage(comicFile) == false) {
				//TODO: error message
				return false;
			}
			ComicBook comicToAdd = new ComicBook(comicFile.Name, GetFilesInComicBook(comicFile.Name), GetImageFromComicFile);
			if(comicToAdd.TotalPages < 1) {
				IsoStorageUtilities.DeleteFileFromIsoStorage(comicFile.Name);
				return false;
			}
            var disp = Deployment.Current.Dispatcher;
            DispatcherSynchronizationContext myDispSync = new DispatcherSynchronizationContext(disp); //needed to dispatch synchronously                  
            myDispSync.Send((obj) =>
            {
                ComicArchiveList.Add(new ComicCoverViewModel(comicToAdd));
            }, null);
            

			return true;
		}

		//*****************************************
		public bool RemoveComicFromArchive(string comicFileName) {
            ComicCoverViewModel foundBook = GetComic(comicFileName);
           
            if (foundBook != null)
            {
                var disp = Deployment.Current.Dispatcher;
                DispatcherSynchronizationContext myDispSync = new DispatcherSynchronizationContext(disp); //needed to dispatch synchronously                  
                myDispSync.Send((obj) =>
                {
                    ComicArchiveList.Remove(foundBook);
                }, null);
                
            }
			return true;
		}

		//*****************************************
		public ComicBook OpenComic(string comicFileName) {
            ComicCoverViewModel requestedComic = GetComic(comicFileName);
			if (requestedComic != null) {
				SetLastOpenedComic(comicFileName);
			}
			return requestedComic.Comic;
		}

		//*****************************************
		public void SetLastOpenedComic(string comicFileName) 
		{
			LastComicOpened = comicFileName;
			Stream metaFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(COMIC_ARCHIVE_META_FILE);
			if (metaFileStream != null) {
				StreamReader sr = new StreamReader(metaFileStream);
				string metaFileContents = sr.ReadToEnd();
				sr.Close();
				if (metaFileContents != "") {
					Regex rgx = new Regex( LAST_COMIC_META_STRING + ".+\\" + COMIC_ARCHIVE_ZIP_EXT);
					metaFileContents = rgx.Replace(metaFileContents, LAST_COMIC_META_STRING + " " + comicFileName);
				} else {
					metaFileContents = LAST_COMIC_META_STRING + " " + comicFileName;
				}
				metaFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(COMIC_ARCHIVE_META_FILE, FileMode.Truncate);
				if (metaFileStream != null) {
					StreamWriter sw = new StreamWriter(metaFileStream);
					sw.Write(metaFileContents);
					sw.Close();
				}
				metaFileStream.Close();
			}
		}

		//*****************************************
		private string GetLastOpenedComic() {
            string lastComicOpened = "";
				Stream metaFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(COMIC_ARCHIVE_META_FILE);
				if (metaFileStream != null) {
					StreamReader sr = new StreamReader(metaFileStream);
					string metaLine = "";
					while ((metaLine = sr.ReadLine()) != null) {
						if (metaLine.Contains(LAST_COMIC_META_STRING)) {
							string metaValue = metaLine.Substring(LAST_COMIC_META_STRING.Length);
							metaValue.Trim();
                            lastComicOpened = metaValue;
							break;
						}
					}
					sr.Close();
					metaFileStream.Close();
				}

			return lastComicOpened;
		}


	}

}
