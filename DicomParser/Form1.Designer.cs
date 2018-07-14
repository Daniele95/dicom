using CSharpGL;using Kitware.VTK;

namespace VolumeRendering.Raycast
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private RenderWindowControl renderWindowControl1;

        private void InitializeComponent()
        {
            if (!CSHARPGL)
            {
                // parte di activiz--------------------------------------------------------------------
                //all other controls added by the designer

                renderWindowControl1 = new RenderWindowControl();
                renderWindowControl1.SetBounds(0, 0, 640, 480);
                this.Controls.Add(this.renderWindowControl1);
                /// <summary>
                /// Required method for Designer support - do not modify
                /// the contents of this method with the code editor.
                /// </summary>
                this.SuspendLayout();
                // 
                // Form1
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(819, 477);
                this.Name = "Form1";
                this.Text = "Form1";
                this.Load += new System.EventHandler(this.Form1_Load);
                this.ResumeLayout(false);
            }
            else if (CSHARPGL)
            {

                // parte di CSHARPGL - scommentare per volumetrica
                // parte di CSharpGL--------------------------------------------------------------------
                this.winGLCanvas1 = new CSharpGL.WinGLCanvas();
                ((System.ComponentModel.ISupportInitialize)(this.winGLCanvas1)).BeginInit();
                this.SuspendLayout();
                // 
                // winGLCanvas1
                // 
                this.winGLCanvas1.AccumAlphaBits = ((byte)(0));
                this.winGLCanvas1.AccumBits = ((byte)(0));
                this.winGLCanvas1.AccumBlueBits = ((byte)(0));
                this.winGLCanvas1.AccumGreenBits = ((byte)(0));
                this.winGLCanvas1.AccumRedBits = ((byte)(0));
                this.winGLCanvas1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.winGLCanvas1.Location = new System.Drawing.Point(12, 12);
                this.winGLCanvas1.Name = "winGLCanvas1";
                this.winGLCanvas1.RenderTrigger = CSharpGL.RenderTrigger.Manual;
                this.winGLCanvas1.Size = new System.Drawing.Size(961, 553);
                this.winGLCanvas1.StencilBits = ((byte)(0));
                this.winGLCanvas1.TabIndex = 0;
                this.winGLCanvas1.TimerTriggerInterval = 40;
                this.winGLCanvas1.UpdateContextVersion = true;
                // 
                // FormMain
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(985, 577);
                this.Controls.Add(this.winGLCanvas1);
                this.Name = "FormMain";
                this.Text = "Raycast Volume Rendering - CSharpGL";
                ((System.ComponentModel.ISupportInitialize)(this.winGLCanvas1)).EndInit();
                this.ResumeLayout(false);
            }


        }


        private CSharpGL.WinGLCanvas winGLCanvas1;
        #region Windows Form Designer generated code

    

        #endregion
    }
}