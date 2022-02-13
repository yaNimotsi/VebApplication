using Newtonsoft.Json;

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

            HttpClient client = new();

            var taskList = new List<Task<Post>>();

            for (int i = startPostIndex; i <= endPostIndex; i++)
            {
                var newUriPost = new Uri(Path.Combine(PostAddressWithOutIndex, i.ToString()));
                taskList.Add(GetAndSendUriAsync(newUriPost, client));
            }

            var posts = await Task.WhenAll(taskList);

            ProcessingResponse(new List<Post>(posts));

            Console.WriteLine("Program has finished working");
        }

        /// <summary>
        /// Get respons after request
        /// </summary>
        /// <param name="postAddressUri"> request address</param>
        /// <param name="client">HTML client object</param>
        /// <returns>Post object </returns>
        private static async Task<Post> GetAndSendUriAsync(Uri postAddressUri, HttpClient client)
        {
            try
            {
                var response = await client.GetAsync(postAddressUri,
                    HttpCompletionOption.ResponseContentRead, Cts.Token);

                var postFromUri = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Post>(postFromUri);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"We have exception with uir {postAddressUri} " +
                    $"\r\n {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Processing response list to finally view
        /// </summary>
        /// <param name="responsesList"></param>
        /// <returns></returns>
        private static void ProcessingResponse(List<Post> responsesList)
        {
            if (responsesList.Count <= 0) return;

            StringBuilder stringBuilder = new();

            foreach (var t in responsesList)
            {
                stringBuilder.Append(t.ToString());
            }

            WriteToFile(stringBuilder.ToString());
        }

        /// <summary>
        /// Write text to txt file
        /// </summary>
        /// <param name="textToWrite"> Text to write in txt file</param>
        /// <returns></returns>
        private static void WriteToFile(string textToWrite)
        {
            if (!File.Exists(PathToTxt))
            {
                CreateTxt();
            }

            var encodingTextByte = Encoding.UTF8.GetBytes(textToWrite);

            if (encodingTextByte.Length <= 0)
            {
                return;
            }

            using var fileStream = new FileStream(PathToTxt, FileMode.Append, FileAccess.Write,
                FileShare.None, 4096, true);
            try
            {
                fileStream.Write(encodingTextByte);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Create txt file
        /// </summary>
        private static void CreateTxt()
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
