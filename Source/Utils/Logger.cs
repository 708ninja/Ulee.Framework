//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Logger Class
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

//------------------------------------------------------------------------------
namespace Ulee.Utils
{
    public enum ELogTime { None, Second, Millisecond }

    public enum ELogTag { None, Note, Warning, Error, Exception }

    public enum EUlLogFileSeperation { Min, Hour, Day }

    public delegate void LoggerMessageHandler(string str);

    //--------------------------------------------------------------------------
    public class UlLogger
    {
        private bool active;
        private string path;
        private string fName;
        private string ext;
        private Encoding fEncoding;
        private EUlLogFileSeperation fSeperation;
        private ELogTime timeType;
        private List<string> tags;

        public bool Active 
        {
            get { return active; }
            set { active = value; }
        }

        public string Path 
        { 
            get { return path; } 
            set 
            { 
                path = value;
                if ((path.Length>2) && (path.Substring(path.Length-2, 2)!="\\"))
                {
                    path += "\\";
                }
            } 
        }

        public string FName
        { get { return fName; } set { fName = value; } }

        public string Ext
        { get { return ext; } set { ext = value; } }

        public Encoding FEncoding
        { get { return fEncoding; } set { fEncoding = value; } }

        public EUlLogFileSeperation FSeperation 
        { get { return fSeperation; } set { fSeperation = value; } }

        public ELogTime TimeType
        {
            get { return timeType; }
            set { timeType = value; }
        }

        public event LoggerMessageHandler LoggerMessage = null;
        protected void OnLoggerMessage(string str)
        {
            LoggerMessage?.Invoke(str);
        }

        public string this[int tag]
        {
            set { Log(tag, value); }
        }

        public string this[ELogTag tag]
        {
            set { Log((int)tag, value); }
        }

        public string this[string tag]
        {
            set { Log(tag, value); }
        }

        //----------------------------------------------------------------------
        public UlLogger()
        {
            active = true;
            timeType = ELogTime.Millisecond;
            path = Directory.GetCurrentDirectory();
            fName = "Log";
            ext = "txt";
            fEncoding = Encoding.Default;
            fSeperation = EUlLogFileSeperation.Day;

            tags = new List<string>();
            tags.Add("");
            tags.Add("NOTE");
            tags.Add("WARNING");
            tags.Add("ERROR");
            tags.Add("EXCEPTION");
        }

        //----------------------------------------------------------------------
        ~UlLogger()
        {
        }

        //----------------------------------------------------------------------
        public void Clear()
        {
            tags.Clear();
        }

        //----------------------------------------------------------------------
        private string GetFName()
        {
            string sRet = path + fName;

            switch (fSeperation)
            {
                case EUlLogFileSeperation.Min:
                    sRet = sRet + "." + DateTime.Now.ToString("yyyy.MM.dd.HH.mm") + "." + ext;
                    break;

                case EUlLogFileSeperation.Hour:
                    sRet = sRet + "." + DateTime.Now.ToString("yyyy.MM.dd.HH") + "." + ext;
                    break;

                case EUlLogFileSeperation.Day:
                    sRet = sRet + "." + DateTime.Now.ToString("yyyy.MM.dd") + "." + ext;
                    break;
            }

            return sRet;
        }

        //----------------------------------------------------------------------
        public void AddTag(string aStr)
        {
            tags.Add(aStr);
        }

        //----------------------------------------------------------------------
        public void Log(string fmt, params object[] args)
        {
            string sStr = string.Format(fmt, args);
            Log(sStr);
        }

        //----------------------------------------------------------------------
        public void Log(int aTag, string fmt, params object[] args)
        {
            string sStr = string.Format(fmt, args);
            Log(aTag, sStr);
        }

        //----------------------------------------------------------------------
        public void Log(string aStr)
        {
            Log(0, aStr);
        }

        //----------------------------------------------------------------------
        public void Log(int aTag, string aStr)
        {
            if (aTag >= tags.Count)
            {
                throw new Exception("Occurred ref of Tager list index error - UlLogger.Log");
            }

            Log(tags[aTag], aStr);
        }

        //----------------------------------------------------------------------
        public void Log(string aTag, string aStr)
        {
            string sStr = "";

            if (active == false) return;
			
			lock (this)
            {
				StreamWriter theStream = null;

                try
                {
                    theStream = new StreamWriter(GetFName(), true, fEncoding);

                    switch (timeType)
                    {
                        case ELogTime.Second:
                            sStr = $"{DateTime.Now.ToString("HH:mm:ss ")}";
                            break;

                        case ELogTime.Millisecond:
                            sStr = $"{DateTime.Now.ToString("HH:mm:ss.fff ")}";
                            break;
                    }

                    switch (aTag)
                    {
                        case "":
                            sStr += aStr.Trim();
                            break;

                        default:
                            sStr += $"[{aTag}] {aStr.Trim()}";
                            break;
                    }

					theStream.WriteLine(sStr);
                    OnLoggerMessage(sStr);
                }
                catch (Exception e)
				{
					throw new Exception($"{e.ToString()} - UlLogger.Log");
				}
				finally
				{
					if (theStream != null)
					{
						theStream.Close();
					}
				}
			}
        }

        //----------------------------------------------------------------------
        public void Delete(
            int aDays, SearchOption aOption=SearchOption.TopDirectoryOnly)
        {
            string[] theFiles = Directory.GetFiles(path, "*."+ext, aOption);

            foreach (string sFName in theFiles)
            {
                DateTime theDate = File.GetCreationTime(sFName);

				if ((DateTime.Now-theDate).Days > aDays)
                {
					File.Delete(sFName);
                }
            }
        }
    }
}
