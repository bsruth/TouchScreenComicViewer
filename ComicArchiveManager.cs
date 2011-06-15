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

namespace TouchScreenComicViewer {
	public class ComicArchiveManager {
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		const string COMIC_ARCHIVE_META_FILE = "comic_archive_meta.txt";
		const string LAST_COMIC_META_STRING = "lastcomic";
		private string mLastComicOpened = "";

		public ComicArchiveManager() {
			if (IsoStorageUtilities.DoesFileExist(COMIC_ARCHIVE_META_FILE) == false) {
				IsoStorageUtilities.CreateIsolatedStorageFile(COMIC_ARCHIVE_META_FILE);
			}

			

		}

		//*****************************************
		public List<string> GetAvailableComics() {
			return IsoStorageUtilities.GetIsolatedStorageFilesWithExtension(COMIC_ARCHIVE_ZIP_EXT);
		}

		//*****************************************
		public BitmapImage GetComicCover(string comicFileName) {
			BitmapImage coverImage = new BitmapImage();

			Stream comicStream = IsoStorageUtilities.OpenIsolatedStorageFileStream(comicFileName);
			string[] comicFiles = ZipFileUtilities.GetZipContents(comicStream);
			comicStream.Close();
			if (comicFiles.GetLength(0) > 0) {
				Stream coverFileStream = ZipFileUtilities.GetFileStreamFromZIPFile(comicFileName, comicFiles[0]);
				if (coverFileStream != null) {
					coverImage.SetSource(coverFileStream);
					coverFileStream.Close();
				}
			}
			return coverImage;
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