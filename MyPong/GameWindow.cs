using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MyPong {
	/// <summary>
	/// Description of GameWindow.
	/// </summary>
	public sealed class GameWindow : Form {
		
		const int kPadHeight = 64;
		const int kHalfPadHeight = kPadHeight / 2;
		const float kStartLevelSpeed = 500;
		const float kLevelSpeedIncrese = 100;
		const float kInitRightPadSpeed = 300;
		
		int left_pad_y_;
		float right_pad_y_;
		float ball_x_;
		float ball_y_;
		float ball_speed_x_;
		float ball_speed_y_;
		
		float level_speed_;
		
		readonly int limit_top_;
		readonly int limit_bottom_;
		
		float right_pad_max_speed_;
		
		StringFormat left_score_format_;
		StringFormat right_score_format_;
		
		int left_score_;
		int right_score_;
		
		bool serve_side_;
		int round_;
		
		Stopwatch timer_;
		
		bool mouse_resetting_;
		
		bool cursor_locked_;
		
		int mouse_offset_y_;
		
		string status_;
		
		SolidBrush ui_bg_brush_;
		Font title_font_;
		
		double last_elapsed_time_;
		
		Action<int, int> beep_action_;
		
		int last_left_pad_sample_;
		int last_right_pad_sample_;
		
		public GameWindow() {
			this.Text = "My Pong";
			
			var random_ = new Random();
			if (random_.Next(0, 99) < 1)
				this.Text = "My Pony";
			
			this.ClientSize = new Size(640, 480);
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.StartPosition = FormStartPosition.CenterScreen;
			
			this.Font = new Font("Tahomas", 24, GraphicsUnit.Pixel);
			
			this.DoubleBuffered = true;
			
			left_pad_y_ = 0;
			right_pad_y_ = 0;
			ball_x_ = 0;
			ball_y_ = 0;
			ball_speed_x_ = 0;
			ball_speed_y_ = 0;
			
			level_speed_ = kStartLevelSpeed;
			
			left_score_format_ = StringFormat.GenericTypographic.Clone() as StringFormat;
			left_score_format_.Alignment = StringAlignment.Far;
			right_score_format_ = StringFormat.GenericTypographic.Clone() as StringFormat;
			right_score_format_.Alignment = StringAlignment.Near;
			
			left_score_ = 0;
			right_score_ = 0;
			
			timer_ = new Stopwatch();
			
			mouse_resetting_ = true;
			
			
			limit_top_ = kHalfPadHeight - 240;
			limit_bottom_ = 480 - kHalfPadHeight - 240;
			
			status_ = "title";
			
			cursor_locked_ = false;
			
			ui_bg_brush_ = new SolidBrush(Color.FromArgb(192, 0, 0, 0));
			title_font_ = new Font("Tahomas", 96, GraphicsUnit.Pixel);
			
			last_elapsed_time_ = 0.0;
			
			right_pad_max_speed_ = kInitRightPadSpeed;
			
			beep_action_ = Console.Beep;
			
			serve_side_ = false;
			round_ = 0;
		}
		
		void LockCursor() {
			Point client_pos = PointToScreen(new Point(0, 0));
			Cursor.Clip = new Rectangle(client_pos.X, client_pos.Y, 640, 480);
			cursor_locked_ = true;
			mouse_resetting_ = true;
			Cursor.Hide();
		}
		
		void UnlockCursor() {
			Cursor.Clip = Rectangle.Empty;
			Cursor.Show();
			cursor_locked_ = false;
		}
		
		void PlayBeep(int frequency, int duration) {
			beep_action_.BeginInvoke(frequency, duration, a => beep_action_.EndInvoke(a), null);
		}
		
		void PlayBouncePadSound() {
			PlayBeep(698, 100);
		}
		
		void PlayBounceSound() {
			PlayBeep(523, 100);
		}
		
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			timer_.Stop();
			last_elapsed_time_ = timer_.Elapsed.TotalSeconds;
			timer_.Reset();
			timer_.Start();
			Render(e.Graphics);
			this.Invalidate();
		}
		
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (!Focused)
				return;
			if (!cursor_locked_)
				return;
			if (status_ != "playing")
				return;
			if (mouse_resetting_) {
				Cursor.Position = PointToScreen(new Point(320, 240));
				mouse_resetting_ = false;
				mouse_offset_y_ = 0;
				return;
			}
			mouse_offset_y_ = e.Y - 240;
			mouse_resetting_ = true;
			
			left_pad_y_ += mouse_offset_y_;
			
			if (left_pad_y_ < limit_top_)
				left_pad_y_ = limit_top_;
			if (left_pad_y_ > limit_bottom_)
				left_pad_y_ = limit_bottom_;
		}
		
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button != MouseButtons.Left)
				return;
			
			if (status_ != "waiting" && status_ != "title" && status_ != "winning" && status_ != "gameover")
				return;
			
			level_speed_ = kStartLevelSpeed;
			
			var rnd = new Random();
			var serve_angle = rnd.Next(0, 90);
			if (status_ == "title") {
				// pick a side
				serve_side_ = rnd.NextDouble() < 0.5;
			}
			
			if (serve_side_) {
				serve_angle -= 45;
			}
			else {
				serve_angle += 135;
			}
			
			var serve_rad = serve_angle * Math.PI / 180.0;
			
			ball_speed_x_ = (float)Math.Cos(serve_rad);
			ball_speed_y_ = (float)Math.Sin(serve_rad);
			
			if (status_ == "winning" || status_ == "gameover") {
				left_score_ = 0;
				right_score_ = 0;
				status_ = "title";
				return;
			}
			
			LockCursor();
			
			status_ = "playing";
		}
		
		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			if (e.KeyCode == Keys.Escape) {
				Close();
				return;
			}
		}
		
		protected override void OnLostFocus(System.EventArgs e) {
			base.OnLostFocus(e);
			if (!cursor_locked_)
				return;
			Cursor.Show();
		}
		
		protected override void OnGotFocus(System.EventArgs e) {
			base.OnGotFocus(e);
			if (!cursor_locked_)
				return;
			Cursor.Hide();
		}
		
		int ClientYToCentralY(int clientY) {
			return clientY - 320;
		}
		
		void ProcessAI() {
			if (Math.Abs(right_pad_y_ - ball_y_) < float.Epsilon)
				return;
			var delta = right_pad_y_ - ball_y_;
			if (delta > 0) {
				right_pad_y_ -= (float)(right_pad_max_speed_ * last_elapsed_time_);
			
				if (right_pad_y_ < limit_top_)
					right_pad_y_ = limit_top_;
				return;
			}
			else {
				right_pad_y_ += (float)(right_pad_max_speed_ * last_elapsed_time_);
				
				if (right_pad_y_ > limit_bottom_)
					right_pad_y_ = limit_bottom_;
				return;
			}
		}
		
		void ProcessBouce() {
			var left_pad_delta = (left_pad_y_ - last_left_pad_sample_) / 10;
			var right_pad_delta = (right_pad_y_ - last_right_pad_sample_) / 10;
			last_left_pad_sample_ = left_pad_y_;
			last_right_pad_sample_ = (int)right_pad_y_;
			
			if (ball_x_ < -264) {
				if (ball_x_ < -340) {
					right_score_ ++;
					round_ ++;
					if (round_ % 2 == 0) {
						serve_side_ = !serve_side_;
					}
					if (right_score_ >= 11) {
						// game over
						status_ = "gameover";
						UnlockCursor();
						return;
					}
					ball_x_ = 0;
					ball_y_ = 0;
					status_ = "waiting";
					UnlockCursor();
					left_pad_y_ = 0;
					right_pad_y_ = 0;
					return;
				}
				if (ball_x_ > -280 && Math.Abs(left_pad_y_ - ball_y_) < kHalfPadHeight) {
					ball_x_ = -264;
					PlayBouncePadSound();
					ball_speed_x_ = -ball_speed_x_;
					ball_speed_y_ = Math.Min(2, Math.Max(ball_speed_y_ + left_pad_delta, -2));
				}
			}
			if (ball_x_ > 264) {
				if (ball_x_ > 340) {
					left_score_ ++;
					round_ ++;
					if (round_ % 2 == 0) {
						serve_side_ = !serve_side_;
					}
					if (left_score_ >= 11) {
						// game over
						status_ = "winning";
						UnlockCursor();
						return;
					}
					ball_x_ = 0;
					ball_y_ = 0;
					status_ = "waiting";
					UnlockCursor();
					left_pad_y_ = 0;
					right_pad_y_ = 0;
					return;
				}
				if (ball_x_ < 280 && Math.Abs(right_pad_y_ - ball_y_) < kHalfPadHeight) {
					ball_x_ = 264;
					PlayBouncePadSound();
					ball_speed_x_ = -ball_speed_x_;
					ball_speed_y_ =Math.Min(2, Math.Max(ball_speed_y_ + right_pad_delta, -2));
				}
			}
			if (ball_y_ < -232) {
				ball_y_ = -232;
				PlayBounceSound();
				ball_speed_y_ = -ball_speed_y_;
			}
			if (ball_y_ > 232) {
				ball_y_ = 232;
				PlayBounceSound();
				ball_speed_y_ = -ball_speed_y_;
			}
		}
		
		public void Render(Graphics g) {
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			g.Clear(Color.Black);
			
			if (status_ == "playing") {
				ball_x_ += (float)(last_elapsed_time_ * level_speed_ * ball_speed_x_);
				ball_y_ += (float)(last_elapsed_time_ * level_speed_ * ball_speed_y_);
				
				ProcessAI();
				ProcessBouce();
			}
			
			#region mid line
			g.FillRectangle(Brushes.Gray, 319, 0, 2, 480);
			#endregion
			
			#region left pad
			g.FillRectangle(Brushes.White, 32, left_pad_y_ + 240 - kPadHeight / 2, 16, kPadHeight);
			#endregion
			
			#region right pad
			g.FillRectangle(Brushes.White, 592, right_pad_y_ + 240 - kPadHeight / 2, 16, kPadHeight);
			#endregion
			
			#region ball
			g.FillRectangle(Brushes.White, 320 + ball_x_ - 8, 240 + ball_y_ - 8, 16, 16);
			#endregion
			
			#region left score
			g.DrawString(left_score_.ToString(), Font, Brushes.Black, new Rectangle(-1, 16, 304, 32), left_score_format_);
			g.DrawString(left_score_.ToString(), Font, Brushes.Black, new Rectangle(0, 15, 304, 32), left_score_format_);
			g.DrawString(left_score_.ToString(), Font, Brushes.Black, new Rectangle(1, 16, 304, 32), left_score_format_);
			g.DrawString(left_score_.ToString(), Font, Brushes.Black, new Rectangle(0, 17, 304, 32), left_score_format_);
			g.DrawString(left_score_.ToString(), Font, Brushes.White, new Rectangle(0, 16, 304, 32), left_score_format_);
			#endregion
			
			#region right score
			g.DrawString(right_score_.ToString(), Font, Brushes.Black, new Rectangle(335, 16, 304, 32), right_score_format_);
			g.DrawString(right_score_.ToString(), Font, Brushes.Black, new Rectangle(336, 15, 304, 32), right_score_format_);
			g.DrawString(right_score_.ToString(), Font, Brushes.Black, new Rectangle(337, 16, 304, 32), right_score_format_);
			g.DrawString(right_score_.ToString(), Font, Brushes.Black, new Rectangle(336, 17, 304, 32), right_score_format_);
			g.DrawString(right_score_.ToString(), Font, Brushes.White, new Rectangle(336, 16, 304, 32), right_score_format_);
			#endregion
			
			#region	title
			if (status_ == "title") {
				g.FillRectangle(ui_bg_brush_, 0, 0, 640, 480);
				g.DrawString("MY PONG", title_font_, Brushes.White, 32, 32);
			}
			#endregion
			
			#region game over
			if (status_ == "gameover") {
				g.FillRectangle(ui_bg_brush_, 0, 0, 640, 480);
				g.DrawString("GAME\nOVER", title_font_, Brushes.White, 32, 32);
			}
			#endregion
			
			#region game over
			if (status_ == "winning") {
				g.FillRectangle(ui_bg_brush_, 0, 0, 640, 480);
				g.DrawString("YOU WIN !", title_font_, Brushes.White, 32, 32);
			}
			#endregion
			
			if (status_ != "playing") {
				g.DrawString("Press <LEFT MOUSE BUTTON> to continue ...", Font, Brushes.White, 32, 416);
			}
		}
	}
}
