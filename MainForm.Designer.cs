using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataAnalysisApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnTestNetwork = new System.Windows.Forms.Button();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripTotalCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTotalCount = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button(); // 添加设置按钮
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // panelTop
            this.panelTop.Controls.Add(this.dtpStart);
            this.panelTop.Controls.Add(this.dtpEnd);
            this.panelTop.Controls.Add(this.lblTo);
            this.panelTop.Controls.Add(this.btnSearch);
            this.panelTop.Controls.Add(this.btnSave);
            this.panelTop.Controls.Add(this.btnTestNetwork);
            this.panelTop.Controls.Add(this.btnSettings); // 添加到面板
            this.panelTop.Controls.Add(this.lblTotalCount);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(884, 60);
            this.panelTop.TabIndex = 0;

            // dtpStart
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStart.Location = new System.Drawing.Point(70, 15); // 适当右移，避免和“全选”重叠
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(120, 20); // 宽度120，和dtpEnd一致
            this.dtpStart.TabIndex = 0;
            this.dtpStart.Value = DateTime.Now;

            // dtpEnd
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEnd.Location = new System.Drawing.Point(155, 20);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(120, 20);
            this.dtpEnd.TabIndex = 1;
            this.dtpEnd.Value = DateTime.Now;

            // lblTo（可选：调整垂直居中）
            this.lblTo.Location = new System.Drawing.Point(135, 19); // 微调使文字垂直居中

            // lblTo
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(135, 24);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(19, 13);
            this.lblTo.TabIndex = 2;
            this.lblTo.Text = "至";


            // btnSearch
            this.btnSearch.Location = new System.Drawing.Point(505, 18); // 295+200+10=505
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(100, 25);
            this.btnSearch.TabIndex = 4;
            this.btnSearch.Text = "查询";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 新增的代码：设置btnSearch样式
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.BackColor = Color.WhiteSmoke;
            btnSearch.ForeColor = Color.DodgerBlue;
            btnSearch.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnSearch.Height = 28;
            btnSearch.Region = System.Drawing.Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, btnSearch.Width, btnSearch.Height, 12, 12)
            );
            btnSearch.MouseEnter += (s, e) => btnSearch.BackColor = Color.Gainsboro;
            btnSearch.MouseLeave += (s, e) => btnSearch.BackColor = Color.WhiteSmoke;

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(615, 18);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 25);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "下载";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnTestNetwork
            this.btnTestNetwork.Location = new System.Drawing.Point(725, 18);
            this.btnTestNetwork.Name = "btnTestNetwork";
            this.btnTestNetwork.Size = new System.Drawing.Size(100, 25);
            this.btnTestNetwork.TabIndex = 6;
            this.btnTestNetwork.Text = "测试网络";
            this.btnTestNetwork.UseVisualStyleBackColor = true;
            this.btnTestNetwork.Click += new System.EventHandler(this.btnTestNetwork_Click);

            // btnSettings - 添加设置按钮
            this.btnSettings.Location = new System.Drawing.Point(835, 18);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(40, 25);
            this.btnSettings.TabIndex = 7;
            this.btnSettings.Text = "设置";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);

            // dataGridView
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(0, 60); // 调整位置，使其紧接面板下方
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.Size = new System.Drawing.Size(884, 501);
            this.dataGridView.TabIndex = 1;

            // lblTotalCount
            this.lblTotalCount.AutoSize = true;
            this.lblTotalCount.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalCount.Location = new System.Drawing.Point(650, 5); // 临时位置，会在Resize事件中调整
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(120, 20);
            this.lblTotalCount.TabIndex = 8;
            this.lblTotalCount.Text = "当前合同总数：0";

            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripTotalCount
            });
            this.statusStrip.Location = new System.Drawing.Point(0, 561);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(884, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";

            // toolStripStatusLabel
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel.Text = "就绪 | 数据已加载";

            // toolStripTotalCount
            this.toolStripTotalCount.Name = "toolStripTotalCount";
            this.toolStripTotalCount.Size = new System.Drawing.Size(120, 17);
            this.toolStripTotalCount.Text = "当前合同总数：0";

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 583);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.statusStrip);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "智行合同查询系统";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnTestNetwork;
        private System.Windows.Forms.Button btnSettings; // 添加设置按钮声明
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripTotalCount;
        private System.Windows.Forms.Label lblTotalCount;

        // NativeMethods.CreateRoundRectRgn 需引入 Win32 API
        internal class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }
    }
}
