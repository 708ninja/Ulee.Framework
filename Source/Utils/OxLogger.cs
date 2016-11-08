//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

//------------------------------------------------------------------------------
namespace OxLib.Utils
{
    public enum EOxLogFileSeperation { Min, Hour, Day }

    //--------------------------------------------------------------------------
    public class OxLogger
    {
        private bool active;
        private string path;
        private string fName;
        private string ext;
        private Encoding fEncoding;
        private EOxLogFileSeperation fSeperation;
        private List<string> headList;

        public bool Active 
        { get { return active; } set { active = value; } }

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

        public EOxLogFileSeperation FSeperation 
        { get { return fSeperation; } set { fSeperation = value; } }

        //----------------------------------------------------------------------
        public OxLogger()
        {
            active = true;
            path = Directory.GetCurrentDirectory();
            fName = "Log";
            ext = "txt";
            fEncoding = Encoding.Default;
            fSeperation = EOxLogFileSeperation.Day;
            headList = new List<string>();
        }

        //----------------------------------------------------------------------
        ~OxLogger()
        {
        }

        //----------------------------------------------------------------------
        private string GetFName()
        {
            string sRet = path + fName;

            switch (fSeperation)
            {
                case EOxLogFileSeperation.Min:
                    sRet = sRet + "." + DateTime.Now.ToString("yyyy.MM.dd.HH.mm") + "." + ext;
                    break;

                case EOxLogFileSeperation.Hour:
                    sRet = sRet + "." + DateTime.Now.ToString("yyyy.MM.dd.HH") + "." + ext;
                    break;

                case EOxLogFileSeperation.Day:
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
        public void Log(string aStr)
        {
            Log("", aStr);
        }

        //----------------------------------------------------------------------
        public void Log(int aHead, string aStr)
        {
            if (aHead >= headList.Count)
            {
                throw new Exception("Occurred out of header list index error - OxLogger.Log");
            }

            Log(headList[aHead], aStr);
        }

        //----------------------------------------------------------------------
        public void Log(string aHead, string aStr)
        {
			
			if (active == false) return;
			
			lock (this)
            {
				StreamWriter theStream = null;

				try
				{
					string sStr;
					theStream = new StreamWriter(GetFName(), true, fEncoding);

					if (aHead.Trim() == "")
					{
						sStr = string.Format("{0} {1}",
							DateTime.Now.ToString("HH:mm:ss.fff"), aStr);
					}
					else
					{
						sStr = string.Format("{0} [{1}] {2}",
							DateTime.Now.ToString("HH:mm:ss.fff"), aHead, aStr);
					}

					theStream.WriteLine(sStr.Trim());
				}
				catch (Exception)
				{
					throw new Exception("Occurred file stream writing exception - OxLogger.Log");
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
