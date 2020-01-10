//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Panel Control
//------------------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System;

//------------------------------------------------------------------------------
namespace Ulee.Controls
{
	//--------------------------------------------------------------------------
	public enum EUlBevelStyle
	{
		None,
		Single,
		Lowered,
		Raised
	}

	//--------------------------------------------------------------------------
	public enum EUlHoriAlign
	{
		Left,
		Center,
		Right
	}

	//--------------------------------------------------------------------------
	public enum EUlVertAlign
	{
		Top,
		Middle,
		Bottom
	}

    [ToolboxBitmap(@"D:\ShDoc\Projects\VS\Ulee\Framework\Resources\UlPanel.bmp")]
    //--------------------------------------------------------------------------
    public partial class UlPanel : Panel
	{
		private EUlHoriAlign textHoriAlign;
		private EUlVertAlign textVertAlign;

		private int spacing;

		private Color inColor1, inColor2;
		private EUlBevelStyle bevelInner;

		private Color refColor1, refColor2;
		private EUlBevelStyle bevelOuter;

		//----------------------------------------------------------------------
		[Browsable(true)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		public new Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Layer")]
		public int Spacing
		{
			get
			{
				return spacing;
			}
			set
			{
				spacing = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Layer")]
		public EUlHoriAlign TextHAlign
		{
			get
			{
				return textHoriAlign;
			}
			set
			{
				textHoriAlign = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Layer")]
		public EUlVertAlign TextVAlign
		{
			get
			{
				return textVertAlign;
			}
			set
			{
				textVertAlign = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color InnerColor1
		{
			get
			{
				return inColor1;
			}
			set
			{
				inColor1 = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color InnerColor2
		{
			get
			{
				return inColor2;
			}
			set
			{
				inColor2 = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public EUlBevelStyle BevelInner
		{
			get
			{
				return bevelInner;
			}
			set
			{
				bevelInner = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color OuterColor1
		{
			get
			{
				return refColor1;
			}
			set
			{
				refColor1 = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color OuterColor2
		{
			get
			{
				return refColor2;
			}
			set
			{
				refColor2 = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public EUlBevelStyle BevelOuter
		{
			get
			{
				return bevelOuter;
			}
			set
			{
				bevelOuter = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		public UlPanel()
		{
			InitializeComponent();

			textHoriAlign = EUlHoriAlign.Center;
			textVertAlign = EUlVertAlign.Middle;

			inColor1 = Color.FromArgb(160, 160, 160);
			inColor2 = Color.White;
			bevelInner = EUlBevelStyle.None;

			refColor1 = Color.FromArgb(160, 160, 160);
			refColor2 = Color.White;
			bevelOuter = EUlBevelStyle.Lowered;

			// Set DoubleBuffered style
			SetStyle((ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint), true);
			UpdateStyles();

			Invalidate();
		}

		//----------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
            base.OnPaint(e);

            DrawBevel(e.Graphics);
			DrawText(e.Graphics);
		}

        //----------------------------------------------------------------------
        protected void DrawBevel(Graphics g)
		{
			Pen P1, P2;

			switch (bevelOuter)
			{
				case EUlBevelStyle.None:
					P1 = new Pen(BackColor, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P1, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P1, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;

				case EUlBevelStyle.Single:
					P1 = new Pen(refColor1, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P1, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P1, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;

				case EUlBevelStyle.Lowered:
					P1 = new Pen(refColor1, 1);
					P2 = new Pen(refColor2, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P2, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P2, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;

				case EUlBevelStyle.Raised:
					P1 = new Pen(refColor2, 1);
					P2 = new Pen(refColor1, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P2, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P2, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;
			}

			switch (bevelInner)
			{
				case EUlBevelStyle.None:
					P1 = new Pen(BackColor, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P1, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P1, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;

				case EUlBevelStyle.Single:
					P1 = new Pen(inColor1, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P1, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P1, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;

				case EUlBevelStyle.Lowered:
					P1 = new Pen(inColor1, 1);
					P2 = new Pen(inColor2, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P2, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P2, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;

				case EUlBevelStyle.Raised:
					P1 = new Pen(inColor2, 1);
					P2 = new Pen(inColor1, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P2, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P2, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;
			}
		}

		//----------------------------------------------------------------------
		protected void DrawText(Graphics g)
		{
			if (Text == "") return;

			float fLeft = 0;
			float fTop = 0;
			SizeF TSize = g.MeasureString(Text, Font);

			switch (textHoriAlign)
			{
				case EUlHoriAlign.Left:
					fLeft = spacing;
					break;

				case EUlHoriAlign.Center:
					fLeft = (Size.Width / 2) - (TSize.Width / 2);
					break;

				case EUlHoriAlign.Right:
					fLeft = Size.Width - TSize.Width - spacing;
					break;
			}

			switch (textVertAlign)
			{
				case EUlVertAlign.Top:
					fTop = spacing;
					break;

				case EUlVertAlign.Middle:
					fTop = (Size.Height / 2) - (TSize.Height / 2);
					break;

				case EUlVertAlign.Bottom:
					fTop = Size.Height - TSize.Height - spacing;
					break;
			}

			g.DrawString(
				Text, Font, new SolidBrush(this.ForeColor), new PointF(fLeft, fTop));
		}
	}
}
//------------------------------------------------------------------------------
