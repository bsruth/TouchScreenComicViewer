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
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;

namespace TouchScreenComicViewer {
	public class IsoStorageUtilities {

		//*****************************************
		//returns the number of bytes removed from the store
		static public long RemoveOldestFileFromIsoStore(IsolatedStorageFile isoStore) {

			string[] fileListing = isoStore.GetFileNames();
			if (fileListing.Length == 0) {
				//nothing to delete
				return 0;
			}

			string oldestFileName = fileListing[0];
			DateTimeOffset oldestAccessDate = isoStore.GetLastAccessTime(fileListing[0]);
			foreach (string file in fileListing) {
				DateTimeOffset accessDate = isoStore.GetLastAccessTime(file);
				if (accessDate < oldestAccessDate) {
					oldestAccessDate = accessDate;
					oldestFileName = file;
				}
			}

			IsolatedStorageFileStream oldestFile = isoStore.OpenFile(oldestFileName, FileMode.Open);
			long numBytesRemoved = oldestFile.Length;
			oldestFile.Close();
			isoStore.DeleteFile(oldestFileName);
			return numBytesRemoved;
		}

		//*****************************************
		static public bool CopyFileToIsoStorage(FileInfo fileToCopy) {
			// Save all selected files into application's isolated storage
			using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication()) {


				//don't copy a file if it is already in the cache
				if (DoesFileExist(fileToCopy.Name) == false) {
					Int64 spaceToAdd = fileToCopy.Length;

					//check if it is even possible to store this file
					//without a quota increase
					if (iso.Quota < spaceToAdd) {
						if (iso.IncreaseQuotaTo(iso.Quota + (spaceToAdd - iso.AvailableFreeSpace)) == false) {
							//no way we can fit the file no matter what we do
							return false;
						}
					}

					//the new file isn't big enough to fit, we need to increase
					//the quota
					if (iso.AvailableFreeSpace < spaceToAdd) {
						//try and increase without removing any files from cache
						if (iso.IncreaseQuotaTo(iso.Quota + (spaceToAdd - iso.AvailableFreeSpace)) == false) {
							//there is enough space, but we need to remove some files
							//evidently we couldn't allocate any more space
							if (iso.AvailableFreeSpace < spaceToAdd) {
								//see if we can free up space by removing some files.
								long numBytesRemoved = 0;
								do {
									numBytesRemoved = RemoveOldestFileFromIsoStore(iso);
								} while ((numBytesRemoved > 0) && (iso.AvailableFreeSpace < spaceToAdd));

							}
						}
					}

					//sanity check
					if (iso.AvailableFreeSpace < spaceToAdd) {
						//we should have been able to clear things out,
						//this should never be hit.
						return false;
					}

					//finally, copy file
					using (Stream fileStream = fileToCopy.OpenRead()) {
						using (IsolatedStorageFileStream isoStream =
										new IsolatedStorageFileStream(fileToCopy.Name, FileMode.Create, iso)) {

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
			}
			return true;
		}

		//*****************************************
		static public bool DoesFileExist(string fileName)
		{
			using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication() ) {
				return iso.FileExists(fileName);
			}
		}

		//*****************************************
		static public bool CreateIsolatedStorageFile(string fileName) 
		{
			try {
				using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication()) {
					using (IsolatedStorageFileStream isoStream =
						new IsolatedStorageFileStream(fileName, FileMode.Create, iso)) {
					}
				}
			} catch (Exception e) {
			}

			return DoesFileExist(fileName);
		}

		//*****************************************
		static public FileStream OpenIsolatedStorageFileStream(string fileName) 
		{
			try {
				using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication()) {
					IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(fileName, FileMode.Open, iso);
					return fileStream;
				}
			} catch (Exception e) {
				return null;
			}
		}

		//*****************************************
		static public List<string> GetIsolatedStorageFilesWithExtension(string fileExtension) 
		{
			List<string> filesWithExt = new List<string>();
			try {
				using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication()) {
					if(!fileExtension.StartsWith(".")) {
						fileExtension = "." + fileExtension;
					}
					filesWithExt.AddRange(iso.GetFileNames("*" + fileExtension));
				}
			} catch (Exception e) {
				return new List<string>(); //empty list
			}
			return filesWithExt;
		}


	}
}
