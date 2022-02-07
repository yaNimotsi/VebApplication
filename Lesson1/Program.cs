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

        const byte startPostIndex = 4;
        const byte endPostIndex = 13;

        private static async Task Main()
        {
            Cts.CancelAfter(10000);

            var responsesList = await GetResponsesAsync();

            await ProcessingResponseAsync(responsesList);

            Console.WriteLine("Program has finished working");
        }

        /// <summary>
        /// Get all respons from the post
        /// </summary>
        /// <returns>Responses from the post</returns>
        private static async Task<List<string>> GetResponsesAsync()
        {
            HttpClient client = new();

            var result = new List<string>();

            for (int i = startPostIndex; i <= endPostIndex; i++)
            {
                var newUriPost = new Uri(Path.Combine(PostAddressWithOutIndex, i.ToString()));
                var response = await GetAndSendUriAsync(newUriPost, client);
                result.Add(response);
            }
            
            return result;
        }

        /// <summary>
        /// Get respons after request
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
        private static async Task ProcessingResponseAsync(List<string> responsesList)
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
                CreateTxtAsync();
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
        private static void CreateTxtAsync()
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
