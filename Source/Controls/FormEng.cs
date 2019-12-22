//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : English Form
//------------------------------------------------------------------------------
using System;
using System.Windows.Forms;

namespace Ulee.Controls
{
	public partial class UlFormEng : Form
	{
        private UlMenu defMenu;
        public UlMenu DefMenu
        {
            get { return defMenu; }
            set { defMenu = value; }
        }

        public UlFormEng()
		{
            InitializeComponent();
            defMenu = null;
        }

        public virtual void InvalidForm(object sender, EventArgs args)
        {
        }
	}
}
