namespace Scan2Buy
{
    partial class MainScreen
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainScreen));
            this.buttonPay = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.labelWarenkorb = new System.Windows.Forms.Label();
            this.buttonLogin = new System.Windows.Forms.Button();
            this.buttonScan = new System.Windows.Forms.Button();
            this.labelArtikelAnzahl = new System.Windows.Forms.Label();
            this.labelPreis = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonPay
            // 
            this.buttonPay.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPay.Location = new System.Drawing.Point(1176, 751);
            this.buttonPay.Name = "buttonPay";
            this.buttonPay.Size = new System.Drawing.Size(150, 98);
            this.buttonPay.TabIndex = 0;
            this.buttonPay.Text = "Pay";
            this.buttonPay.UseVisualStyleBackColor = true;
            this.buttonPay.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(99, 143);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(565, 625);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // labelWarenkorb
            // 
            this.labelWarenkorb.AutoSize = true;
            this.labelWarenkorb.BackColor = System.Drawing.Color.Transparent;
            this.labelWarenkorb.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWarenkorb.Location = new System.Drawing.Point(93, 83);
            this.labelWarenkorb.Name = "labelWarenkorb";
            this.labelWarenkorb.Size = new System.Drawing.Size(146, 31);
            this.labelWarenkorb.TabIndex = 2;
            this.labelWarenkorb.Text = "Warenkorb";
            this.labelWarenkorb.Click += new System.EventHandler(this.label1_Click);
            // 
            // buttonLogin
            // 
            this.buttonLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLogin.Location = new System.Drawing.Point(1458, 799);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(50, 50);
            this.buttonLogin.TabIndex = 3;
            this.buttonLogin.TabStop = false;
            this.buttonLogin.Text = "o/";
            this.buttonLogin.UseVisualStyleBackColor = true;
            this.buttonLogin.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonScan
            // 
            this.buttonScan.BackColor = System.Drawing.SystemColors.Control;
            this.buttonScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonScan.Location = new System.Drawing.Point(1004, 751);
            this.buttonScan.Name = "buttonScan";
            this.buttonScan.Size = new System.Drawing.Size(150, 98);
            this.buttonScan.TabIndex = 4;
            this.buttonScan.Text = "Scan";
            this.buttonScan.UseVisualStyleBackColor = false;
            // 
            // labelArtikelAnzahl
            // 
            this.labelArtikelAnzahl.AutoSize = true;
            this.labelArtikelAnzahl.BackColor = System.Drawing.Color.Transparent;
            this.labelArtikelAnzahl.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelArtikelAnzahl.Location = new System.Drawing.Point(927, 206);
            this.labelArtikelAnzahl.Name = "labelArtikelAnzahl";
            this.labelArtikelAnzahl.Size = new System.Drawing.Size(178, 31);
            this.labelArtikelAnzahl.TabIndex = 5;
            this.labelArtikelAnzahl.Text = "Artikelanzahl:";
            this.labelArtikelAnzahl.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // labelPreis
            // 
            this.labelPreis.AutoSize = true;
            this.labelPreis.BackColor = System.Drawing.Color.Transparent;
            this.labelPreis.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPreis.Location = new System.Drawing.Point(927, 259);
            this.labelPreis.Name = "labelPreis";
            this.labelPreis.Size = new System.Drawing.Size(84, 31);
            this.labelPreis.TabIndex = 6;
            this.labelPreis.Text = "Preis:";
            // 
            // MainScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1520, 861);
            this.Controls.Add(this.buttonScan);
            this.Controls.Add(this.buttonLogin);
            this.Controls.Add(this.labelWarenkorb);
            this.Controls.Add(this.labelArtikelAnzahl);
            this.Controls.Add(this.labelPreis);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonPay);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Name = "MainScreen";
            this.Text = "MainScreen";
            this.Load += new System.EventHandler(this.MainScreen_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonPay;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label labelWarenkorb;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.Button buttonScan;
        private System.Windows.Forms.Label labelArtikelAnzahl;
        private System.Windows.Forms.Label labelPreis;
    }
}