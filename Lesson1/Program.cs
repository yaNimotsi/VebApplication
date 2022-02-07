using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lesson1
{
    internal static class Program
    {
        private static readonly CancellationTokenSource Cts = new();
        private static readonly string PathToTxt = Path.Combine(Environment.CurrentDirectory, "result.txt");

        private const string PostAddressWithOutIndex = "https://jsonplaceholder.typicode.com/posts/";

        private static async Task Main()
        {
            HttpClient client = new();
            
            const byte startPostIndex = 4;
            const byte endPostIndex = 13;

            var responseList = new List<string>();

            Cts.CancelAfter(10000);

            for (int i = startPostIndex; i <= endPostIndex; i++)
            {
                var newUriPost = new Uri(Path.Combine(PostAddressWithOutIndex, i.ToString()));
                responseList.Add(GetAndSendUriAsync(newUriPost, client).Result);
            }

            await ProcessingResponse(responseList);

            Console.WriteLine("Program has finished working");
        }

        /// <summary>
        /// Get response after request
        /// </summary>
        /// <param name="postAddressUri"> request address</param>
        /// <param name="client">HTML client object</param>
        /// <returns>post text </returns>
        private static async Task<string> GetAndSendUriAsync(Uri postAddressUri, HttpClient client)
        {
            try
            {
                var responseQuery = client.GetAsync(postAddressUri, HttpCompletionOption.ResponseContentRead, Cts.Token).Result;
                responseQuery.EnsureSuccessStatusCode();
                return await responseQuery.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Processing response list to finally view
        /// </summary>
        /// <param name="responsesList"></param>
        /// <returns></returns>
        private static async Task ProcessingResponse(List<string> responsesList)
        {
            if (responsesList.Count <= 0) return;

            StringBuilder stringBuilder = new();

            foreach (var t in responsesList)
            {
                stringBuilder.Append(t + "\r\r");
            }

            await WriteToFileAsync(stringBuilder.ToString());
        }

        /// <summary>
        /// Write text to txt file
        /// </summary>
        /// <param name="textToWrite"> Text to write in txt file</param>
        /// <returns></returns>
        private static async Task WriteToFileAsync(string textToWrite)
        {
            if (!File.Exists(PathToTxt))
            {
                await CreateTxtAsync();
            }

            var encodingTextByte = Encoding.UTF8.GetBytes(textToWrite);

            if (encodingTextByte.Length <= 0)
            {
                return;
            }

            await using var fileStream = new FileStream(PathToTxt, FileMode.Append, FileAccess.Write,
                FileShare.None, 4096, true);
            try
            {
                await fileStream.WriteAsync(encodingTextByte, Cts.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Create txt file
        /// </summary>
        private static async Task CreateTxtAsync()
        {
            try
            {
                File.Create(PathToTxt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
