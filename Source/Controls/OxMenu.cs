using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

//------------------------------------------------------------------------------
namespace OxLib.Controls
{
	//--------------------------------------------------------------------------
    public class OxMenu
    {
        private int index;
        private Panel panel;
        private Label label;
        private Color flatButtonColor;
        private List<SimpleButton> buttons;

        //----------------------------------------------------------------------
        public OxMenu(Panel APanel)
        {
            index = -1;
            panel = APanel;
            label = null;
            flatButtonColor = Color.Silver;

			buttons = new List<SimpleButton>();
        }

        //----------------------------------------------------------------------
        public OxMenu(Panel APanel, Label ALabel)
            : this(APanel)
        {
            label = ALabel;
        }

        //----------------------------------------------------------------------
        public OxMenu(Panel APanel, Label ALabel, Color AColor)
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
        public Color FlatButtonColor 
        { 
            set 
            { 
                flatButtonColor = value; 
            } 
        }

        //----------------------------------------------------------------------
        public int Index
        {
            get
            {
                return index;
            }
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

        //----------------------------------------------------------------------
        private void Add(UserControl AControl)
        {
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
			AButton.Appearance.BackColor   = flatButtonColor;
			AButton.Appearance.BackColor2  = Color.Empty;
			AButton.Appearance.BorderColor = Color.Black;
            AButton.Click +=  new EventHandler(OnClick);
            
			buttons.Add(AButton);
        }

        //----------------------------------------------------------------------
        private void OnClick(object sender, EventArgs e)
        {
			int nIndex = (int) (sender as SimpleButton).Tag;

			if (index != nIndex)
			{
				// Hide all forms
				foreach (UserControl C in panel.Controls)
				{
					C.Hide();
				}
				// Show active form
				panel.Controls[nIndex].Show();

				// Set all buttons to Standard
				foreach (SimpleButton B in buttons)
				{
					B.ButtonStyle = BorderStyles.Default;
				}

				// Set active button to Flat
				buttons[nIndex].ButtonStyle = BorderStyles.UltraFlat;

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
