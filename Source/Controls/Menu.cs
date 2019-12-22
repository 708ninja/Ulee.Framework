//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Menu Control
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

//------------------------------------------------------------------------------
namespace Ulee.Controls
{
	//--------------------------------------------------------------------------
    public class UlMenu
    {
        private int index;
        private Panel panel;
        private Label label;
        private Color flatButtonColor;
        private bool enabled;
        private List<SimpleButton> buttons;

        //----------------------------------------------------------------------
        public UlMenu(Panel APanel)
        {
            index = -1;
            this.panel = APanel;
            label = null;
            flatButtonColor = Color.Silver;
            enabled = true;

			buttons = new List<SimpleButton>();
        }

        //----------------------------------------------------------------------
        public UlMenu(Panel APanel, Label ALabel)
            : this(APanel)
        {
            label = ALabel;
        }

        //----------------------------------------------------------------------
        public UlMenu(Panel APanel, Label ALabel, Color AColor)
            : this(APanel)
        {
            label = ALabel;
            flatButtonColor = AColor;
        }

		//----------------------------------------------------------------------
        public void Close()
        {
            foreach (UserControl C in panel.Controls)
            {
                C.Dispose();
            }
        }

        //----------------------------------------------------------------------
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                foreach (SimpleButton btn in buttons)
                {
                    btn.Enabled = value;
                }
                enabled = value;
            }
        }

        //----------------------------------------------------------------------
        public Color FlatButtonColor 
        { 
            set { flatButtonColor = value; } 
        }

        //----------------------------------------------------------------------
        public int Index
        {
            get { return index; }
            set
            {
                // If there are registered buttons, occurr the Click event.
                if ((buttons.Count > 0) && (value < buttons.Count))
                {
                    OnClick(buttons[value], new EventArgs());
                }
                else
                {
                    // Hide all forms
                    foreach (UserControl C in panel.Controls)
                    {
                        C.Hide();
                    }

                    // Show active form
                    panel.Controls[value].Show();
                }
            }
        }

        public int ControlsCount
        {
            get { return panel.Controls.Count; }
        }

        public Control Controls(int index)
        {
            Control ctrl = null;

            foreach (Control C in panel.Controls)
            {
                if ((int)C.Tag == index)
                {
                    ctrl = C;
                    break;
                }
            }

            return ctrl;
        }

        public Control ActiveControl
        { get { return Controls(Index); } }

        //----------------------------------------------------------------------
        private void Add(UserControl AControl)
        {
            AControl.Tag = buttons.Count;
            AControl.Dock = DockStyle.Fill;
            panel.Controls.Add(AControl);
        }

        //----------------------------------------------------------------------
        public void Add(UserControl AControl, SimpleButton AButton)
        {
            Add(AControl);

            AButton.Tag = buttons.Count;
            AButton.AllowFocus = false;
            AButton.TabStop = false;
            AButton.Appearance.BackColor2 = Color.Empty;
            AButton.Appearance.BorderColor = Color.Black;
            AButton.Click += new EventHandler(OnClick);

            buttons.Add(AButton);
        }

        //----------------------------------------------------------------------
        private void OnClick(object sender, EventArgs e)
        {
            int nIndex = (int)(sender as SimpleButton).Tag;

            if (index != nIndex)
            {
                // Hide all forms
                foreach (UserControl C in panel.Controls)
                {
                    if ((int)C.Tag == nIndex)
                    {
                        C.Show();
                        C.Focus();
                    }
                    else
                    {
                        C.Hide();
                    }
                }

                // Set all buttons to Standard
                foreach (SimpleButton B in buttons)
                {
                    if ((int)B.Tag == nIndex)
                    {
                        B.PaintStyle = PaintStyles.Light;
                    }
                    else
                    {
                        B.PaintStyle = PaintStyles.Default;
                    }
                }

                // Show menu label
                if (label != null)
                {
                    label.Text = buttons[nIndex].Text;
                }

                index = nIndex;
            }
        }
    }
}
//------------------------------------------------------------------------------
