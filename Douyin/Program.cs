using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Flurl.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using TG.INI;

namespace X.Lucifer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var xsavepath = "";
            try
            {
                Console.WriteLine($"date:{DateTime.Now:G} | start...");
                var file = AppContext.BaseDirectory + "config.ini";
                if (!File.Exists(file))
                {
                    var xfile = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    var doc = new IniDocument();
                    doc.Sections.Add(new IniSection("douyin")
                    {
                        new IniComment("抖音用户主页地址"),
                        new IniKeyValue("url", "https://v.douyin.com/e4Q17W3/"),
                        new IniWhiteSpace(),
                        new IniComment("视频保存路径, 不填默认当前目录"),
                        new IniKeyValue("savepath", AppContext.BaseDirectory + @"Download\"),
                        new IniWhiteSpace(),
                        new IniComment("是否只解析第一页, 默认为false"),
                        new IniKeyValue("onlyfirst", "false")
                    });
                    doc.Write(xfile);
                    Console.WriteLine($"date:{DateTime.Now:G} | config file not exists, created finished...");
                }

                var xdoc = new IniDocument(file);
                var section = xdoc.Sections.Find("douyin");
                if (section == null)
                {
                    File.Delete(file);
                    Console.WriteLine($"date:{DateTime.Now:G} | config file error, is about to be automatically generated , please run the software again...");
                    return;
                }

                var url = section["url"]?.Value ?? "";
                var savepath = section["savepath"]?.Value ?? "";
                var onlyfirst = section["onlyfirst"]?.ValueBoolean ?? false;
                if (string.IsNullOrEmpty(url))
                {
                    Console.WriteLine($"date:{DateTime.Now:G} | douyin url error...");
                    return;
                }

                if (string.IsNullOrEmpty(savepath))
                {
                    savepath = AppContext.BaseDirectory + @"Download\";
                    if (!Directory.Exists(savepath))
                    {
                        Directory.CreateDirectory(savepath);
                    }
                }
                xsavepath = savepath;
                var result = await url
                    .WithHeader("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36 Edg/89.0.774.48")
                    .WithHeader("Accept", "*/*")
                    .WithHeader("Accept-Encoding", "gzip, deflate, br").WithAutoRedirect(false).GetAsync();
                if (result == null)
                {
                    Console.WriteLine($"date:{DateTime.Now:G} | douyin analysis error...");
                    return;
                }

                var location = result.Headers.FirstOrDefault(HeaderNames.Location) ?? "";
                if (string.IsNullOrEmpty(location))
                {
                    Console.WriteLine("douyin location error...");
                    return;
                }
                var uri = new Uri(result.Headers.FirstOrDefault(HeaderNames.Location));
                var uid = HttpUtility.ParseQueryString(uri.Query).Get("sec_uid") ?? "";
                var baseurl = $"https://www.iesdouyin.com/web/api/v2/aweme/post/?count=99&sec_uid={uid}";
                var isnext = !onlyfirst;
                long cursor = 0;
                var xdownlist = new HashSet<VideoInfo>();
                var retry = 0;
                do
                {
                    var sign = Guid.NewGuid().ToString("N");
                    var zurl = $"{baseurl}&max_cursor={cursor}&_signature={sign}&aid=1128";
                    var xresult = await zurl.GetStringAsync();
                    if (xresult == null)
                    {
                        isnext = false;
                    }
                    else
                    {
                        var zresult = JsonConvert.DeserializeObject<XModel>(xresult);
                        if (zresult?.aweme_list == null || zresult.aweme_list.Length <= 0)
                        {
                            if (retry > 2)
                            {
                                isnext = false;
                            }

                            cursor = zresult?.max_cursor ?? 0;
                            retry++;
                            Console.WriteLine($"date:{DateTime.Now:G} | douyin retry: {retry}...");
                        }
                        else
                        {
                            cursor = zresult.max_cursor;
                            foreach (var item in zresult.aweme_list)
                            {
                                var nickname = item.author?.nickname ?? "";
                                if (!string.IsNullOrEmpty(nickname))
                                {
                                    nickname = Regex.Replace(nickname, @"\W+", "");
                                }
                                var name = item.desc ?? item.aweme_id ?? "";
                                name = Regex.Replace(name, @"\W+", "");
                                var urls = item.video?.play_addr?.url_list;
                                if (string.IsNullOrEmpty(name) || urls == null || urls.Length <= 0)
                                {
                                    continue;
                                }
                                var xurl = urls.FirstOrDefault() ?? "";
                                if (string.IsNullOrEmpty(xurl))
                                {
                                    continue;
                                }

                                xdownlist.Add(new VideoInfo
                                {
                                    Id = item.aweme_id,
                                    Name = name,
                                    NickName = nickname,
                                    Url = xurl
                                });
                                Console.WriteLine($"date:{DateTime.Now:G} | analysis: {name}...");
                            }
                        }
                    }
                } while (isnext);
                Console.WriteLine($"date:{DateTime.Now:G} | douyin analysis finished...");
                Console.WriteLine($"date:{DateTime.Now:G} | start download...");
                var tasks = xdownlist.Select(item => Task.Run(async () =>
                {
                    var zpath = savepath;
                    if (!string.IsNullOrEmpty(item.NickName))
                    {
                        zpath = zpath + item.NickName + @"\";
                        if (!Directory.Exists(zpath))
                        {
                            Directory.CreateDirectory(zpath);
                        }
                    }

                    var xfile = zpath + item.Name + ".mp4";
                    if (!File.Exists(xfile))
                    {
                        var xresult = await item.Url.WithTimeout(20).OnError(x =>
                        {
                            Console.WriteLine($"date:{DateTime.Now:G} | download: {xfile}, error...");
                        }).DownloadFileAsync(zpath, item.Name + ".mp4", 2048);
                        Console.WriteLine($"date:{DateTime.Now:G} | download: {xresult}");
                    }
                })).ToList();
                await Task.WhenAll(tasks);
                Console.WriteLine($"date:{DateTime.Now:G} | download finished, will automatically open the download directory...");
                Process.Start(xsavepath);
            }
            catch (Exception)
            {
                Console.WriteLine($"date:{DateTime.Now:G} | error: download error, please try again... ");
                if (!string.IsNullOrEmpty(xsavepath))
                {
                    if (Directory.Exists(xsavepath))
                    {
                        Process.Start(xsavepath);
                    }
                }
            }
        }
    }
}
