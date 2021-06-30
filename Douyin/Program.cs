using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                    _log.Error("config file error, is about to be automatically generated , please run the software again...");
                    return;
                }

                var url = section["url"]?.Value ?? "";
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

                var uri = new Uri(result.Headers.FirstOrDefault(HeaderNames.Location));
                var uid = HttpUtility.ParseQueryString(uri.Query).Get("sec_uid") ?? "";
                var baseurl = $"https://www.iesdouyin.com/web/api/v2/aweme/post/?count=99&sec_uid={uid}";
                var isnext = !onlyfirst;
                long cursor = 0;
                var xdownlist = new ConcurrentBag<VideoInfo>();
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
                                var nickname = item.author?.nickname ?? "";
                                nickname = nickname.RemoveIllegal();
                                var name = item.desc ?? item.aweme_id ?? "";
                                name = name.RemoveIllegal();
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

                                xdownlist.Add(new VideoInfo
                                {
                                    Id = item.aweme_id,
                                    Name = name,
                                    NickName = nickname,
                                    Url = xurl
                                });
                                _log.Info($"analysis: {name}...");
                            }
                        }
                    }
                } while (isnext);

                _log.Info("douyin analysis finished...");
                _log.Info("start download...");
                await Download(xdownlist, xsavepath);
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
        /// 下载
        /// </summary>
        /// <param name="list"></param>
        /// <param name="xsavepath"></param>
        /// <returns></returns>
        private static async Task Download(ConcurrentBag<VideoInfo> list, string xsavepath)
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

                        var xfile = zpath + item.Name + ".mp4";
                        if (File.Exists(xfile))
                        {
                            _log.Info($"exists: {xfile}, skipped...");
                        }
                        else
                        {
                            var xresult = await item.Url.WithTimeout(60).GetAsync();
                            if (xresult!=null&&xresult.StatusCode == (int)HttpStatusCode.OK)
                            {
                                var response = xresult?.ResponseMessage?.Content;
                                if (response != null)
                                {
                                    var stream = await response.ReadAsStreamAsync();
                                    using (var file = new FileStream(xfile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
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

                            await Task.Delay(rand.Next(1, 3) * 300);
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
                _log.Info("all download finished...");
                Process.Start(xsavepath);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}