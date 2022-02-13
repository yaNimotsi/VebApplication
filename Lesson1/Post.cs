namespace Lesson1
{
    internal class Post
    {
        public string userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }

        public override string ToString()
        {
            var newRow = "\r";
            var result = userId + newRow + id.ToString() + newRow + title + newRow + body 
                + newRow + newRow;
            return result;
        }
    }
}
