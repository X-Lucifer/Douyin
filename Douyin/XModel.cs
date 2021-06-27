namespace X.Lucifer
{
    public class XModel
    {
        public Extra extra { get; set; }
        public int status_code { get; set; }
        public Aweme_List[] aweme_list { get; set; }
        public long max_cursor { get; set; }
        public long min_cursor { get; set; }
        public bool has_more { get; set; }
    }

    public class VideoInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string NickName { get; set; }

        public string Url { get; set; }
    }

    public class Extra
    {
        public long now { get; set; }
        public string logid { get; set; }
    }

    public class Aweme_List
    {
        public object comment_list { get; set; }
        public object video_text { get; set; }
        public object image_infos { get; set; }
        public object promotions { get; set; }
        public string desc { get; set; }
        public Video video { get; set; }
        public Statistics statistics { get; set; }
        public Text_Extra[] text_extra { get; set; }
        public object label_top_text { get; set; }
        public object long_video { get; set; }
        public object images { get; set; }
        public object cha_list { get; set; }
        public object video_labels { get; set; }
        public int aweme_type { get; set; }
        public object geofencing { get; set; }
        public string aweme_id { get; set; }
        public Author author { get; set; }
    }

    public class Video
    {
        public Cover cover { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public Origin_Cover origin_cover { get; set; }
        public string ratio { get; set; }
        public Download_Addr download_addr { get; set; }
        public object bit_rate { get; set; }
        public int duration { get; set; }
        public Play_Addr play_addr { get; set; }
        public Dynamic_Cover dynamic_cover { get; set; }
        public bool has_watermark { get; set; }
        public Play_Addr_Lowbr play_addr_lowbr { get; set; }
        public string vid { get; set; }
    }

    public class Cover
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Origin_Cover
    {
        public string[] url_list { get; set; }
        public string uri { get; set; }
    }

    public class Download_Addr
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Play_Addr
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Dynamic_Cover
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Play_Addr_Lowbr
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Statistics
    {
        public string aweme_id { get; set; }
        public int comment_count { get; set; }
        public int digg_count { get; set; }
        public int play_count { get; set; }
        public int share_count { get; set; }
        public int forward_count { get; set; }
    }

    public class Author
    {
        public string nickname { get; set; }
        public Video_Icon video_icon { get; set; }
        public object policy_version { get; set; }
        public string uid { get; set; }
        public Avatar_Medium avatar_medium { get; set; }
        public int follow_status { get; set; }
        public int favoriting_count { get; set; }
        public string total_favorited { get; set; }
        public bool has_orders { get; set; }
        public string signature { get; set; }
        public int secret { get; set; }
        public bool user_canceled { get; set; }
        public bool is_ad_fake { get; set; }
        public int aweme_count { get; set; }
        public bool with_commerce_entry { get; set; }
        public object platform_sync_info { get; set; }
        public bool with_shop_entry { get; set; }
        public string short_id { get; set; }
        public string region { get; set; }
        public bool with_fusion_shop_entry { get; set; }
        public int rate { get; set; }
        public object[] type_label { get; set; }
        public string custom_verify { get; set; }
        public int follower_count { get; set; }
        public bool story_open { get; set; }
        public int verification_type { get; set; }
        public object followers_detail { get; set; }
        public int following_count { get; set; }
        public object geofencing { get; set; }
        public Avatar_Thumb avatar_thumb { get; set; }
        public string unique_id { get; set; }
        public string enterprise_verify_reason { get; set; }
        public bool is_gov_media_vip { get; set; }
        public string sec_uid { get; set; }
        public Avatar_Larger avatar_larger { get; set; }
    }

    public class Video_Icon
    {
        public string uri { get; set; }
        public object[] url_list { get; set; }
    }

    public class Avatar_Medium
    {
        public string[] url_list { get; set; }
        public string uri { get; set; }
    }

    public class Avatar_Thumb
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Avatar_Larger
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

    public class Text_Extra
    {
        public int start { get; set; }
        public int end { get; set; }
        public int type { get; set; }
        public string hashtag_name { get; set; }
        public long hashtag_id { get; set; }
    }
}