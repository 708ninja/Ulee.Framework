//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : INI File Class
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

using Ulee.DllImport.Win32;

namespace Ulee.Utils
{
    public class UlIniFile
    {
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public UlIniFile(string fileName = "")
        {
            this.fileName = fileName;
        }

        public bool IsExist()
        {
            return File.Exists(fileName);
        }

        public string Read(string section, string key)
        {
            StringBuilder strings = new StringBuilder(255);

            Win32.GetPrivateProfileString(
                section, key, "", strings, 255, fileName);

            return strings.ToString();
        }

        public void Write(string section, string key, string value)
        {
            Win32.WritePrivateProfileString(section, key, value, fileName);
        }

        public string GetString(string section, string key, string def="")
        {
            return Read(section, key);
        }

        public void SetString(string section, string key, string value)
        {
            Write(section, key, value);
        }

        public bool GetBoolean(string section, string key, bool def=false)
        {
            string str = GetString(section, key).Trim().ToLower();

            if (str == "") return def;

            return (str == "true") ? true : false;
        }

        public void SetBoolean(string section, string key, bool value)
        {
            string str = (value == true) ? "true" : "false";

            SetString(section, key, str);
        }

        public int GetInteger(string section, string key, int def = 0)
        {
            int value = 0;
            string str = GetString(section, key).Trim();

            if (str == "") return def;

            try
            {
                value = int.Parse(str);
            }
            catch
            {
                value = 0;
            }

            return value;
        }

        public void SetInteger(string section, string key, int value)
        {
            SetString(section, key, value.ToString());
        }

        public double GetDouble(string section, string key, double def = 0)
        {
            double value = 0;
            string str = GetString(section, key).Trim();

            if (str == "") return def;

            try
            {
                value = double.Parse(str);
            }
            catch
            {
                value = 0;
            }

            return value;
        }

        public void SetDouble(string section, string key, double value, string fmt="0.0")
        {
            SetString(section, key, value.ToString(fmt));
        }
    }
}
