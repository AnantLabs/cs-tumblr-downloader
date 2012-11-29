using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Threading;

namespace TumblrDownloader
{
	public enum Modes
	{
		DownloadAll,
		Update
	}

	public class Program
	{
		private static int threshold;
		private static List<string> tumblrAccounts;
		private static List<string> downloadedImages;
		private static Modes mode;

		static Program()
		{
			threshold = 3;
			LoadSettings();
			LoadDownloadedImages();
		}

		private static void DownloadImages(string username)
		{
			if (!Directory.Exists("./Images")) Directory.CreateDirectory("./Images");
			if (!Directory.Exists("./Images/" + username + "/")) Directory.CreateDirectory("./Images/" + username + "/");

			XmlDocument doc = new XmlDocument();
			XmlNodeList list = null;
			int start = 0;
			int currentImage = 0;
			int foundCount = 0;

			while (true)
			{
				try
				{
					doc.Load("http://" + username + ".tumblr.com/api/read?start=" + start + "&num=50");
					list = doc.GetElementsByTagName("photo-url");
					if (list.Count == 0)
					{
						break;
					}
					foreach (XmlNode node in list)
					{
						try
						{
							Console.Title = "[" + username + "]: Current image: " + currentImage + " (Page: " + (start / 50) + ")";
							if (node.Attributes["max-width"].Value.Equals("1280"))
							{
								string fileName = node.InnerText;
								fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
								fileName = fileName.Replace("tumblr_", (username + "----"));

								if (Path.GetExtension(fileName) == string.Empty) fileName += ".jpg";

								/*
								 * I could just check if the file exists, but it's better saving the file name so that
								 * if there's an image you don't want, you can delete it and the program wont automatically
								 * re-download it.
								 */
								if (!Downloaded(fileName))
								{
									Console.WriteLine("Image found: {0} downloading...", fileName);
									using (var client = new WebClient())
									{
										byte[] buffer = client.DownloadData(node.InnerText);
										Console.WriteLine("Image downloaded, saving file... (size: {0} bytes)", buffer.Length);
										using (var bw = new BinaryWriter(File.Open("./Images/" + username + "/" + fileName, FileMode.Create)))
										{
											bw.Write(buffer, 0, buffer.Length);
										}
										AddDownloadedImage(fileName);
										Console.WriteLine("Image saved\n---------");
									}
								}
								else
								{
									if (mode == Modes.Update)
									{
										foundCount++;
										if (foundCount >= threshold) return;
									}
									else
									{
										Console.WriteLine("{0} already exists, skipping...", fileName);
									}
								}
								currentImage++;
							}
							continue;
						}
						catch (Exception ex)
						{
							Console.WriteLine("Error: " + ex.Message);
							Console.WriteLine("query: " + node.InnerText);
						}
					}
					//the start needs to be skipped by 50 every time
					start += 50;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error: " + ex.Message);
					break;
				}
			}
		}

		private static bool Downloaded(string fileName)
		{
			foreach (string str in downloadedImages) if (str.Equals(fileName)) return true;
			return false;
		}

		private static void AddDownloadedImage(string fileName)
		{
			downloadedImages.Add(fileName);
			//append file name
			using (var sw = new StreamWriter("./DownloadedImages", true))
			{
				sw.WriteLine(fileName);
			}
		}

		private static void LoadDownloadedImages()
		{
			downloadedImages = new List<string>();
			if (!File.Exists("./DownloadedImages")) return;
			using (var sr = new StreamReader("./DownloadedImages"))
			{
				string line = "";
				while ((line = sr.ReadLine()) != null) downloadedImages.Add(line);
			}
		}

		private static void LoadSettings()
		{
			XmlDocument settings = new XmlDocument();
			settings.Load("./Settings.xml");

			tumblrAccounts = new List<string>();
			foreach (XmlNode node in settings.GetElementsByTagName("tumblrAccount"))
			{
				tumblrAccounts.Add(node.InnerText);
			}
		}

		private static void ProcessTumblrAccounts(Modes mode)
		{
			if (tumblrAccounts.Count == 0)
			{
				Console.WriteLine("Error - you have not specified any Tumblr accounts.");
				return;
			}

			Program.mode = mode;
			switch (mode)
			{
				case Modes.Update:
					while (true)
					{
						Console.Clear();
						foreach (string account in tumblrAccounts)
						{
							Console.WriteLine("Starting update process for {0}", account);
							DownloadImages(account);
							Console.WriteLine("Update complete, moving on...\n---------");
						}
						Console.WriteLine("All accounts are now up-to-date. Checking again in 30 minutes...");
						Console.Title = "Next update in 30 minutes...";
						//wait for 30 minutes before checking again.
						Thread.Sleep((60 * 1000) * 30);
					}
				case Modes.DownloadAll:
					foreach (string account in tumblrAccounts)
					{
						Console.WriteLine("Starting image download for {0}", account);
						DownloadImages(account);
						Console.WriteLine("No more images found, moving on...\n---------");
					}
					break;
			}
		}

		public static void Main(string[] args)
		{
			Console.Title = "Tumblr High-Res Image Downloader - TehJayden";
		Start:
			{
				Console.Clear();
				Console.WriteLine("<- Options ->\n");
				Console.WriteLine("1 - (Download All) All images will be downloaded from the Tumblr accounts you specified in the settings file until there are no images left to download. If images that have already been downloaded are encountered, they will be skipped, regardless of the image still existing on your hard drive (This was done so that you can delete images you don't want, without them being re-downloaded automatically). \n");
				Console.WriteLine("2 - (Update) All images will be downloaded from the Tumblr accounts specified in the settings file until files that have already been downloaded are reached. If left running, the program will automatically check the blogs every 30 minutes.\n");
				Console.Write("Selection: ");
				string selection = Console.ReadLine();

				if (selection.Equals("1")) ProcessTumblrAccounts(Modes.DownloadAll);
				else if (selection.Equals("2")) ProcessTumblrAccounts(Modes.Update);
				else goto Start;
			}
			Console.Read();
		}
	}
}
