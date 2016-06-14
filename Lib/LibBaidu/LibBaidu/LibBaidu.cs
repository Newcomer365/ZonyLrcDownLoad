﻿using System;
using System.Text;
using LibIPlug;
using LibNet;
using System.Text.RegularExpressions;
using System.Net;
using ID3;
using System.IO;

namespace LibBaidu
{
    [PluginInfoAttribute("百度音乐歌词","v1.0","Zony","从百度云音乐下载歌词。",0)]
    public class Baidu : IPlugin
    {
        public PluginInfoAttribute PluginInfo
        {
            get;set;
        }

        public bool Down(string filePath, ref byte[] lrcData,int iThread)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = iThread;
            string RequestURL = "http://music.baidu.com/search/lrc?key=";
            string ResponseURL = "http://music.baidu.com";
            var m_info = new MusicInfo(filePath, 0);
            var m_tools = new NetUtils();

            string getURL = RequestURL + m_info.Title;
            string lrcHtml = m_tools.Http_Get(getURL, Encoding.UTF8);

            if (lrcHtml == "") return false;

            Regex reg = new Regex(@"/data2/lrc/\d*/\d*.lrc");
            try
            {
                string result = reg.Match(lrcHtml).ToString();
                if (result == "") return false;

                lrcData = new WebClient().DownloadData(ResponseURL + result);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    class MusicInfo
    {
        #region 歌曲信息字段
        /// <summary>
        /// 歌曲标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 歌手名称
        /// </summary>
        public string Singer { get; set; }
        /// <summary>
        /// 歌曲路径
        /// </summary>
        public string MusicPath { get; set; }
        #endregion

        /// <summary>
        /// 获得歌曲文件信息
        /// </summary>
        /// <param name="filePath">源歌曲文件路径</param>
        /// <param name="flag">标志位，根据设置而定</param>
        public MusicInfo(string filePath, int flag)
        {
            try
            {
                // 尝试获取ID3标签
                ID3Info id3 = new ID3Info(filePath, true);
                Title = id3.ID3v1Info.Title != null ? id3.ID3v1Info.Title : id3.ID3v2Info.GetTextFrame("TIT2");
                Singer = id3.ID3v1Info.Artist != null ? id3.ID3v1Info.Artist : id3.ID3v2Info.GetTextFrame("TPE1");
                if (Title == "")
                {
                    // 尝试手工分割文件名
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    Title = fileName;
                    Singer = fileName;
                }
            }
            catch (Exception)
            {
                // 尝试手工分割文件名
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                Title = fileName;
                Singer = fileName;
            }

            MusicPath = filePath;
        }
    }
}