namespace GoPost.Models
{
    public class PostFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        // Foreign key
        public int PostId { get; set; }
        public Post Post { get; set; }
    }

}
