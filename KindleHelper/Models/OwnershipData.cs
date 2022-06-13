using System;
using Newtonsoft.Json;

namespace KindleHelper.Models
{
	public class OwnershipData
	{
		[JsonProperty("hasMoreItems")]
		public bool HasMoreItems { get; set; }
		[JsonProperty("numberOfItems")]
		public int NumberOfItems { get; set; }
		[JsonProperty("success")]
		public bool Success { get; set; }
		[JsonProperty("items")]
		public List<OwnershipDataItem> Items { get; set; }

	}

	public class OwnershipDataItem
    {
		[JsonProperty("asin")]
		public string Asin { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
    }

	public class BookListResp
    {
		[JsonProperty("OwnershipData")]
		public OwnershipData OwnershipData { get; set; }
    }
}

