namespace X.Lucifer
{
    public class XAlbumModel
    {
        public int status_code { get; set; }
        public Item_List[] item_list { get; set; }
    }

    public class Item_List
    {
        public long group_id { get; set; }
        public string aweme_id { get; set; }
        public int create_time { get; set; }
        public object comment_list { get; set; }
        public Video video { get; set; }
        public Image[] images { get; set; }
        public int duration { get; set; }
        public object video_text { get; set; }
        public object promotions { get; set; }
        public object[] text_extra { get; set; }
        public long author_user_id { get; set; }
        public int is_preview { get; set; }
        public Author author { get; set; }
        public int aweme_type { get; set; }
        public object long_video { get; set; }
        public string forward_id { get; set; }
        public string desc { get; set; }
        public string share_url { get; set; }
        public Statistics statistics { get; set; }
        public object video_labels { get; set; }
        public object geofencing { get; set; }
        public object label_top_text { get; set; }
        public bool is_live_replay { get; set; }
    }


    public class Image
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
        public string[] download_url_list { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}