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
        private List<string> headList;

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

        public event LoggerMessageHandler LoggerMessage = null;
        protected void OnLoggerMessage(string str)
        {
            LoggerMessage?.Invoke(str);
        }

        public string this[int index]
        {
            set { Log(index, value); }
        }

        public string this[string head]
        {
            set { Log(head, value); }
        }

        //----------------------------------------------------------------------
        public UlLogger()
        {
            active = true;
            path = Directory.GetCurrentDirectory();
            fName = "Log";
            ext = "txt";
            fEncoding = Encoding.Default;
            fSeperation = EUlLogFileSeperation.Day;
            headList = new List<string>();
        }

        //----------------------------------------------------------------------
        ~UlLogger()
        {
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
        public void AddHead(string aStr)
        {
            headList.Add(aStr);
        }

        //----------------------------------------------------------------------
        public void LogRawString(string aStr)
        {
            Log("", aStr);
        }

        //----------------------------------------------------------------------
        public void Log(string fmt, params object[] args)
        {
            string sStr = string.Format(fmt, args);
            Log(sStr);
        }

        //----------------------------------------------------------------------
        public void Log(int aHead, string fmt, params object[] args)
        {
            string sStr = string.Format(fmt, args);
            Log(aHead, sStr);
        }

        //----------------------------------------------------------------------
        public void Log(string aStr)
        {
            Log(0, aStr);
        }

        //----------------------------------------------------------------------
        public void Log(int aHead, string aStr)
        {
            if (aHead >= headList.Count)
            {
                throw new Exception("Occurred ref of header list index error - UlLogger.Log");
            }

            Log(headList[aHead], aStr);
        }

        //----------------------------------------------------------------------
        public void Log(string aHead, string aStr)
        {
            string sStr;

            if (active == false) return;
			
			lock (this)
            {
				StreamWriter theStream = null;

                try
                {
                    string head = aHead.Trim().ToUpper();

                    theStream = new StreamWriter(GetFName(), true, fEncoding);

                    switch (head)
                    {
                        case "":
                            sStr = string.Format("{0} {1}",
                                DateTime.Now.ToString("HH:mm:ss.fff"), aStr).Trim();
                            break;

                        case "TIMELESS":
                            sStr = aStr;
                            break;

                        default:
                            sStr = string.Format("{0} [{1}] {2}",
                                DateTime.Now.ToString("HH:mm:ss.fff"), aHead, aStr).Trim();
                            break;
                    }

					theStream.WriteLine(sStr);
                    OnLoggerMessage(sStr);
                }
                catch
				{
					throw new Exception("Occurred file stream writing exception - UlLogger.Log");
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
