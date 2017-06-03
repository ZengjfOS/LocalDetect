namespace LocalDetect
{
    partial class localDetect
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(localDetect));
            this.label1 = new System.Windows.Forms.Label();
            this.udpPortValue = new System.Windows.Forms.TextBox();
            this.dataLV = new System.Windows.Forms.ListView();
            this.start = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tcpPortValue = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(413, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "UDP Port:";
            // 
            // udpPortValue
            // 
            this.udpPortValue.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.udpPortValue.Location = new System.Drawing.Point(415, 56);
            this.udpPortValue.Name = "udpPortValue";
            this.udpPortValue.Size = new System.Drawing.Size(67, 23);
            this.udpPortValue.TabIndex = 1;
            this.udpPortValue.Text = "50000";
            // 
            // dataLV
            // 
            this.dataLV.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataLV.Location = new System.Drawing.Point(12, 12);
            this.dataLV.Name = "dataLV";
            this.dataLV.Size = new System.Drawing.Size(390, 268);
            this.dataLV.TabIndex = 2;
            this.dataLV.UseCompatibleStateImageBehavior = false;
            // 
            // start
            // 
            this.start.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.start.Location = new System.Drawing.Point(411, 172);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(75, 67);
            this.start.TabIndex = 0;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(413, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 14);
            this.label2.TabIndex = 0;
            this.label2.Text = "TCP Port:";
            // 
            // tcpPortValue
            // 
            this.tcpPortValue.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tcpPortValue.Location = new System.Drawing.Point(415, 111);
            this.tcpPortValue.Name = "tcpPortValue";
            this.tcpPortValue.Size = new System.Drawing.Size(67, 23);
            this.tcpPortValue.TabIndex = 1;
            this.tcpPortValue.Text = "50002";
            // 
            // localDetect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 292);
            this.Controls.Add(this.start);
            this.Controls.Add(this.dataLV);
            this.Controls.Add(this.tcpPortValue);
            this.Controls.Add(this.udpPortValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "localDetect";
            this.Text = "LocalDetect";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox udpPortValue;
        private System.Windows.Forms.ListView dataLV;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tcpPortValue;
    }
}

