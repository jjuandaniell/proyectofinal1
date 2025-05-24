namespace Grok_API_Form
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            intext = new TextBox();
            btnenviar = new Button();
            btnword = new Button();
            btnpwpt = new Button();
            outtext = new RichTextBox();
            SuspendLayout();
            // 
            // intext
            // 
            intext.Location = new Point(12, 782);
            intext.Multiline = true;
            intext.Name = "intext";
            intext.Size = new Size(1281, 165);
            intext.TabIndex = 0;
            // 
            // btnenviar
            // 
            btnenviar.BackColor = SystemColors.GradientActiveCaption;
            btnenviar.Location = new Point(1299, 791);
            btnenviar.Name = "btnenviar";
            btnenviar.Size = new Size(204, 46);
            btnenviar.TabIndex = 1;
            btnenviar.Text = "enviar";
            btnenviar.UseVisualStyleBackColor = false;
            btnenviar.Click += btnenviar_Click;
            // 
            // btnword
            // 
            btnword.BackColor = SystemColors.MenuHighlight;
            btnword.Location = new Point(1299, 843);
            btnword.Name = "btnword";
            btnword.Size = new Size(204, 46);
            btnword.TabIndex = 2;
            btnword.Text = "generar_word";
            btnword.UseVisualStyleBackColor = false;
            btnword.Click += btnword_Click;
            // 
            // btnpwpt
            // 
            btnpwpt.BackColor = Color.DarkOrange;
            btnpwpt.Location = new Point(1299, 895);
            btnpwpt.Name = "btnpwpt";
            btnpwpt.Size = new Size(204, 46);
            btnpwpt.TabIndex = 3;
            btnpwpt.Text = "generar_pwpt";
            btnpwpt.UseVisualStyleBackColor = false;
            btnpwpt.Click += btnpwpt_Click;
            // 
            // outtext
            // 
            outtext.BackColor = SystemColors.Control;
            outtext.Location = new Point(-1, 12);
            outtext.Name = "outtext";
            outtext.Size = new Size(1491, 764);
            outtext.TabIndex = 4;
            outtext.Text = "";
            
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Menu;
            ClientSize = new Size(1515, 967);
            Controls.Add(outtext);
            Controls.Add(btnpwpt);
            Controls.Add(btnword);
            Controls.Add(btnenviar);
            Controls.Add(intext);
            Name = "Form1";
            Text = "OsunIA";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox intext;
        private Button btnenviar;
        private Button btnword;
        private Button btnpwpt;
        private RichTextBox outtext;
    }
}
