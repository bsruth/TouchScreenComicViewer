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
using System.Windows.Threading;
using System.ComponentModel;

namespace TouchScreenComicViewer {
	public partial class LoadingComicsProgress : UserControl {

		IEnumerator<FileInfo> _filesToLoad; //enumerator to list of files we're loading
		int _totalFileCount; //total number of files being loaded
		int _currentLoadingFileNumber; //the number of the file currently being loaded

		public RoutedEventHandler FileLoadingCompleted; //fired when file loading is completed

		public LoadingComicsProgress() {
			InitializeComponent();
			this.FileNameLabel.DataContext = this;
		}

		public void LoadFiles(IEnumerator<FileInfo> fileEnumerator, ComicArchiveManager comicArchiveMgr) {
			//get num files
			_totalFileCount = 0;
			while (fileEnumerator.MoveNext()) {
				_totalFileCount++;
			}
			fileEnumerator.Reset();
			this.progressBar1.Maximum = _totalFileCount;

			BackgroundWorker comicLoader = new BackgroundWorker();
			comicLoader.WorkerReportsProgress = true;
			comicLoader.ProgressChanged += new ProgressChangedEventHandler(comicLoader_ProgressChanged);
			comicLoader.RunWorkerCompleted += (completedSender, completedEvArgs) =>
				{
					System.Threading.Thread.Sleep(1000);
					var handler = FileLoadingCompleted;
					if (handler != null) {
						handler(this, new RoutedEventArgs());
					}
					this.Visibility = System.Windows.Visibility.Collapsed;
				};
			comicLoader.DoWork += (workSender, workE) =>
			{
				BackgroundWorker bw = workSender as BackgroundWorker;
				try {
					_currentLoadingFileNumber = 0;
					while (fileEnumerator.MoveNext()) {
						bw.ReportProgress(_currentLoadingFileNumber, fileEnumerator.Current.Name);
						comicArchiveMgr.AddComicToArchive(fileEnumerator.Current);
						_currentLoadingFileNumber++;

					}
				} catch (Exception ex) {
					string exceptionString = ex.ToString();
				}
			};

			comicLoader.RunWorkerAsync();

		}

		void comicLoader_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			this.FileNameLabel.Content = e.UserState as string;
			this.LoadingCount.Content = (_currentLoadingFileNumber + 1) + "/" + _totalFileCount;
			this.progressBar1.Value = _currentLoadingFileNumber;
		}
	}
}
