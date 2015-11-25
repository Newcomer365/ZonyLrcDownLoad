﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Zony_Lrc_Download_2._0
{
    /// <summary>
    /// 函数状态返回值
    /// </summary>
    public enum DownLoadReturn
    {
        /// <summary>
        /// 正常返回
        /// </summary>
        NORMAL=0,
        /// <summary>
        /// HTML页面无数据
        /// </summary>
        HTML_INVALID=-1,
        /// <summary>
        /// 发生异常
        /// </summary>
        EXCEPTION=-2,
        /// <summary>
        /// 搜寻下载链接失败
        /// </summary>
        REGEX_ERROR=-3,
        /// <summary>
        /// 文件创建失败
        /// </summary>
        FILE_CREAT_ERROR=-4,
        /// <summary>
        /// 网络错误
        /// </summary>
        INET_ERROR=-5
    }

    /// <summary>
    /// 百度歌词下载类
    /// </summary>
    public class LrcDownLoad
    {
        private WebClient m_client;
        private const string BAIDULRC = "http://music.baidu.com/search/lrc?key=";
        private const string BAIDUMUSCI = "http://music.baidu.com";

        /// <summary>
        /// 歌词搜索地址
        /// </summary>
        private string m_strSearchURL;
        
        public LrcDownLoad()
        {
            m_client = new WebClient();
        }
        
        /// <summary>
        /// 歌词下载函数
        /// </summary>
        /// <param name="filename">歌曲完整路径</param>
        /// <param name="filedata">下载回来的数据</param>
        /// <returns>状态</returns>
        public DownLoadReturn DownLoad(string filepath, ref byte[] filedata)
        {
            string t_songName = Path.GetFileNameWithoutExtension(filepath);
            m_strSearchURL = BAIDULRC + t_songName;
            byte[] lrcHtmlData = m_client.DownloadData(m_strSearchURL);
            string lrcHtmlString = Encoding.UTF8.GetString(lrcHtmlData);

            if("".Equals(lrcHtmlString)||lrcHtmlString=="")
            {
                #region 日志点
                Log.WriteLog(t_songName,"在DownLoad函数中发生：HTML页面数据为空。");
                #endregion
                return DownLoadReturn.HTML_INVALID;
            }

            //正则搜寻下载链接
            Regex reg = new Regex(@"/data2.*.lrc");
            try
            {
                string result = reg.Match(lrcHtmlString).ToString();
                if(result==""||"".Equals(result))
                {
                    return DownLoadReturn.REGEX_ERROR;
                }
                if("".Equals(lrcHtmlString)||lrcHtmlString=="")
                {
                    return DownLoadReturn.INET_ERROR;
                }

                // 获得LRC文件数据
                filedata = m_client.DownloadData(BAIDUMUSCI + result);

                return DownLoadReturn.NORMAL;
            }catch(Exception exp)
            {
                #region 日志点
                Log.WriteLog(t_songName,"发生异常：" + exp.ToString());
                #endregion
                /*throw (exp); 并不抛出，直接返回异常*/
                return DownLoadReturn.EXCEPTION;
            }
        }

        /// <summary>
        /// 将数据写入文件
        /// </summary>
        /// <param name="filedata">lrc文件数据</param>
        /// <param name="filepath">歌曲路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>状态</returns>
        public DownLoadReturn WriteFile(ref byte[] filedata,string filepath,int encoding)
        {
            string t_songName = Path.GetFileNameWithoutExtension(filepath);
            try
            {
                string lrcPath=Path.GetDirectoryName(filepath) + "\\" + t_songName + ".lrc";
                FileStream lrcFileStream = new FileStream(lrcPath, FileMode.Create);

                if(!File.Exists(lrcPath))
                {
                    Log.WriteLog(t_songName, "歌词文件创建失败。");
                    return DownLoadReturn.FILE_CREAT_ERROR;
                }
                // 输出编码选择
                switch (encoding)
                {
                    case 0:
                        filedata = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("gb2312"), filedata);
                        break;
                    case 1:
                        filedata = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("gb2312"), filedata);
                        break;
                    case 2:
                        // 默认UTF-8
                        break;
                    case 3:
                        filedata = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("BIG5"), filedata);
                        break;
                    case 4:
                        filedata = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("shift_jis"), filedata);
                        break;
                }

                lrcFileStream.Write(filedata, 0, filedata.Length);
                lrcFileStream.Close();
                return DownLoadReturn.NORMAL;
            }catch(Exception e)
            {
                #region 日志点
                Log.WriteLog(t_songName, "发生异常：" + e.ToString());
                #endregion
                /*throw (exp); 并不抛出，直接返回异常*/
                return DownLoadReturn.EXCEPTION;
            }
        }
    }
}