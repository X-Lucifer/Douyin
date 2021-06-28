# Douyin
抖音视频无水印批量解析和下载

#### 功能说明:
> 抖音用户视频批量解析<br />
> 支持最高清的格式<br />
> 自动解析和下载<br />
> 支持最新视频解析<br />

#### 支持系统:
> Window 7+ <br />
> 默认需要.NET Framework 4.8+ 运行环境, 无运行环境, 请点击[这里](https://dotnet.microsoft.com/download/dotnet-framework)自行下载安装即可.<br />
> 自动解析和下载<br />
> 支持最新视频解析<br />

#### 下载地址:
> 点击[这里](https://github.com/X-Lucifer/Douyin/releases)下载最新版本<br />
> 软件分为安装版和便携版, 安装版需要安装才能使用, 便携版解压即可直接使用<br />
> 安装版: Douyin.Tools.exe<br />
> 便携版: Douyin.Tools.Portable.zip<br />

#### 使用说明:
> 打开根目录下的config.ini配置文件, 如不存在, 可直接运行一次软件, 会自动生成<br />
> config.ini内容示例如下:
```ini
[douyin]
;抖音用户主页地址
url=https://v.douyin.com/e4vSGrG/

;视频保存路径, 不填默认当前目录
savepath=E:\Download\Douyin\Download\

;是否只解析第一页, 默认为false
onlyfirst=false
```
> url： 抖音用户主页地址, 可打开抖音,点击 "分享"->"复制地址"即可得到用户主页地址<br />
> savepath: 视频下载目录, 留空会默认下载到当前根目录下的Download文件夹中<br />
> onlyfirst: 是否只下载第一页视频, 可选的值为true和false<br />
