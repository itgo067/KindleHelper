using System;
using System.Text;
using System.Text.RegularExpressions;
using KindleHelper.Models;
using Newtonsoft.Json;

namespace KindleHelper.Utils
{
	public class HttpUtil
	{
		public HttpUtil()
		{
			
		}

		private static HttpClient client = new HttpClient();


		public static HttpClient NewClient()
		{ 
			if (client.DefaultRequestHeaders.Contains("User-Agent"))
            {
				client.DefaultRequestHeaders.Remove("User-Agent");
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.61 Safari/537.36");
            }
            else
            {
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.61 Safari/537.36");
			}
			if (client.DefaultRequestHeaders.Contains("Cookie"))
            {
				client.DefaultRequestHeaders.Remove("Cookie");
				client.DefaultRequestHeaders.Add("Cookie", KindleConst.COOKIE);
            }
            else
            {
				client.DefaultRequestHeaders.Add("Cookie", KindleConst.COOKIE);
			}
			return client;
        }

        public static async Task<string> GetCsrfToken(KindleUrl kindleUrl)
        {
			string url = kindleUrl.BookAll; // "https://www.amazon.cn/hz/mycd/myx#/home/content/booksAll/dateDsc/";
			var client = NewClient();
            try
            {
				var resp = await client.GetAsync(url);
				if (resp.StatusCode == System.Net.HttpStatusCode.OK)
				{
					var content = await resp.Content.ReadAsStringAsync();
					var re = new Regex("var csrfToken = \"(?<token>.+)\"");
					var match = re.Match(content);
					if (match.Success)
					{
						var result = match.Groups["token"].Value;
						Console.WriteLine($"crsfToken: {result}");
						return result;
					}
				}
			}
            catch (Exception ex)
            {
				Console.WriteLine(ex.Message);
            }
			
			Console.WriteLine("获取csrfToken异常,请检查 cookie 是否正确");
			return String.Empty;
        }

		public static async Task<DeviceResp?> GetDevices(KindleDownload kindleDownload)
        {
			var url = kindleDownload.KindleUrl.Payload; // "https://www.amazon.cn/hz/mycd/ajax";

			var payload = string.Format("data={0}&csrfToken={1}", "{\"param\":{\"GetDevices\":{}}}", kindleDownload.CsrfToken); //"data={\"param\":{\"GetDevices\":{}}}&csrfToken=" + "";


			var client = NewClient();
			client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
			//client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
			var httpContent = new StringContent(payload);
			httpContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
			var resp = await client.PostAsync(url, httpContent);
			if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
				var content = await resp.Content.ReadAsStringAsync();
				DeviceResp deviceResp = JsonConvert.DeserializeObject<DeviceResp>(content);
				if (deviceResp ==  null || deviceResp.GetDevices == null || deviceResp.GetDevices.Devices == null || deviceResp.GetDevices.Devices.Count == 0)
                {
					Console.WriteLine(content);
					return null;
                }
				return deviceResp;
            }
			Console.WriteLine("device null");
			return null;
        }

		public static async Task<BookListResp?> GetBookList(KindleDownload kindleDownload, int startIndex, string fileType)
        {
			var url = kindleDownload.KindleUrl.Payload;

			string paramStr = string.Empty;

			if (fileType == "EBOK") {
				var dataParam = new OwnershipDataParam()
				{
					param = new
					{
					 OwnershipData = new OwnershipDataParam.OwnershipDataEBok()
						{
							SortOrder = "DESCENDING",
							SortIndex = "DATE",
							StartIndex = startIndex,
							BatchSize = kindleDownload.BatchSize,
							ContentType = CONTENT_TYPES.FromString(fileType).Name,
							ItemStatus = new List<string>() { "Active" },
							OriginType = new List<string>() { "Purchase" }

						}
					}
                };
				paramStr = JsonConvert.SerializeObject(dataParam);
			}
            else
            {
				kindleDownload.BatchSize = 18;
				var dataParam = new OwnershipDataParam()
				{
					param = new
					{
						OwnershipData = new OwnershipDataParam.OwnershipDataPDoc()
						{
							SortOrder = "DESCENDING",
							SortIndex = "DATE",
							StartIndex = startIndex,
							BatchSize = kindleDownload.BatchSize,
							ContentType = CONTENT_TYPES.FromString(fileType).Name,
							ItemStatus = new List<string>() { "Active" },
							IsExtendedMYK = false

						}
					}
				};
				paramStr = JsonConvert.SerializeObject(dataParam);
			}

			Console.WriteLine($"paramStr: {paramStr}");

			var payload = string.Format("data={0}&csrfToken={1}", paramStr, kindleDownload.CsrfToken);
			var client = NewClient();
			client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
			//client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
			var httpContent = new StringContent(payload);
			httpContent.Headers.ContentType.MediaType ="application/x-www-form-urlencoded";
			var resp = await client.PostAsync(url, httpContent);
			if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
				var content = await resp.Content.ReadAsStringAsync();
				BookListResp bookListResp = JsonConvert.DeserializeObject<BookListResp>(content);
				if (bookListResp == null)
				{
					return null;
				}
				return bookListResp;
			}
            else if(resp.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
				if (startIndex == 0)
                {
					Console.WriteLine("Amazon api limit when this download done.\n Please run it again");
                }
                else
                {
					kindleDownload.NotDone = false;
					Console.WriteLine($"Amazon api limit when this download done.\n You can add command `resume-from {startIndex}`");
                }
            }
			Console.WriteLine("get book list error");
			return null;
		}

		public static async Task DownloadFile(KindleDownload kindleDownload, OwnershipDataItem item, DeviceResp.Device device, int index, string fileType)
        {
			var url = string.Format(kindleDownload.KindleUrl.Download,fileType, item.Asin, device.DeviceSerialNumber, device.DeviceType, device.CustomerId);
			var client = NewClient();
			var resp = await client.GetAsync(url);
			if (resp.StatusCode == System.Net.HttpStatusCode.Redirect)
            {
				Console.WriteLine($"content: {await resp.Content.ReadAsStringAsync()}");

				return;
            }

			var contentDisposition = resp.Content.Headers.ContentDisposition;
			Console.WriteLine(contentDisposition);

			var fileName = contentDisposition.FileNameStar ;
			Console.WriteLine($"size:{contentDisposition.Size}");
			Console.WriteLine($"fileName- filenamestar: {contentDisposition.FileName} - {contentDisposition.FileNameStar}");
			Console.WriteLine($"开始下载「{fileName}」");
			fileName = Regex.Replace(fileName, "[\\/:*?\" <>|]", "_");
			if (fileName.Length > kindleDownload.CutLength)
            {
				fileName = fileName.Substring(0, kindleDownload.CutLength - 5) + fileName.Substring(fileName.Length - 5);
            }
			var totalSize = resp.Content.Headers.ContentLength;
			string filePath = kindleDownload.OutDir +  Path.DirectorySeparatorChar + fileName;
			if (File.Exists(filePath))
            {
				return;
            }
			Console.WriteLine($"({index + 1}/{kindleDownload.TotalDownload}) downloading {fileName} {totalSize} bytes");

			using (var st = await resp.Content.ReadAsStreamAsync())
            {
				using(var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
					await st.CopyToAsync(fs);
                }
            }
        }

	}
}

