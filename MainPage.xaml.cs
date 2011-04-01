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
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;
using System.Windows.Interop;

namespace TouchScreenComicViewer {
	public partial class MainPage : UserControl {
		private const double _originalWidth = 970;
		private const double _originalHeight = 1470;
		private const double _originalAspectRatio =
				_originalWidth / _originalHeight;
		private List<string> fileList = new List<string>();
		private int currentIndex = 0;
		public MainPage() {

			InitializeComponent();

			SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);
			this.MainDisplayImage.MouseLeftButtonUp += new MouseButtonEventHandler(MainDisplayImage_MouseLeftButtonUp);

			
		}

		void MainDisplayImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			currentIndex++;
			if (currentIndex >= this.fileList.Count) {
				currentIndex = 0;
			}
			string data = String.Empty;
			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication()) {
				IsolatedStorageFileStream file = isf.OpenFile(this.fileList[currentIndex], FileMode.Open);
				System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
				bmp.SetSource(file);
				this.MainDisplayImage.Source = bmp;
				this.MainDisplayImage.Visibility = System.Windows.Visibility.Visible;
				file.Close();
			}
		}

		void MainPage_SizeChanged(object sender, SizeChangedEventArgs e) {
			if (e.NewSize.Width < _originalWidth ||
					e.NewSize.Height < _originalHeight) {
				// don't shrink
				PageScale.ScaleX = 1.0;
				PageScale.ScaleY = 1.0;
			} else {
				// resize keeping aspect ratio the same
				if (e.NewSize.Width / e.NewSize.Height > _originalAspectRatio) {
					// height is our constraining property
					PageScale.ScaleY = e.NewSize.Height / _originalHeight;
					PageScale.ScaleX = PageScale.ScaleY;
				} else {
					// either width is our constraining property, or the user
					// managed to nail our aspect ratio perfectly.
					PageScale.ScaleX = e.NewSize.Width / _originalWidth;
					PageScale.ScaleY = PageScale.ScaleX;
				}
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();

			IsolatedStorageFile iso2 = IsolatedStorageFile.GetUserStoreForApplication();
			if (iso2.Quota < 2048576) {
				if (!iso2.IncreaseQuotaTo(iso2.Quota * 15)) {
					throw new Exception("Can't store the image.");
				} else {
					return;
				}
			}

			this.button1.Visibility = System.Windows.Visibility.Collapsed;
			dlg.Multiselect = true;
			if (dlg.ShowDialog() == true) {
				// Save all selected files into application's isolated storage
				IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
				foreach (FileInfo file in dlg.Files) {
						try {
							this.fileList.Add(file.Name);
						} catch (Exception ex) {
							ex.ToString();
						}

						//Images files are quite large, we may need to request more space.
						Int64 spaceToAdd = file.Length;
						Int64 curAvail = iso.AvailableFreeSpace;
						if (curAvail < spaceToAdd) {
							if (!iso.IncreaseQuotaTo(iso.Quota + spaceToAdd)) {
								throw new Exception("Can't store the image.");
							}
						}
					using (Stream fileStream = file.OpenRead()) {
						using (IsolatedStorageFileStream isoStream =
								new IsolatedStorageFileStream(file.Name, FileMode.Create, iso)) {

							// Read and write the data block by block until finish
							while (true) {
								byte[] buffer = new byte[100001];
								int count = fileStream.Read(buffer, 0, buffer.Length);
								if (count > 0) {
									isoStream.Write(buffer, 0, count);
								} else {
									break;
								}
							}
						}
					}
				}

				string data = String.Empty;
				using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication()) {
					IsolatedStorageFileStream file = isf.OpenFile(this.fileList[currentIndex], FileMode.Open);
					System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
					bmp.SetSource(file);
					this.MainDisplayImage.Source = bmp;
					this.MainDisplayImage.Visibility = System.Windows.Visibility.Visible;

				}

			}
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
			if (Application.Current.Host.Content.IsFullScreen == true) {
				this.button2.Visibility = System.Windows.Visibility.Visible;
			} else {
				this.button2.Visibility = System.Windows.Visibility.Collapsed;
			}

		}
	}
}