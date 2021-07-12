using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Flurl.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NLog;
using TG.INI;

namespace X.Lucifer
{
    class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            var xsavepath = "";
            try
            {
                _log.Info("start...");
                var file = AppContext.BaseDirectory + "config.ini";
                if (!File.Exists(file))
                {
                    var xfile = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    var doc = new IniDocument();
                    doc.Sections.Add(new IniSection("douyin")
                    {
                        new IniComment("抖音用户主页地址"),
                        new IniKeyValue("url", "https://v.douyin.com/e4vSGrG/"),
                        new IniWhiteSpace(),
                        new IniComment("视频保存路径, 不填默认当前目录"),
                        new IniKeyValue("savepath", AppContext.BaseDirectory + @"Download\"),
                        new IniWhiteSpace(),
                        new IniComment("是否只解析第一页, 默认为false"),
                        new IniKeyValue("onlyfirst", "false")
                    });
                    doc.Write(xfile);
                    _log.Info("config file not exists, created finished...");
                }

                var xdoc = new IniDocument(file);
                var section = xdoc.Sections.Find("douyin");
                if (section == null)
                {
                    File.Delete(file);
                    _log.Error(
                        "config file error, is about to be automatically generated , please run the software again...");
                    return;
                }

                var url = section["url"]?.Value.GetUrl() ?? "";
                //抖音地址验证
                if (!url.Contains("www.iesdouyin.com") && !url.Contains("v.douyin.com"))
                {
                    File.Delete(file);
                    _log.Error(
                        "config file error, is about to be automatically generated , please run the software again...");
                    return;
                }

                var savepath = section["savepath"]?.Value ?? "";
                var onlyfirst = section["onlyfirst"]?.ValueBoolean ?? false;
                if (string.IsNullOrEmpty(url))
                {
                    _log.Error("douyin url error...");
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
                Uri uri;
                //添加完整主页地址支持
                if (url.Contains("v.douyin.com"))
                {
                    var result = await url
                        .WithHeader("User-Agent",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36 Edg/89.0.774.48")
                        .WithHeader("Accept", "*/*")
                        .WithHeader("Accept-Encoding", "gzip, deflate, br").WithAutoRedirect(false).GetAsync();
                    if (result == null)
                    {
                        _log.Error("douyin analysis error...");
                        return;
                    }
                    var location = result.Headers.FirstOrDefault(HeaderNames.Location) ?? "";
                    if (string.IsNullOrEmpty(location))
                    {
                        _log.Error("douyin location error...");
                        return;
                    }
                    uri = new Uri(result.Headers.FirstOrDefault(HeaderNames.Location));
                }
                else
                {
                    uri = new Uri(url);
                }
                var uid = HttpUtility.ParseQueryString(uri.Query).Get("sec_uid") ?? "";
                if (string.IsNullOrEmpty(uid))
                {
                    File.Delete(file);
                    _log.Error(
                        "config file error, is about to be automatically generated , please run the software again...");
                    return;
                }
                var baseurl = $"https://www.iesdouyin.com/web/api/v2/aweme/post/?count=99&sec_uid={uid}";
                var isnext = !onlyfirst;
                long cursor = 0;
                //视频列表
                var xdownlist = new ConcurrentBag<XFileInfo>();
                //相册列表
                var xalbumlist = new ConcurrentBag<XFileInfo>();
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
                            _log.Info($"douyin retry: {retry}...");
                        }
                        else
                        {
                            cursor = zresult.max_cursor;
                            foreach (var item in zresult.aweme_list)
                            {
                                if (item.aweme_type != 2 && item.aweme_type != 4)
                                {
                                    continue;
                                }

                                var nickname = item.author?.nickname ?? "";
                                nickname = nickname.RemoveIllegal();
                                var name = item.desc ?? item.aweme_id ?? "";
                                name = name.RemoveIllegal();
                                if (item.aweme_type == 2)
                                {
                                    //相册类型单独处理
                                    xalbumlist.Add(new XFileInfo
                                    {
                                        Id = item.aweme_id,
                                        Name = name,
                                        NickName = nickname,
                                        Url =
                                            $"https://www.iesdouyin.com/web/api/v2/aweme/iteminfo/?item_ids={item.aweme_id}",
                                        AwemeType = item.aweme_type
                                    });
                                    continue;
                                }
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

                                if (xdownlist.Any(x => x.Id == item.aweme_id))
                                {
                                    continue;
                                }

                                xdownlist.Add(new XFileInfo
                                {
                                    Id = item.aweme_id,
                                    Name = name,
                                    NickName = nickname,
                                    Url = xurl,
                                    AwemeType = item.aweme_type
                                });
                                _log.Info($"analysis: {name}...");
                            }
                        }
                    }
                } while (isnext);

                _log.Info("douyin analysis finished...");
                _log.Info("start download...");
                await Download(xdownlist, xsavepath, 4);
                if (xalbumlist.Count > 0)
                {
                    await AnalysisAlbums(xalbumlist, xsavepath, 2);
                }
            }
            catch (Exception)
            {
                _log.Error("download error, please try again... ");
                if (!string.IsNullOrEmpty(xsavepath))
                {
                    if (Directory.Exists(xsavepath))
                    {
                        Process.Start(xsavepath);
                    }
                }
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        /// <summary>
        /// 解析相册
        /// </summary>
        /// <param name="list"></param>
        /// <param name="xsavepath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static async Task AnalysisAlbums(ConcurrentBag<XFileInfo> list, string xsavepath, int type)
        {
            try
            {
                var xlist = new ConcurrentBag<XFileInfo>();
                var tasks = list.Select(item => Task.Run(async () =>
                {
                    try
                    {
                        var xresult = await item.Url.WithTimeout(60).GetJsonAsync<XAlbumModel>();
                        if (xresult?.item_list != null && xresult.item_list.Length > 0)
                        {
                            var ximgs = xresult.item_list.FirstOrDefault();
                            if (ximgs != null)
                            {
                                int xindex = 1;
                                foreach (var xitem in ximgs.images)
                                {
                                    var xurl = xitem.url_list.FirstOrDefault(x => x.Contains("jpeg"));
                                    if (string.IsNullOrEmpty(xurl))
                                    {
                                        xurl = xitem.url_list.FirstOrDefault() ?? "";
                                    }

                                    if (string.IsNullOrEmpty(xurl))
                                    {
                                        continue;
                                    }

                                    xlist.Add(new XFileInfo
                                    {
                                        Id = item.Id,
                                        Url = xurl,
                                        Name = item.Name,
                                        NickName = item.NickName,
                                        AwemeType = item.AwemeType,
                                        Index = xindex
                                    });
                                    xindex++;
                                }
                            }
                        }
                        else
                        {
                            _log.Info($"analysis: {item.Name}|{item.Id}, fail...");
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error($"task: {item.Name}|{item.Id} , error: {e}");
                        await Task.CompletedTask;
                    }
                }));
                await Task.WhenAll(tasks);
                if (xlist.Count > 0)
                {
                    await Download(xlist, xsavepath, type);
                }

                _log.Info("all album analysis finished...");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="list"></param>
        /// <param name="xsavepath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static async Task Download(ConcurrentBag<XFileInfo> list, string xsavepath, int type)
        {
            try
            {
                var rand = new Random();
                var tasks = list.Select(item => Task.Run(async () =>
                {
                    try
                    {
                        var zpath = xsavepath;
                        if (!string.IsNullOrEmpty(item.NickName))
                        {
                            zpath = zpath + item.NickName + @"\";
                            if (!Directory.Exists(zpath))
                            {
                                Directory.CreateDirectory(zpath);
                            }
                        }

                        var xfile = item.AwemeType == 4
                            ? $"{zpath}{item.Name}.mp4"
                            : $"{zpath}{item.Name}_{item.Index}.jpeg";
                        if (File.Exists(xfile))
                        {
                            _log.Info($"exists: {xfile}, skipped...");
                        }
                        else
                        {
                            var xresult = await item.Url.WithTimeout(60).GetAsync();
                            if (xresult != null && xresult.StatusCode == (int) HttpStatusCode.OK)
                            {
                                var response = xresult.ResponseMessage?.Content;
                                if (response != null)
                                {
                                    var stream = await response.ReadAsStreamAsync();
                                    using (var file = new FileStream(xfile, FileMode.Create, FileAccess.ReadWrite,
                                        FileShare.ReadWrite))
                                    {
                                        await stream.CopyToAsync(file);
                                        _log.Info($"download: {xfile}");
                                    }
                                }
                                else
                                {
                                    _log.Info($"download: {xfile} , nocontent");
                                }
                            }
                            else
                            {
                                _log.Info($"download: {xfile}, fail...");
                            }

                            if (item.AwemeType == 4)
                            {
                                await Task.Delay(rand.Next(1, 3) * 300);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error($"task: {item.Name} , error: {e}");
                        await Task.Delay(rand.Next(1, 5) * 400);
                        await Task.CompletedTask;
                    }
                })).ToList();
                await Task.WhenAll(tasks);
                var xtype = type == 4 ? "video" : "album";
                _log.Info($"all {xtype} download finished...");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}