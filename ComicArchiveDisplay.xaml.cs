using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;

namespace TouchScreenComicViewer {

	public partial class ComicArchiveDisplay : UserControl {
		const string COMIC_ARCHIVE_ZIP_EXT = ".cbz";
		MainPage comicDisplay;
		public ComicArchiveDisplay() {
			InitializeComponent();
		}

		private void ComicArchiveDisplayPage_Loaded(object sender, RoutedEventArgs e) {
			List<string> comics = IsoStorageUtilities.GetIsolatedStorageFilesWithExtension(COMIC_ARCHIVE_ZIP_EXT);
			ComicArchiveListBox.ItemsSource = comics;
			comicDisplay = new MainPage(this);
		}

		private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			string selectedItem = ComicArchiveListBox.SelectedItem.ToString();
			comicDisplay.SetComic(selectedItem);
			((UserControlContainer)Application.Current.RootVisual).SwitchControl(comicDisplay);
		}

		private childItem FindVisualChild<childItem>(DependencyObject obj)
		where childItem : DependencyObject {
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is childItem)
					return (childItem)child;
				else {
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

	}
}