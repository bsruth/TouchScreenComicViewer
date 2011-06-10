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

namespace TouchScreenComicViewer {
	public class ComicArchiveManager {
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";

		public ComicArchiveManager() {
		}

		public List<string> GetAvailableComics() {
			return IsoStorageUtilities.GetIsolatedStorageFilesWithExtension(COMIC_ARCHIVE_ZIP_EXT);
		}

	}

}
