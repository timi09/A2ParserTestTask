using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace A2ParserTestTask
{
    class WoodDealsApi
    {
        private HttpClient httpClient;

        private const string url = "https://www.lesegais.ru/open-area/graphql";

        private readonly string GetDealsPageQuery;

        private readonly string GetDealsInfoQuery;

        public WoodDealsApi()
        {
            httpClient = new HttpClient();
            SetHeaders();
            GetDealsPageQuery = File.ReadAllText(Directory.GetCurrentDirectory() + "/GetDealsPageQuery.json");
            GetDealsInfoQuery = File.ReadAllText(Directory.GetCurrentDirectory() + "/GetDealsInfoQuery.json");
        }

        public async Task<List<Deal>> GetDealsPage(int pageSize, int pageNum) 
        {
            string graphqlQuery = GetDealsPageQuery
                .Replace("\"DealPageSize\"", pageSize.ToString())
                .Replace("\"DealPageNumber\"", pageNum.ToString());
            
            string json = await GetJsonResponse(graphqlQuery);

            return DeserializeJson<DealsPage>(json).content;
        }

        public async Task<DealsInfo> GetDealsInfo() 
        {
            string graphqlQuery = GetDealsInfoQuery;

            string json = await GetJsonResponse(graphqlQuery);

            return DeserializeJson<DealsInfo>(json);
        }

        private async Task<string> GetJsonResponse(string graphqlQuery) 
        {
            StringContent postData = new StringContent(graphqlQuery, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(url, postData);

            byte[] result = await response.Content.ReadAsByteArrayAsync();

            byte[] decompressedResult = Decompress(result);

            string jsonResult = Encoding.UTF8.GetString(decompressedResult);

            return jsonResult;
        } 

        private T DeserializeJson<T>(string json) where T : class
        {
            var obj = JsonSerializer.Deserialize<ResponseObj<T>>(json);
            return obj.data.searchReportWoodDeal;
        }

        private void SetHeaders() 
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("Host", "www.lesegais.ru");
            httpClient.DefaultRequestHeaders.Add("Origin", "https://www.lesegais.ru");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://www.lesegais.ru/open-area/deal");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        }

        private byte[] Decompress(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        private class ResponseObj<T> where T : class
        {
            public DataObj<T> data { get; set; }
        }

        private class DataObj<T>
        {
            public T searchReportWoodDeal { get; set; }
        }

        private class DealsPage 
        {
            public List<Deal> content { get; set; }
        }
    }

    public class Deal
    {
        public string buyerInn { get; set; }
        public string buyerName { get; set; }
        public string dealDate { get; set; }
        public string dealNumber { get; set; }
        public string sellerInn { get; set; }
        public string sellerName { get; set; }
        public double woodVolumeBuyer { get; set; }
        public double woodVolumeSeller { get; set; }
    }

    public class DealsInfo
    {
        public double overallBuyerVolume { get; set; }
        public double overallSellerVolume { get; set; }
        public int total { get; set; }
    }
}
