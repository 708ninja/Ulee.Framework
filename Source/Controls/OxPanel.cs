
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

//------------------------------------------------------------------------------
namespace OxLib.Controls
{
	//--------------------------------------------------------------------------
	public enum EOxBevelStyle
	{
		None,
		Single,
		Lowered,
		Raised
	}

	//--------------------------------------------------------------------------
	public enum EOxHoriAlign
	{
		Left,
		Center,
		Right
	}

	//--------------------------------------------------------------------------
	public enum EOxVertAlign
	{
		Top,
		Middle,
		Bottom
	}

	[ToolboxBitmap(typeof(OxPanel), "OxPanel.bmp")]
	//--------------------------------------------------------------------------
	public partial class OxPanel : Panel
	{
		private EOxHoriAlign textHoriAlign;
		private EOxVertAlign textVertAlign;

		private int spacing;

		private Color inColor1, inColor2;
		private EOxBevelStyle bevelInner;

		private Color outColor1, outColor2;
		private EOxBevelStyle bevelOuter;

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
		[Category("Layout")]
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
		[Category("Layout")]
		public EOxHoriAlign TextHAlign
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
		[Category("Layout")]
		public EOxVertAlign TextVAlign
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
		public EOxBevelStyle BevelInner
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
				return outColor1;
			}
			set
			{
				outColor1 = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color OuterColor2
		{
			get
			{
				return outColor2;
			}
			set
			{
				outColor2 = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public EOxBevelStyle BevelOuter
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
		public OxPanel()
		{
			InitializeComponent();

			textHoriAlign = EOxHoriAlign.Center;
			textVertAlign = EOxVertAlign.Middle;

			inColor1 = Color.FromArgb(160, 160, 160);
			inColor2 = Color.White;
			bevelInner = EOxBevelStyle.None;

			outColor1 = Color.FromArgb(160, 160, 160);
			outColor2 = Color.White;
			bevelOuter = EOxBevelStyle.Lowered;

			// Set DoubleBuffered style
			SetStyle((ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint), true);
			UpdateStyles();

			Invalidate();
		}

		//----------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawBevel(e.Graphics);
			DrawText(e.Graphics);

			base.OnPaint(e);
		}

		//----------------------------------------------------------------------
		protected void DrawBevel(Graphics g)
		{
			Pen P1, P2;

			switch (bevelOuter)
			{
				case EOxBevelStyle.None:
					P1 = new Pen(BackColor, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P1, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P1, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;

				case EOxBevelStyle.Single:
					P1 = new Pen(outColor1, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P1, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P1, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;

				case EOxBevelStyle.Lowered:
					P1 = new Pen(outColor1, 1);
					P2 = new Pen(outColor2, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P2, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P2, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;

				case EOxBevelStyle.Raised:
					P1 = new Pen(outColor2, 1);
					P2 = new Pen(outColor1, 1);
					g.DrawLine(P1, new Point(0, Size.Height - 1), new Point(0, 0));
					g.DrawLine(P1, new Point(0, 0), new Point(Size.Width - 1, 0));
					g.DrawLine(P2, new Point(Size.Width - 1, 0), new Point(Size.Width - 1, Size.Height - 1));
					g.DrawLine(P2, new Point(Size.Width - 1, Size.Height - 1), new Point(0, Size.Height - 1));
					break;
			}

			switch (bevelInner)
			{
				case EOxBevelStyle.None:
					P1 = new Pen(BackColor, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P1, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P1, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;

				case EOxBevelStyle.Single:
					P1 = new Pen(inColor1, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P1, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P1, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;

				case EOxBevelStyle.Lowered:
					P1 = new Pen(inColor1, 1);
					P2 = new Pen(inColor2, 1);
					g.DrawLine(P1, new Point(1, Size.Height - 2), new Point(1, 1));
					g.DrawLine(P1, new Point(1, 1), new Point(Size.Width - 2, 1));
					g.DrawLine(P2, new Point(Size.Width - 2, 1), new Point(Size.Width - 2, Size.Height - 2));
					g.DrawLine(P2, new Point(Size.Width - 2, Size.Height - 2), new Point(1, Size.Height - 2));
					break;

				case EOxBevelStyle.Raised:
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
				case EOxHoriAlign.Left:
					fLeft = spacing;
					break;

				case EOxHoriAlign.Center:
					fLeft = (Size.Width / 2) - (TSize.Width / 2);
					break;

				case EOxHoriAlign.Right:
					fLeft = Size.Width - TSize.Width - spacing;
					break;
			}

			switch (textVertAlign)
			{
				case EOxVertAlign.Top:
					fTop = spacing;
					break;

				case EOxVertAlign.Middle:
					fTop = (Size.Height / 2) - (TSize.Height / 2);
					break;

				case EOxVertAlign.Bottom:
					fTop = Size.Height - TSize.Height - spacing;
					break;
			}

			g.DrawString(
				Text, Font, new SolidBrush(this.ForeColor), new PointF(fLeft, fTop));
		}
	}
}
//------------------------------------------------------------------------------
