﻿using System;
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

namespace TouchScreenComicViewer {
	public class ComicArchiveManager {
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		const string COMIC_ARCHIVE_META_FILE = "comic_archive_meta.txt";
		const string LAST_COMIC_META_STRING = "lastcomic";
		private string mLastComicOpened = "";

        public ObservableCollection<ComicBook> ComicArchiveList
        {
            get;
            set;
        }

		public ComicArchiveManager() {

            ComicArchiveList = new ObservableCollection<ComicBook>();
            
            IsoStorageUtilities.FileRemoved += (fileRemoved, ev) =>
			{
				RemoveComicFromArchive(fileRemoved as string);
			};


			if (IsoStorageUtilities.DoesFileExist(COMIC_ARCHIVE_META_FILE) == false) {
				IsoStorageUtilities.CreateIsolatedStorageFile(COMIC_ARCHIVE_META_FILE);
			}

			List<string> comicBookFileNames = IsoStorageUtilities.GetIsolatedStorageFilesWithExtension(COMIC_ARCHIVE_ZIP_EXT);
			foreach (string fileName in comicBookFileNames) {
				ComicBook newComic = new ComicBook(fileName);
                ComicArchiveList.Add(newComic);
			}

		}

		//*****************************************
		public List<string> GetAvailableComics() {
			List<string> comicBookList = new List<string>();
            foreach (ComicBook comic in ComicArchiveList)
            {
				comicBookList.Add(comic.GetComicFileName());
			}
			comicBookList.Sort();
			return comicBookList;
		}

        public ComicBook GetComic(string comicFileName)
        {
            ComicBook requestedComic = null;
            foreach (var item in ComicArchiveList)
            {
                if (item.GetComicFileName() == comicFileName)
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
			ComicBook comicToAdd = new ComicBook(comicFile.Name);
			if(comicToAdd.TotalPages < 1) {
				IsoStorageUtilities.DeleteFileFromIsoStorage(comicFile.Name);
				return false;
			}
            var disp = Deployment.Current.Dispatcher;
            DispatcherSynchronizationContext myDispSync = new DispatcherSynchronizationContext(disp); //needed to dispatch synchronously                  
            myDispSync.Send((obj) =>
            {
                ComicArchiveList.Add(comicToAdd);
            }, null);
            

			return true;
		}

		//*****************************************
		public bool RemoveComicFromArchive(string comicFileName) {
            ComicBook foundBook = GetComic(comicFileName);
           
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
		public BitmapImage GetComicCover(string comicFileName) {
			ComicBook requestedComic = GetComic(comicFileName);
            if(requestedComic != null)
            {
				return requestedComic.CoverImage;
			}
			return null;
		}

		//*****************************************
		public ComicBook OpenComic(string comicFileName) {
			ComicBook requestedComic = GetComic(comicFileName);
			if (requestedComic != null) {
				SetLastOpenedComic(comicFileName);
			}
			return requestedComic;
		}

		//*****************************************
		public void SetLastOpenedComic(string comicFileName) 
		{
			mLastComicOpened = comicFileName;
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
		public string GetLastOpenedComic() {
			if (mLastComicOpened == "") {
				Stream metaFileStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(COMIC_ARCHIVE_META_FILE);
				if (metaFileStream != null) {
					StreamReader sr = new StreamReader(metaFileStream);
					string metaLine = "";
					while ((metaLine = sr.ReadLine()) != null) {
						if (metaLine.Contains(LAST_COMIC_META_STRING)) {
							string metaValue = metaLine.Substring(LAST_COMIC_META_STRING.Length);
							metaValue.Trim();
							this.mLastComicOpened = metaValue;
							break;
						}
					}
					sr.Close();
					metaFileStream.Close();
				}
			}

			return mLastComicOpened;
		}


	}

}
