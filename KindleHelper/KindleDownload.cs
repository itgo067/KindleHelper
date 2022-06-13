using System;
using KindleHelper.Models;
using KindleHelper.Utils;

namespace KindleHelper
{
	public class KindleDownload
	{
		private string csrfToken;
		private string domain;
		private string outDir;
		private int cutLength;
        private KindleUrl kindleUrl;
        private int totalToDownload;
        private bool notDone;
        private int batchSize;

        public KindleDownload(string csrfToken, string domain = "cn", string outDir ="", int cutLength = 100, int batchSize = 100)
        {
            this.csrfToken = csrfToken;
            this.domain = domain;
            this.outDir = outDir;
            this.cutLength = cutLength;
            this.kindleUrl = KindleUrl.GetKindleUrl(domain);
            this.notDone = false;
            this.batchSize = batchSize;
        }

        public KindleUrl KindleUrl { get { return kindleUrl; } private set { kindleUrl = value; } }

        public string CsrfToken { get { return csrfToken; } }

        public int CutLength {  get { return cutLength; } }

        public int BatchSize { get { return batchSize; } set { batchSize = value; } }
        public int TotalDownload { get { return totalToDownload; } }
        public bool NotDone { get { return notDone; } set { notDone = value; } }

        public string OutDir { get { return outDir; } }


        public async Task DownloadBooks(int startIndex, string fileType= "EBOK")
        {
            if (string.IsNullOrWhiteSpace(csrfToken))
            {
                this.csrfToken = await HttpUtil.GetCsrfToken(kindleUrl);
                Console.WriteLine($"csrfToken: {csrfToken}");
            }

            var deviceResp = await HttpUtil.GetDevices(this);
            if (deviceResp == null)
            {
                Console.WriteLine("未能获取到 kindle 设备，请检查cookie");
                return;
            }

            var device = deviceResp.GetDevices.Devices[0];
            var resp = await HttpUtil.GetBookList(this, startIndex, fileType);
            if (resp == null) {
                return;
            }

            totalToDownload = resp.OwnershipData.NumberOfItems;
            var items = resp.OwnershipData.Items;

            if (resp.OwnershipData.HasMoreItems) {
                while(true)
                {
                    startIndex += batchSize;
                    resp = await HttpUtil.GetBookList(this, startIndex, fileType);
                    if (resp != null)
                    {
                        items.AddRange(resp.OwnershipData.Items);
                        if (!resp.OwnershipData.HasMoreItems)
                        {
                            break;
                        }
                    } else
                    {
                        break;
                    }
                }
            }

            var index = startIndex;

            foreach (var item in items)
            {
                await HttpUtil.DownloadFile(this, item, device, index, fileType);
                index++;
            }

        }

    }
}

