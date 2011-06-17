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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.IsolatedStorage;
using System.Windows.Resources;


namespace TouchScreenComicViewer {
	public class ZipFileUtilities {

		/// <summary>
		/// Reads the file names from the header of the zip file
		/// </summary>
		/// <param name="zipStream">The stream to the zip file</param>
		/// <returns>An array of file names stored within the zip file. These file names may also include relative paths.</returns>
		public static string[] GetZipContents(System.IO.Stream zipStream) {

			List<string> names = new List<string>();
			BinaryReader reader = new BinaryReader(zipStream);
			while (reader.ReadUInt32() == 0x04034b50) {

				// Skip the portions of the header we don't care about
				reader.BaseStream.Seek(14, SeekOrigin.Current);
				uint compressedSize = reader.ReadUInt32();
				uint uncompressedSize = reader.ReadUInt32();
				int nameLength = reader.ReadUInt16();
				int extraLength = reader.ReadUInt16();
				byte[] nameBytes = reader.ReadBytes(nameLength);
				names.Add(Encoding.UTF8.GetString(nameBytes, 0, nameLength));
				reader.BaseStream.Seek(extraLength + compressedSize, SeekOrigin.Current);

			}
			// Move the stream back to the beginning
			zipStream.Seek(0, SeekOrigin.Begin);
			return names.ToArray();
		}

		public static Stream GetFileStreamFromZIPFile(string zipFileName, string requestedFileName)
		{
			Stream requestedFileStream = null;
			using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication()) {
				using (IsolatedStorageFileStream file = isf.OpenFile(zipFileName, FileMode.Open)) {
					StreamResourceInfo zipInfo = new StreamResourceInfo(file, null);
					StreamResourceInfo streamInfo = Application.GetResourceStream(zipInfo, new Uri(requestedFileName, UriKind.Relative));
					requestedFileStream = streamInfo.Stream;
				}
			}
			return requestedFileStream;

		}

	}
}
