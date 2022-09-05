using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel.Design;
using System.ComponentModel;

namespace GameLauncher
{
	public partial class AlphaForm : System.Windows.Forms.Form
	{
		public AlphaForm()
		{
			if (!this.DesignMode)
			{
				m_layeredWnd = new LayeredWindow();
			}

			m_sizeMode = SizeModes.None;
			m_background = null;
			m_backgroundEx = null;
			m_backgroundFull = null;
			m_renderCtrlBG = false;
			m_enhanced = false;
			m_isDown.Left = false;
			m_isDown.Right = false;
			m_isDown.Middle = false;
			m_isDown.XBtn = false;
			m_moving = false;
			m_hiddenControls = new List<Control>();
			m_controlDict = new Dictionary<Control, bool>();
			m_initialised = false;

			//Set drawing styles
			this.SetStyle(ControlStyles.DoubleBuffer, true);
		}

		#region Properties
		public enum SizeModes
		{
			None,
			Stretch,
			Clip
		}

		/// <summary>
		/// Gets or Sets the image to be blended with the desktop
		/// </summary>
		[Category("AlphaForm")]
		public Bitmap BlendedBackground
		{
			get { return m_background; }
			set
			{
				if (m_background != value)
				{
					m_background = value;
					UpdateLayeredBackground();
				}
			}
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_layeredWnd != null)
                {
                    m_layeredWnd.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// If true, a portion of the background image will be drawn behind
        /// each control on the form. This is to solve problems with some
        /// controls that need to blend with the background, Labels are an 
        /// excellent example.
        /// </summary>
        [Category("AlphaForm")]
		public bool DrawControlBackgrounds
		{
			get { return m_renderCtrlBG; }
			set
			{
				if (m_renderCtrlBG != value)
				{
					m_renderCtrlBG = value;
					UpdateLayeredBackground();
				}
			}
		}

		/// <summary>
		/// If true when the form is dragged the foreground window will be drawn to the
		/// background window and then hidden. This prevents any visual disparity between
		/// the two forms.
		/// </summary>
		[Category("AlphaForm")]
		public bool EnhancedRendering
		{
			get { return m_enhanced; }
			set { m_enhanced = value; }
		}

		/// <summary>
		/// Sets the size mode of the form.
		///   None: The background image will always remain its original size
		///   Stretch: The background will be resized to fit the client area of the main form
		///   Clip: The background image will be clipped to within the client area of the main form
		/// </summary>
		[Category("AlphaForm")]
		public SizeModes SizeMode
		{
			get { return m_sizeMode; }
			set
			{
				m_sizeMode = value;
				UpdateLayeredBackground();
			}
		}

		public void SetOpacity(double Opacity)
		{
			this.Opacity = Opacity;
			if (m_background != null)
			{
				int width = this.ClientSize.Width;
				int height = this.ClientSize.Height;
				if(m_sizeMode == SizeModes.None)
				{
					width = m_background.Width;
					height = m_background.Height;
				}

				byte _opacity = (byte)(this.Opacity * 255);
				if (m_useBackgroundEx)
				{
					m_layeredWnd.UpdateWindow(m_backgroundEx, _opacity, width, height, m_layeredWnd.LayeredPos);
				}
				else
				{
					m_layeredWnd.UpdateWindow(m_background, _opacity, width, height, m_layeredWnd.LayeredPos);
				}
			}
		}

		public void UpdateLayeredBackground()
		{
			updateLayeredBackground(this.ClientSize.Width, this.ClientSize.Height);
		}

		public void DrawControlBackground(Control ctrl, bool drawBack)
		{
			if (m_controlDict.ContainsKey(ctrl))
				m_controlDict[ctrl] = drawBack;
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//Set the transparency key to make the back of the form transparent
			//INTERESTING NOTE: If you use Fuchsia as the transparent colour then
			// it will be not only visually transparent but also transparent to 
			// mouse clicks. If you use any other colour then you will be able to
			// see through it, but you'll still get your mouse events
			this.BackColor = Color.Fuchsia;
			this.TransparencyKey = Color.Fuchsia;
			this.AllowTransparency = true;
			
			//Work out any offset to position the background form, in the event that
			//the borders are still active
			Point screen = new Point(0, 0);
			screen = this.PointToScreen(screen);
			m_offX = screen.X - this.Location.X;
			m_offY = screen.Y - this.Location.Y;

			if (!this.DesignMode)
			{
				//Disable the form so that it cannot receive focus
				//We need to do this so that the form will not get focuse
				// by any means and then be positioned above our main form
				Point formLoc = this.Location;
				formLoc.X += m_offX;
				formLoc.Y += m_offY;
				m_layeredWnd.Text = "AlphaForm";
				m_initialised = true;
				updateLayeredBackground(this.ClientSize.Width, this.ClientSize.Height, formLoc, true);
				m_layeredWnd.Show();
				
				m_layeredWnd.Enabled = false;

				//Subclass the background window so that we can intercept its messages
				m_customLayeredWindowProc = new Win32.Win32WndProc(this.LayeredWindowWndProc);
				m_layeredWindowProc = Win32.SetWindowLong(m_layeredWnd.Handle, (uint)Win32.Message.GWL_WNDPROC, m_customLayeredWindowProc);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (m_background != null)
			{
				//If we are in design mode then we can't see the blended background,
				//so we'll just draw it because we're so friendly and helpful
				if (this.DesignMode)
				{
					e.Graphics.DrawImage(m_background, 0, 0, m_background.Width, m_background.Height);
				}
				else if(!m_moving && m_renderCtrlBG)
				{
					//If desired we render a portion of the background image behind
					//each control on our form, these sections are also cut out from the
					//background image. This resolves any issues when the opacity of the 
					//form is less then 1.0 and controls would blend with the background 
					//image instead of the desktop.
					foreach (KeyValuePair<Control, bool> kvp in m_controlDict)
					{
						Control ctrl = kvp.Key;
						bool drawBack = kvp.Value;
						if (drawBack && ctrl.BackColor == Color.Transparent)
						{
							Rectangle rect = ctrl.ClientRectangle;
							rect.X = ctrl.Left;
							rect.Y = ctrl.Top;

							if(m_useBackgroundEx)
								e.Graphics.DrawImage(m_backgroundFull, rect, rect, GraphicsUnit.Pixel);
							else
								e.Graphics.DrawImage(m_background, rect, rect, GraphicsUnit.Pixel);
						}
					}
				}
			}
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			if(!m_controlDict.ContainsKey(e.Control))
				m_controlDict.Add(e.Control, true);
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);
			if (m_controlDict.ContainsKey(e.Control))
				m_controlDict.Remove(e.Control);
		}

		private void updateLayeredBackground(int width, int height, Point pos)
		{
			updateLayeredBackground(width, height, pos, true);
		}

		private void updateLayeredBackground(int width, int height)
		{
			updateLayeredBackground(width, height, m_layeredWnd.LayeredPos, true);
		}

		private void updateLayeredBackground(int width, int height, Point pos, bool Render)
		{
			m_useBackgroundEx = false;
			if (this.DesignMode || m_background == null || !m_initialised)
				return;

			switch (m_sizeMode)
			{
				case SizeModes.Stretch:
					m_useBackgroundEx = true;
					break;

				case SizeModes.Clip:
					//Do nothing, use the width and height provided to
					//clip the background image
					break;

				case SizeModes.None:
					//Always use the width and height of the image,
					//regardless of the size of the window
					width = m_background.Width;
					height = m_background.Height;
					break;
			}

			//Create the extended image with the approproate size
			if ((m_renderCtrlBG || m_useBackgroundEx) && Render)
			{
				if (m_backgroundEx != null)
				{
					m_backgroundEx.Dispose();
					m_backgroundEx = null;
				}
				if (m_backgroundFull != null)
				{
					m_backgroundFull.Dispose();
					m_backgroundFull = null;
				}

				if (m_sizeMode == SizeModes.Clip)
					m_backgroundEx = new Bitmap(m_background);
				else
					m_backgroundEx = new Bitmap(m_background, width, height);

				m_backgroundFull = new Bitmap(m_backgroundEx);
			}

			//Cut out portions of the alpha background that will be drawn by 
			//the main form
			if (m_renderCtrlBG)
			{
				if (Render)
				{
					Graphics g = Graphics.FromImage(m_backgroundEx);
					foreach (KeyValuePair<Control, bool> kvp in m_controlDict)
					{
						Control ctrl = kvp.Key;
						bool drawBack = kvp.Value;
						if (drawBack && ctrl.BackColor == Color.Transparent)
						{
							Rectangle rect = ctrl.ClientRectangle;
							rect.X = ctrl.Left;
							rect.Y = ctrl.Top;
							g.FillRectangle(Brushes.Fuchsia, rect);
						}
					}
					g.Dispose();
					m_backgroundEx.MakeTransparent(Color.Fuchsia);
				}
				m_useBackgroundEx = true;
			}

			byte _opacity = (byte)(this.Opacity * 255);
			if (m_useBackgroundEx)
			{
				m_layeredWnd.UpdateWindow(m_backgroundEx, _opacity, width, height, pos);
			}
			else
			{
				m_layeredWnd.UpdateWindow(m_background, _opacity, width, height, pos);
			}
		}

		private void updateLayeredSize(int width, int height)
		{
			updateLayeredSize(width, height, false);
		}

		private void updateLayeredSize(int width, int height, bool forceUpdate)
		{
			//The size hasn't changed, no need to do anything
			if (!m_initialised)
				return;

			if (!forceUpdate && (width == m_layeredWnd.LayeredSize.Width && height == m_layeredWnd.LayeredSize.Height))
				return;

			switch (m_sizeMode)
			{
				case SizeModes.None:
					break;

				case SizeModes.Stretch:
					{
						updateLayeredBackground(width, height);
						this.Invalidate(false);
					}
					break;

				case SizeModes.Clip:
					{
						//No need to modify any images, just set the new size
						byte _opacity = (byte)(this.Opacity * 255);
						if (m_useBackgroundEx)
						{
							m_layeredWnd.UpdateWindow(m_backgroundEx, _opacity, width, height, m_layeredWnd.LayeredPos);
						}
						else
						{
							m_layeredWnd.UpdateWindow(m_background, _opacity, width, height, m_layeredWnd.LayeredPos);
						}
					}
					break;
			}
		}

        /// <summary>
		/// Simple function to determine if the user has done a double-click
		/// </summary>
		/// <param name="pos">The position of the mouse</param>
		/// <returns></returns>
		private bool dblClick(Point pos)
        {
            TimeSpan elapsed = DateTime.Now - m_clickTime;
            Size dist = new Size();
            dist.Width = Math.Abs(m_lockedPoint.X - pos.X);
            dist.Height = Math.Abs(m_lockedPoint.Y - pos.Y);

            if (elapsed.Milliseconds <= SystemInformation.DoubleClickTime
                && dist.Width <= SystemInformation.DoubleClickSize.Width
                && dist.Height <= SystemInformation.DoubleClickSize.Height)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Override for the main forms WndProc method
        /// This is used to intercept the forms movement so that we can move
        /// blended background form at the same time, as well as check for 
        /// when the form is activated so we can ensure the Z-order of our
        /// forms remains intact.
        /// </summary>
        /// <param name="m">Windows Message</param>
        protected override void WndProc(ref Message m)
        {
            if (this.DesignMode)
            {
                base.WndProc(ref m);
                return;
            }

            Win32.Message msgId = (Win32.Message)m.Msg;
            bool UseBase = true;
            switch (msgId)
            {
                case Win32.Message.WM_LBUTTONUP:
                    {
                        //Just in case
                        if (Win32.GetCapture() != IntPtr.Zero)
                            Win32.ReleaseCapture();
                    }
                    break;

                case Win32.Message.WM_ENTERSIZEMOVE:
                    {

                    }
                    break;

                case Win32.Message.WM_EXITSIZEMOVE:
                    {
                        //We've stopped dragging the form, so lets make sure that our values are correct
                        m_isDown.Left = false;
                        m_moving = false;

                        if (m_enhanced)
                        {
                            this.SuspendLayout();
                            foreach (Control ctrl in m_hiddenControls)
                                ctrl.Visible = true;
                            m_hiddenControls.Clear();
                            this.ResumeLayout();
                            updateLayeredBackground(this.ClientSize.Width, this.ClientSize.Height, m_layeredWnd.LayeredPos, false);
                        }
                    }
                    break;

                case Win32.Message.WM_MOUSEMOVE:
                    //It's unlikely that we will get here unless this we really have captured the mouse
                    //because the entire thing is transparent, but we check anyway just to make sure
                    if (Win32.GetCapture() != IntPtr.Zero && m_moving)
                    {
                        //In enhanced mode we draw the main window to the layered window and then hide the main
                        //window. This is so that we can have perfectly smooth motion when dragging the form, as
                        //we cannot gurantee that the forms will ever be moved together otherwise
                        if (m_enhanced)
                        {
                            //Setup the device contexts we are going to use
                            IntPtr hdcScreen = Win32.GetWindowDC(m_layeredWnd.Handle);  //Screen DC that the layered window will draw to
                            IntPtr windowDC = Win32.GetDC(this.Handle);                 //Window DC that we are going to copy
                            IntPtr memDC = Win32.CreateCompatibleDC(windowDC);          //Temporary DC that we draw to
                            IntPtr BmpMask = Win32.CreateBitmap(this.ClientSize.Width,
                                            this.ClientSize.Height, 1, 1, IntPtr.Zero); //Mask bitmap so that we only draw areas of the form that are visible

                            Bitmap backImage = m_useBackgroundEx ? m_backgroundFull : m_background;
                            IntPtr BmpBack = backImage.GetHbitmap(Color.FromArgb(0));   //Background Image

                            //Create mask
                            Win32.SelectObject(memDC, BmpMask);
                            uint oldCol = Win32.SetBkColor(windowDC, 0x00FF00FF);
                            Win32.BitBlt(memDC, 0, 0, this.ClientSize.Width, this.ClientSize.Height, windowDC, 0, 0, Win32.TernaryRasterOperations.SRCCOPY);
                            Win32.SetBkColor(windowDC, oldCol);

                            //Blit window to background image using mask
                            //We need to use the SPno raster operation with a white brush to combine our window
                            //with a black backround before putting it onto the 32-bit background image, otherwise
                            //we end up with blending issues (source and destination colours are ANDed together)
                            Win32.SelectObject(memDC, BmpBack);
                            IntPtr brush = Win32.CreateSolidBrush(0x00FFFFFF);
                            Win32.SelectObject(memDC, brush);
                            Win32.MaskBlt(memDC, 0, 0, backImage.Width, backImage.Height, windowDC, 0, 0, BmpMask, 0, 0, 0xCFAA0020);
                            //Win32.BitBlt(memDC, 0, 0, backImage.Width, backImage.Height, windowDC, m_offX, m_offY, Win32.TernaryRasterOperations.SRCCOPY);

                            Point zero = new Point(0, 0);
                            Size size = m_layeredWnd.LayeredSize;
                            Point pos = m_layeredWnd.LayeredPos;
                            Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
                            blend.AlphaFormat = (byte)Win32.BlendOps.AC_SRC_ALPHA;
                            blend.BlendFlags = (byte)Win32.BlendFlags.None;
                            blend.BlendOp = (byte)Win32.BlendOps.AC_SRC_OVER;
                            blend.SourceConstantAlpha = (byte)(this.Opacity * 255);

                            Win32.UpdateLayeredWindow(m_layeredWnd.Handle, hdcScreen, ref pos, ref size, memDC, ref zero, 0, ref blend, Win32.BlendFlags.ULW_ALPHA);

                            //Clean up
                            Win32.ReleaseDC(IntPtr.Zero, hdcScreen);
                            Win32.ReleaseDC(this.Handle, windowDC);
                            Win32.DeleteDC(memDC);
                            Win32.DeleteObject(brush);
                            Win32.DeleteObject(BmpMask);
                            Win32.DeleteObject(BmpBack);

                            //Hide controls that are visible
                            this.SuspendLayout();
                            foreach (Control ctrl in this.Controls)
                            {
                                if (ctrl.Visible)
                                {
                                    m_hiddenControls.Add(ctrl);
                                    ctrl.Visible = false;
                                }
                            }
                            this.ResumeLayout();
                        }

                        //If we do not release the mouse then Windows will not start dragging the form, also
                        //it will mess up mouse input to any border on our form and other windows on the desktop
                        Win32.ReleaseCapture();
                        Win32.SendMessage(this.Handle, (int)Win32.Message.WM_NCLBUTTONDOWN, (int)Win32.Message.HTCAPTION, 0);

                    }
                    break;

                case Win32.Message.WM_SIZE:
                    {
                        //The updateLayeredSize function will check the width and height
                        //we pass in, so we don't need to worry about updating the window
                        //with the same size it already had
                        int width = m.LParam.ToInt32() & 0xFFFF;
                        int height = m.LParam.ToInt32() >> 16;
                        this.updateLayeredSize(width, height);
                    }
                    break;

                case Win32.Message.WM_WINDOWPOSCHANGING:
                    {
                        Win32.WINDOWPOS posInfo = (Win32.WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(Win32.WINDOWPOS));

                        //We will cancel this movement, and send our own messages to position both forms
                        //this way we can ensure that both forms are moved together and that
                        //the Z order is unchanged.
                        Win32.WindowPosFlags move_size = Win32.WindowPosFlags.SWP_NOMOVE | Win32.WindowPosFlags.SWP_NOSIZE;
                        if ((posInfo.flags & move_size) != move_size)
                        {
                            //Check for my own messages, which I do by setting to hwndInsertAfter to our 
                            //own window, which from what I can gather only happens when you resize your
                            //window, never when you move it
                            if (posInfo.hwndInsertAfter != this.Handle)
                            {
                                IntPtr hwdp = Win32.BeginDeferWindowPos(2);
                                if (hwdp != IntPtr.Zero)
                                    hwdp = Win32.DeferWindowPos(hwdp, m_layeredWnd.Handle, this.Handle, posInfo.x + m_offX, posInfo.y + m_offY,
                                            0, 0, (uint)(posInfo.flags | Win32.WindowPosFlags.SWP_NOSIZE | Win32.WindowPosFlags.SWP_NOZORDER));
                                if (hwdp != IntPtr.Zero)
                                    hwdp = Win32.DeferWindowPos(hwdp, this.Handle, this.Handle, posInfo.x, posInfo.y, posInfo.cx, posInfo.cy,
                                            (uint)(posInfo.flags | Win32.WindowPosFlags.SWP_NOZORDER));
                                if (hwdp != IntPtr.Zero)
                                    Win32.EndDeferWindowPos(hwdp);

                                m_layeredWnd.LayeredPos = new Point(posInfo.x + m_offX, posInfo.y + m_offY);

                                //Update the flags so that the form will not move with this message
                                posInfo.flags |= Win32.WindowPosFlags.SWP_NOMOVE;
                                Marshal.StructureToPtr(posInfo, m.LParam, true);
                            }

                            if ((posInfo.flags & Win32.WindowPosFlags.SWP_NOSIZE) == 0)
                            {
                                //Form was also resized
                                int diffX = posInfo.cx - this.Size.Width;
                                int diffY = posInfo.cy - this.Size.Height;
                                if (diffX != 0 || diffY != 0)
                                    this.updateLayeredSize(this.ClientSize.Width + diffX, this.ClientSize.Height + diffY);
                            }

                            UseBase = false;
                        }
                    }
                    break;


                case Win32.Message.WM_ACTIVATE:
                    {
                        //If WParam is Zero then the form is deactivating and we don't need to do anything
                        //Otherwise we need to make sure that the background form is positioned just behind
                        //the main form.
                        if (m.WParam != IntPtr.Zero)
                        {
                            //Check for any visible windows between the background and main windows
                            IntPtr prevWnd = Win32.GetWindow(m_layeredWnd.Handle, Win32.GetWindow_Cmd.GW_HWNDPREV);
                            while (prevWnd != IntPtr.Zero)
                            {
                                //If we find a visiable window, we stop
                                if (Win32.IsWindowVisible(prevWnd))
                                    break;

                                prevWnd = Win32.GetWindow(prevWnd, Win32.GetWindow_Cmd.GW_HWNDPREV);
                            }

                            //If the visible window isn't ours reset the position of the background form
                            if (prevWnd != this.Handle)
                                Win32.SetWindowPos(m_layeredWnd.Handle, this.Handle, 0, 0, 0, 0, (uint)(Win32.WindowPosFlags.SWP_NOSENDCHANGING | Win32.WindowPosFlags.SWP_NOACTIVATE | Win32.WindowPosFlags.SWP_NOSIZE | Win32.WindowPosFlags.SWP_NOMOVE));
                        }
                    }
                    break;
            }

            if (UseBase)
                base.WndProc(ref m);
        }

        /// <summary>
        /// Used to intercept windows messages for our background form so that we can send events
        /// back to the main form. Because the background form is Disabled (so that it cannot receive focus)
        /// we only actually get one message WM_SETCURSOR, which actually gives us all the information on
        /// what the mouse is doing at the time, so we can use that to fire off events on the main form that
        /// we are missing out on because its background is transparent.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int LayeredWindowWndProc(IntPtr hWnd, int Msg, int wParam, int lParam)
        {
            Point mousePos = this.PointToClient(System.Windows.Forms.Cursor.Position);
            Win32.Message msgId = (Win32.Message)Msg;
            switch (msgId)
            {
                case Win32.Message.WM_LBUTTONDOWN:
                    System.Diagnostics.Debugger.Break();

                    break;
                case Win32.Message.WM_SETCURSOR:
                    {
                        //Set the cursor, we need to do this ourselves because we are not letting this message through
                        Win32.SetCursor(Win32.LoadCursor(IntPtr.Zero, Win32.SystemCursor.IDC_NORMAL));


                        MouseEventArgs e = null;
                        delMouseEvent mouseEvent = null;
                        delStdEvent stdEvent = null;

                        //The low word of the lParam contains the hit test code, which we don't
                        //need to know, we only need to know what the mouse is doing
                        Win32.Message MouseEvent = (Win32.Message)(lParam >> 16);

                        switch (MouseEvent)
                        {
                            case Win32.Message.WM_MOUSEMOVE:
                                {
                                    if (m_isDown.Left && m_lockedPoint != mousePos)
                                    {
                                        //We are using the trick of sending the WM_NCLBUTTONDOWN message to make Windows drag our form
                                        //around, I'm not entirely certain how Windows works but our main form needs to have been the last
                                        //window with mouse capture for it to work, even thought it is necessary to ReleaseCapture prior
                                        //to sending the message
                                        Win32.SetCapture(this.Handle);
                                        m_moving = true;
                                    }
                                    else
                                    {
                                        e = new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, mousePos.X, mousePos.Y, 0);
                                        mouseEvent = new delMouseEvent(this.OnMouseMove);
                                    }
                                }
                                break;

                            case Win32.Message.WM_LBUTTONDOWN:
                                if (m_lastClickMsg == MouseEvent && !m_isDown.Left && dblClick(mousePos))
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 2, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDoubleClick);
                                    stdEvent = new delStdEvent(this.OnDoubleClick);

                                    m_lastClickMsg = 0;
                                }
                                else if (!m_isDown.Left)
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDown);
                                    m_lastClickMsg = MouseEvent;
                                }

                                m_clickTime = DateTime.Now;
                                m_lockedPoint = mousePos;

                                m_isDown.Left = true;
                                break;

                            case Win32.Message.WM_LBUTTONUP:
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseClick);
                                    stdEvent = new delStdEvent(this.OnClick);
                                    m_isDown.Left = false;
                                }
                                break;

                            case Win32.Message.WM_MBUTTONDOWN:
                                if (m_lastClickMsg == MouseEvent && !m_isDown.Middle && dblClick(mousePos))
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Middle, 2, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDoubleClick);
                                    stdEvent = new delStdEvent(this.OnDoubleClick);
                                    m_lastClickMsg = 0;
                                }
                                else if (!m_isDown.Middle)
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Middle, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDown);

                                    m_lastClickMsg = MouseEvent;
                                    m_clickTime = DateTime.Now;
                                    m_lockedPoint = mousePos;
                                }
                                m_isDown.Middle = true;
                                break;

                            case Win32.Message.WM_MBUTTONUP:
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Middle, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseClick);
                                    stdEvent = new delStdEvent(this.OnClick);

                                    m_isDown.Middle = false;
                                }
                                break;

                            case Win32.Message.WM_RBUTTONDOWN:
                                if (m_lastClickMsg == MouseEvent && !m_isDown.Right && dblClick(mousePos))
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Right, 2, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDoubleClick);
                                    stdEvent = new delStdEvent(this.OnDoubleClick);

                                    m_lastClickMsg = 0;
                                }
                                else if (!m_isDown.Right)
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Right, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDown);

                                    m_lastClickMsg = MouseEvent;
                                    m_clickTime = DateTime.Now;
                                    m_lockedPoint = mousePos;
                                }
                                m_isDown.Right = true;
                                break;

                            case Win32.Message.WM_RBUTTONUP:
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Right, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseClick);
                                    stdEvent = new delStdEvent(this.OnClick);

                                    m_isDown.Right = false;
                                }
                                break;

                            case Win32.Message.WM_XBUTTONDOWN:
                                if (m_lastClickMsg == MouseEvent && !m_isDown.XBtn && dblClick(mousePos))
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.XButton1, 2, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDoubleClick);
                                    stdEvent = new delStdEvent(this.OnDoubleClick);

                                    m_lastClickMsg = 0;
                                }
                                else if (!m_isDown.XBtn)
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.XButton1, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseDown);

                                    m_lastClickMsg = MouseEvent;
                                    m_clickTime = DateTime.Now;
                                    m_lockedPoint = mousePos;
                                }
                                m_isDown.XBtn = true;
                                break;

                            case Win32.Message.WM_XBUTTONUP:
                                {
                                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.XButton1, 1, mousePos.X, mousePos.Y, 0);
                                    mouseEvent = new delMouseEvent(this.OnMouseClick);
                                    stdEvent = new delStdEvent(this.OnClick);

                                    m_isDown.XBtn = false;
                                }
                                break;
                        }

                        //Check if the form is being clicked, but is not active
                        bool mouseDown = m_isDown.Left || m_isDown.Middle || m_isDown.Right || m_isDown.XBtn;
                        if (mouseDown && Form.ActiveForm == null)
                        {
                            //We need to give our form focus
                            this.Activate();
                        }

                        if (e != null)
                        {
                            if (mouseEvent != null)
                                this.BeginInvoke(mouseEvent, e);
                            if (stdEvent != null)
                                this.BeginInvoke(stdEvent, e);
                        }

                        return 0;
                    }

                }
            return Win32.CallWindowProc(m_layeredWindowProc, hWnd, Msg, wParam, lParam);
        }

        private Bitmap m_background;
		private Bitmap m_backgroundEx;
		private Bitmap m_backgroundFull;
		private bool m_useBackgroundEx;
		private LayeredWindow m_layeredWnd;
		private int m_offX;
		private int m_offY;
		private bool m_renderCtrlBG;
		private bool m_enhanced;
		private SizeModes m_sizeMode;
		private List<Control> m_hiddenControls;
		private Dictionary<Control, bool> m_controlDict;
		private bool m_moving;
		private bool m_initialised;

		private Win32.Win32WndProc m_customLayeredWindowProc;
		private IntPtr m_layeredWindowProc;

		//Mouse
		private Point m_lockedPoint = new Point();
		private DateTime m_clickTime = DateTime.Now;
		private Win32.Message m_lastClickMsg = 0;
		private HeldButtons m_isDown;

		//Event Delegates
		private delegate void delMouseEvent(MouseEventArgs e);
		private delegate void delStdEvent(EventArgs e);

		struct HeldButtons
		{
			public bool Left;
			public bool Middle;
			public bool Right;
			public bool XBtn;
		};
	}
}
