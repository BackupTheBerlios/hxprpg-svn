using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Drawing.Imaging;
using System.Text;

namespace terrain
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(0, 0);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(120, 40);
			this.button1.TabIndex = 0;
			this.button1.Text = "button1";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(0, 48);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox1.Size = new System.Drawing.Size(288, 216);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "textBox1";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(144, 8);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(88, 24);
			this.button2.TabIndex = 2;
			this.button2.Text = "button2";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
		
		private void button1_Click(object sender, System.EventArgs e) {
			
			//the heightmap
			Bitmap b = (Bitmap)Bitmap.FromFile(@"i:\dev\doom\heightmap.png");
			
			// the maximum height.  Increase for bumpier terrain
			float maxHeight = 200;

			// the x,y and z location of the first vertex
			float startx = 0;
			float starty = 0;
			float startz = 0;

			//the number of units between vertex's.
			float xspacing = 30;
			float yspacing = 30;

			//the number of sections along the x and y axis of each patch.  This must be an even number
			int patchWidth = 6, patchHeight = 6;
			
			//how many times to cut the mesh into smaller meshes, along x and y.
			//for example, cuts = 2 will cut the x and y axis twice, to produce 4 meshes
			//cuts = 20 will cut it 19 times, and produce 400 meshes
			//this variable is poorly named
			int cuts = 20;
			
			StringBuilder sb = new StringBuilder();
			
			float curx = 0, cury = 0;
			float curz = 0;
			float firstx = 0, firsty = 0;
			float heightsWidth, heightsHeight;
			int x = 0,y = 0;
			
			float[,] heights = new float[b.Width + 2,b.Height + 2];
			//store the heightmap.  This also adds a 0 height border around the map
			//so the edges all have a z of 0.  This makes it much easier to line up with the 
			//skybox.  This will have to be rethought though.
			for (x=0; x<b.Width; x++) {
				for (y=0; y<b.Height; y++) {
					heights[x + 1, y + 1] = b.GetPixel(x,y).GetBrightness();
				}
			}
			heightsWidth = heights.GetUpperBound(0);
			heightsHeight = heights.GetUpperBound(1);
			//process
			for (int piecey = 0; piecey<cuts; piecey++) {
				for (int piecex = 0; piecex < cuts; piecex++) {
					sb.Append(string.Format("{{\n patchDef2\n  {{\n  \"textures/rock/rock02\"\n" +
						"  ( {0} {1} 0 0 0 )\n  (\n", 
						patchWidth + 1,patchHeight + 1));
					
					firstx = startx + piecex * patchWidth * xspacing;
					firsty = starty + piecey * patchHeight * yspacing;
					for (int xl = 0 ; xl < patchWidth + 1; xl++) {	
						
						sb.Append("   (");
						for (int yl = 0; yl < patchHeight + 1; yl++) {	
							x = (int)Math.Round((((float)(piecex * patchWidth + xl) / (patchWidth * cuts )) * heightsWidth));
							y = (int)Math.Round((((float)(piecey * patchHeight + yl) / (patchHeight * cuts )) * heightsHeight));
					
							curx = firstx + (xl * xspacing);
							cury = firsty + (yl * yspacing);
							curz = startz + (maxHeight * heights[x,y]);
							sb.Append(string.Format(" ({0} {1} {2} {3} {4}) ",curx, cury, curz, (curx - firstx) / 32, (cury - firsty) / 32));
				
						}
						sb.Append(")");
						sb.Append(Environment.NewLine);
					}
					sb.Append("  )\n }\n}\n");
				}
			}
			sb.Append(string.Format("{{\n brushDef3\n {{\n" +
				"  ( 0 0 -1 {6} ) ( ( 0.015625 0 0 ) ( 0 0.0078125 0 ) ) \"{0}\" 0 0 0\n" +
				"  ( 0 0 1 {5} ) ( ( 0.015625 0 0 ) ( 0 0.0078125 0 ) ) \"{0}\" 0 0 0\n" +
				"  ( 0 -1 0 {2} ) ( ( 0.015625 0 0 ) ( 0 0.0078125 -0.1875 ) ) \"{0}\" 0 0 0\n" +
				"  ( 1 0 0 {3} ) ( ( 0.015625 0 0 ) ( 0 0.0078125 -0.1875 ) ) \"{0}\" 0 0 0\n" +
				"  ( 0 1 0 {4} ) ( ( 0.015625 0 0 ) ( 0 0.0078125 -0.1875 ) ) \"{0}\" 0 0 0\n" + 
				"  ( -1 0 0 {1} ) ( ( 0.015625 -0 0 ) ( 0 0.0078125 -0.1875 ) ) \"{0}\" 0 0 0\n" +
				" }}\n}}", "textures/rock/dirt03", startx, starty, -curx, -cury, -(startz), startz - 8));

			textBox1.Text = sb.ToString();
		}

		private void button2_Click(object sender, System.EventArgs e) {
			//select all the text, to make it easy to copy and paste
			textBox1.SelectAll();
			textBox1.Focus();
		}
	}
}
