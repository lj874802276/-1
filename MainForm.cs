using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Runtime.InteropServices;

namespace DataAnalysisApp
{
    public partial class MainForm : Form
    {
        private DataTable dataTable = new DataTable();
        private string sharedFolderPath = @"\\Healwis-ftp\电子版合同"; // 默认路径
        private string configFilePath = Path.Combine(Application.StartupPath, "config.txt");
        private DataTable allFilesTable = null; // 用于保存所有文件数据

        //private Panel panelSearch; // 搜索框容器
        private CheckBox chkSelectAll; // 全选/全不选

        private TextBox txtSearch; // 搜索框

        // 分页相关字段
        private int pageSize = 50; //默认每页显示50条，支持修改15、50、100、全部
        private int pageIndex = 1;
        private int pageCount = 1;
        private DataTable pagedTable = null;

        public MainForm()
        {
            InitializeComponent();

            // 统一风格
            panelTop.BackColor = Color.FromArgb(245, 245, 247);
            panelTop.Height = 50;
            panelTop.Dock = DockStyle.Top;

            // 控件统一高度和字体
            int elementHeight = 28;
            Font font = new Font("微软雅黑", 10F);

            dtpStart.Height = elementHeight;
            dtpStart.Font = font;
            dtpEnd.Height = elementHeight;
            dtpEnd.Font = font;

            btnSearch.Height = elementHeight;
            btnSearch.Width = 70;
            btnSearch.Font = font;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.BackColor = Color.White;
            btnSearch.ForeColor = Color.DodgerBlue;

            btnSave.Height = elementHeight;
            btnSave.Width = 70;
            btnSave.Font = font;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.BackColor = Color.White;
            btnSave.ForeColor = Color.DodgerBlue;

            btnTestNetwork.Height = elementHeight;
            btnTestNetwork.Width = 90;
            btnTestNetwork.Font = font;
            btnTestNetwork.FlatStyle = FlatStyle.Flat;
            btnTestNetwork.FlatAppearance.BorderSize = 0;
            btnTestNetwork.BackColor = Color.White;
            btnTestNetwork.ForeColor = Color.DodgerBlue;

            btnSettings.Height = elementHeight;
            btnSettings.Width = 60;
            btnSettings.Font = font;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.BackColor = Color.White;
            btnSettings.ForeColor = Color.DodgerBlue;

            lblTo.Font = font;
            lblTotalCount.Font = new Font("微软雅黑", 10F, FontStyle.Bold);

            chkSelectAll = new CheckBox();
            chkSelectAll.Font = font;
            chkSelectAll.AutoSize = true;
            chkSelectAll.Text = "全选";

            // 事件绑定
            // 1. 初始化 txtSearch
            txtSearch = new TextBox();
            txtSearch.Width = 220;
            txtSearch.Height = elementHeight;
            txtSearch.Font = font;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Location = new Point(0, 0);
            txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            txtSearch.Margin = new Padding(0);

            // 2. 绑定事件
            txtSearch.KeyDown += TxtSearch_KeyDown;
            txtSearch.GotFocus += RemovePlaceholder;
            txtSearch.LostFocus += SetPlaceholder;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // 3. 其他控件初始化和事件绑定
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
            dataGridView.CellContentClick += DataGridView_CellContentClick;
            chkSelectAll.CheckedChanged += chkSelectAll_CheckedChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            // 默认全屏
            this.WindowState = FormWindowState.Maximized;

            // 加载配置和数据
            LoadConfig();
            InitializeData();
            this.Resize += MainForm_Resize;
            this.Load += MainForm_Load;

            // 添加到 panelTop
            panelTop.Controls.Add(txtSearch);
            panelTop.Controls.Add(chkSelectAll);
        }
        
        // 加载配置文件
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    string[] lines = File.ReadAllLines(configFilePath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("SharedFolderPath="))
                        {
                            string path = line.Substring("SharedFolderPath=".Length);
                            if (!string.IsNullOrEmpty(path))
                            {
                                sharedFolderPath = path;
                            }
                        }
                    }
                }
                else
                {
                    // 创建默认配置文件
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 保存配置文件
        private void SaveConfig()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(configFilePath, false))
                {
                    sw.WriteLine("SharedFolderPath=" + sharedFolderPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 设置日期选择器默认值为近一年
            dtpEnd.Value = DateTime.Today;
            dtpStart.Value = DateTime.Today.AddYears(-1);

            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "请输入关键字";
            txtSearch.GotFocus += RemovePlaceholder;
            txtSearch.LostFocus += SetPlaceholder;
            txtSearch.TextChanged += TxtSearch_TextChanged; // 实时过滤
            LoadSharedFolderFiles();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // 只有不是灰色提示时才过滤
            if (txtSearch.ForeColor != Color.Gray)
                ApplyFilter();
        }

        private void RemovePlaceholder(object sender, EventArgs e)
        {
            if (txtSearch.ForeColor == Color.Gray)
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void SetPlaceholder(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.ForeColor = Color.Gray;
                txtSearch.Text = "请输入关键字";
            }
        }

        private void InitializeData()
        {
            // 创建示例数据
            dataTable.Columns.Add("日期", typeof(DateTime));
            dataTable.Columns.Add("项目", typeof(string));
            dataTable.Columns.Add("数值", typeof(int));

            Random rand = new Random();
            for (int i = 0; i < 30; i++)
            {
                dataTable.Rows.Add(
                    DateTime.Today.AddDays(-30 + i),
                    string.Format("项目{0}", rand.Next(1, 10)),
                    rand.Next(100, 1000)
                );
            }

            dataGridView.DataSource = dataTable;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 获取所有勾选的行
            var checkedRows = dataGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r =>
                {
                    try
                    {
                        return Convert.ToBoolean(r.Cells["选择"].Value);
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToList();

            if (checkedRows.Count == 0)
            {
                MessageBox.Show("请选择要下载的文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "请选择下载保存的文件夹";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    int successCount = 0, failCount = 0;
                    foreach (DataGridViewRow row in checkedRows)
                    {
                        string fileName = row.Cells["合同名称"].Value?.ToString();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            string sourcePath = Path.Combine(sharedFolderPath, fileName);
                            string destPath = Path.Combine(fbd.SelectedPath, fileName);
                            try
                            {
                                if (File.Exists(sourcePath))
                                {
                                    File.Copy(sourcePath, destPath, true);
                                    successCount++;
                                }
                                else
                                {
                                    failCount++;
                                }
                            }
                            catch
                            {
                                failCount++;
                            }
                        }
                    }
                    MessageBox.Show(string.Format("下载完成！成功：{0}，失败：{1}", successCount, failCount), "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void SaveToCsv(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // 写入列标题
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    sw.Write(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1) sw.Write(",");
                }
                sw.WriteLine();

                // 写入数据
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        var value = row[i];
                        string output = value.ToString();

                        // 处理包含逗号或引号的值
                        if (output.Contains(",") || output.Contains("\""))
                        {
                            output = "\"" + output.Replace("\"", "\"\"") + "\"";
                        }

                        sw.Write(output);
                        if (i < dataTable.Columns.Count - 1) sw.Write(",");
                    }
                    sw.WriteLine();
                }
            }
        }

        // 用于连接网络共享的方法
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        [StructLayout(LayoutKind.Sequential)]
        private class NetResource
        {
            public int Scope;
            public int Type;
            public int DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        // 连接到网络共享
        private bool ConnectToShare(string remotePath, string username, string password)
        {
            NetResource netResource = new NetResource
            {
                Scope = 2, // RESOURCE_GLOBALNET
                Type = 1,  // RESOURCETYPE_DISK
                DisplayType = 3, // RESOURCEDISPLAYTYPE_SHARE
                RemoteName = remotePath
            };

            int result = WNetAddConnection2(netResource, password, username, 0);
            return result == 0; // 0表示成功
        }

        // 显示登录对话框
        private bool ShowLoginDialog()
        {
            // 创建一个更美观的登录表单
            Form loginForm = new Form
            {
                Width = 350,
                Height = 220,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "网络共享登录",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = true,
                ShowInTaskbar = false,
                Font = new Font("微软雅黑", 9F, FontStyle.Regular)
            };

            // 添加说明标签
            Label lblInfo = new Label
            {
                Text = "请输入访问网络共享的用户名和密码",
                Left = 30,
                Top = 20,
                Width = 290,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 用户名标签和文本框
            Label lblUsername = new Label
            {
                Text = "用户名:",
                Left = 30,
                Top = 60,
                Width = 70,
                TextAlign = ContentAlignment.MiddleRight
            };
        
            TextBox txtUsername = new TextBox
            {
                Left = 110,
                Top = 60,
                Width = 200,
                Height = 25,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 密码标签和文本框
            Label lblPassword = new Label
            {
                Text = "密码:",
                Left = 30,
                Top = 95,
                Width = 70,
                TextAlign = ContentAlignment.MiddleRight
            };
        
            TextBox txtPassword = new TextBox
            {
                Left = 110,
                Top = 95,
                Width = 200,
                Height = 25,
                PasswordChar = '●',
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // 按钮面板
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            // 确定按钮
            Button btnOk = new Button
            {
                Text = "确定",
                Width = 80,
                Height = 30,
                Left = (buttonPanel.Width - 170) / 2,
                Top = 10,
                DialogResult = DialogResult.OK,
                BackColor = SystemColors.Control
            };

            // 取消按钮
            Button btnCancel = new Button
            {
                Text = "取消",
                Width = 80,
                Height = 30,
                Left = btnOk.Right + 10,
                Top = 10,
                DialogResult = DialogResult.Cancel,
                BackColor = SystemColors.Control
            };

            // 添加控件到表单
            buttonPanel.Controls.Add(btnOk);
            buttonPanel.Controls.Add(btnCancel);
        
            loginForm.Controls.Add(lblInfo);
            loginForm.Controls.Add(lblUsername);
            loginForm.Controls.Add(txtUsername);
            loginForm.Controls.Add(lblPassword);
            loginForm.Controls.Add(txtPassword);
            loginForm.Controls.Add(buttonPanel);

            // 设置默认按钮和取消按钮
            loginForm.AcceptButton = btnOk;
            loginForm.CancelButton = btnCancel;
        
            // 设置初始焦点
            loginForm.Shown += (s, e) => txtUsername.Focus();

            // 显示对话框并处理结果
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                string username = txtUsername.Text;
                string password = txtPassword.Text;

                // 尝试连接到共享
                if (ConnectToShare(sharedFolderPath, username, password))
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("用户名或密码不正确，无法连接到网络共享。", "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return false;
        }

        private void LoadSharedFolderFiles()
        {
            DataTable table = new DataTable();
            table.Columns.Add("选择", typeof(bool)); // 复选框列
            table.Columns.Add("序号", typeof(int));
            table.Columns.Add("合同名称", typeof(string));
            table.Columns.Add("修改时间", typeof(DateTime));

            try
            {
                // 检查是否可以访问共享文件夹
                if (!Directory.Exists(sharedFolderPath))
                {
                    // 显示登录对话框
                    if (!ShowLoginDialog())
                    {
                        toolStripStatusLabel.ForeColor = Color.Red;
                        toolStripStatusLabel.Text = "无法访问网络共享，请检查网络连接或用户凭据。";
                        lblTotalCount.Text = "当前合同总数：0";
                        toolStripTotalCount.Text = "当前合同总数：0";
                        return;
                    }
                }

                var files = new DirectoryInfo(sharedFolderPath).GetFiles();
                int i = 1;
                foreach (var file in files.OrderByDescending(f => f.LastWriteTime))
                {
                    table.Rows.Add(false, i++, file.Name, file.LastWriteTime); // 默认未选中
                }
                allFilesTable = table.Copy();
                dataGridView.DataSource = table;

                // 设置列宽和样式
                if (dataGridView.Columns["选择"] != null)
                {
                    var col = dataGridView.Columns["选择"] as DataGridViewCheckBoxColumn;
                    col.Width = 40;
                    col.HeaderText = "";
                    col.TrueValue = true;
                    col.FalseValue = false;
                    col.IndeterminateValue = false;
                    col.DefaultCellStyle.NullValue = false;
                    col.ReadOnly = false;  // 确保复选框可编辑
                    dataGridView.EditMode = DataGridViewEditMode.EditOnEnter; // 允许直接编辑
                }
                if (dataGridView.Columns["序号"] != null)
                {
                    dataGridView.Columns["序号"].Width = 60;
                    dataGridView.Columns["序号"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                // 支持排序
                if (dataGridView.Columns["合同名称"] != null)
                    dataGridView.Columns["合同名称"].SortMode = DataGridViewColumnSortMode.Automatic;
                if (dataGridView.Columns["修改时间"] != null)
                    dataGridView.Columns["修改时间"].SortMode = DataGridViewColumnSortMode.Automatic;

                // 列宽自适应
                AutoFitDataGridViewColumns();

                // 美化DataGridView
                dataGridView.EnableHeadersVisualStyles = false;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
                dataGridView.DefaultCellStyle.SelectionBackColor = Color.LightSkyBlue;
                dataGridView.DefaultCellStyle.SelectionForeColor = Color.Black;
                dataGridView.RowHeadersVisible = false;

                toolStripStatusLabel.ForeColor = Color.Black;
                toolStripStatusLabel.Text = string.Format("文件加载成功 | 当前合同总数：{0}", table.Rows.Count);
                lblTotalCount.Text = string.Format("当前合同总数：{0}", table.Rows.Count);
                toolStripTotalCount.Text = string.Format("当前合同总数：{0}", table.Rows.Count);
            }
            catch (Exception ex)
            {
                table.Rows.Clear();
                dataGridView.DataSource = table;
                toolStripStatusLabel.ForeColor = Color.Black;
                toolStripStatusLabel.Text = string.Format("文件加载失败：{0}", ex.Message);
                lblTotalCount.Text = "当前合同总数：0";
            }

            // 分页
            allFilesTable = table;
            pageIndex = 1;
            RefreshPagedData();
        }

        // 分页刷新方法
        private void RefreshPagedData()
        {
            if (allFilesTable == null) return;
            int total = allFilesTable.Rows.Count;
            pageCount = (pageSize == int.MaxValue) ? 1 : (int)Math.Ceiling(total * 1.0 / pageSize);
            if (pageIndex < 1) pageIndex = 1;
            if (pageIndex > pageCount) pageIndex = pageCount;

            DataTable dt = allFilesTable.Clone();
            if (pageSize == int.MaxValue)
            {
                foreach (DataRow row in allFilesTable.Rows)
                    dt.ImportRow(row);
            }
            else
            {
                int start = (pageIndex - 1) * pageSize;
                int end = Math.Min(start + pageSize, total);
                for (int i = start; i < end; i++)
                    dt.ImportRow(allFilesTable.Rows[i]);
            }
            pagedTable = dt;
            dataGridView.DataSource = pagedTable;
            toolStripPageInfo.Text = $"第{pageIndex}/{pageCount}页";
        }

        // 上一页
        private void toolStripPrev_Click(object sender, EventArgs e)
        {
            if (pageIndex > 1)
            {
                pageIndex--;
                RefreshPagedData();
            }
        }

        // 下一页
        private void toolStripNext_Click(object sender, EventArgs e)
        {
            if (pageIndex < pageCount)
            {
                pageIndex++;
                RefreshPagedData();
            }
        }

        // 每页条数变更
        private void toolStripPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sel = toolStripPageSize.SelectedItem.ToString();
            if (sel == "全部")
                pageSize = int.MaxValue;
            else
                pageSize = int.Parse(sel);
            pageIndex = 1;
            RefreshPagedData();
        }

        private void ApplyFilter()
        {
            if (allFilesTable == null)
                return;

            string keyword = txtSearch.Text.Trim();
            DateTime startDate = dtpStart.Value.Date;
            DateTime endDate = dtpEnd.Value.Date.AddDays(1).AddTicks(-1);

            DataView dv = allFilesTable.DefaultView;
            string filter = string.Format("[修改时间] >= #{0:yyyy-MM-dd HH:mm:ss}# AND [修改时间] <= #{1:yyyy-MM-dd HH:mm:ss}#", startDate, endDate);

            if (!string.IsNullOrEmpty(keyword) && txtSearch.ForeColor != Color.Gray)
            {
                filter += string.Format(" AND [合同名称] LIKE '%{0}%'", keyword.Replace("'", "''")); // 关键字过滤
            }

            dv.RowFilter = filter;
            dataGridView.DataSource = dv;

            // 设置“序号”列宽度
            if (dataGridView.Columns["序号"] != null)
            {
                dataGridView.Columns["序号"].Width = 50;
            }

            // 支持排序
            if (dataGridView.Columns["合同名称"] != null)
                dataGridView.Columns["合同名称"].SortMode = DataGridViewColumnSortMode.Automatic;
            if (dataGridView.Columns["修改时间"] != null)
                dataGridView.Columns["修改时间"].SortMode = DataGridViewColumnSortMode.Automatic;

            // 列宽自适应
            AutoFitDataGridViewColumns();

            // 更新总数
            int count = dataGridView.Rows.Count;
            lblTotalCount.Text = string.Format("当前合同总数：{0}", count);
            toolStripTotalCount.Text = string.Format("当前合同总数：{0}", count);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (dtpStart == null || dtpEnd == null || txtSearch == null ||
                btnSearch == null || btnSave == null || btnTestNetwork == null ||
                btnSettings == null || panelTop == null || chkSelectAll == null)
            {
                return;
            }

            int margin = 16;
            int spacing = 8;
            int top = 10;
            int left = margin;

            // 全选
            chkSelectAll.Left = left;
            chkSelectAll.Top = top + 2;
            left = chkSelectAll.Right + spacing;

            // 日期
            dtpStart.Left = left;
            dtpStart.Top = top;
            left = dtpStart.Right + spacing;

            lblTo.Left = left;
            lblTo.Top = top + 5;
            left = lblTo.Right + spacing;

            dtpEnd.Left = left;
            dtpEnd.Top = top;
            left = dtpEnd.Right + spacing;

            // 搜索框
            txtSearch.Left = left;
            txtSearch.Top = top;
            left = txtSearch.Right + spacing;

            // 查询按钮
            btnSearch.Left = left;
            btnSearch.Top = top;
            left = btnSearch.Right + spacing;

            // 下载按钮
            btnSave.Left = left;
            btnSave.Top = top;
            left = btnSave.Right + spacing;

            // 测试网络
            btnTestNetwork.Left = left;
            btnTestNetwork.Top = top;
            left = btnTestNetwork.Right + spacing;

            // 设置按钮
            btnSettings.Left = left;
            btnSettings.Top = top;

            // 合同总数右对齐
            if (lblTotalCount != null)
            {
                lblTotalCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                lblTotalCount.Top = top + 4;
                lblTotalCount.Left = panelTop.Width - lblTotalCount.Width - margin;
            }

            // 菜单栏高度
            panelTop.Height = 48;

            // DataGridView布局
            dataGridView.Top = panelTop.Bottom + 1;
            dataGridView.Height = this.ClientSize.Height - panelTop.Height - statusStrip.Height - 2;
        }

        private void btnTestNetwork_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(sharedFolderPath))
            {
                MessageBox.Show("网络畅通", "网络状态", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // 显示登录对话框
                if (ShowLoginDialog())
                {
                    if (Directory.Exists(sharedFolderPath))
                    {
                        MessageBox.Show("网络连接成功", "网络状态", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // 重新加载文件
                        LoadSharedFolderFiles();
                    }
                    else
                    {
                        MessageBox.Show("登录成功但无法访问路径，请检查路径是否正确", "网络状态", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("请检查数据路径是否正常或用户凭据是否正确", "网络状态", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }


        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView.Columns.Contains("合同名称"))
            {
                string fileName = dataGridView.Rows[e.RowIndex].Cells["合同名称"].Value?.ToString();
                if (!string.IsNullOrEmpty(fileName))
                {
                    string filePath = Path.Combine(sharedFolderPath, fileName);
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(filePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("无法打开文件：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("文件不存在：" + filePath, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 判断是否点击了"选择"列
            if (e.RowIndex >= 0 && dataGridView.Columns[e.ColumnIndex].Name == "选择")
            {
                // 获取当前单元格
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                // 切换复选框状态
                cell.Value = !(Convert.ToBoolean(cell.Value));
                // 选中该行
                dataGridView.Rows[e.RowIndex].Selected = true;
            }
        }
        
        private void BtnSettings_Click(object sender, EventArgs e)
        {
            // 创建设置对话框
            Form settingsForm = new Form();
            settingsForm.Text = "设置";
            settingsForm.Width = 500;
            settingsForm.Height = 200;
            settingsForm.StartPosition = FormStartPosition.CenterParent;
            settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;
            
            // 添加路径输入框和标签
            Label lblPath = new Label();
            lblPath.Text = "共享文件夹路径:";
            lblPath.Location = new Point(20, 20);
            lblPath.AutoSize = true;
            
            TextBox txtPath = new TextBox();
            txtPath.Text = sharedFolderPath;
            txtPath.Width = 350;
            txtPath.Location = new Point(20, 50);
            
            // 添加测试连接按钮
            Button btnTest = new Button();
            btnTest.Text = "测试连接";
            btnTest.Width = 80;
            btnTest.Location = new Point(200, 100);
            btnTest.Click += (s, args) => {
                try
                {
                    if (Directory.Exists(txtPath.Text))
                    {
                        MessageBox.Show("连接成功！路径可访问。", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("无法访问该路径，请检查路径是否正确或尝试用网络登录。", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("测试连接失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            
            // 添加保存按钮
            Button btnSave = new Button();
            btnSave.Text = "保存";
            btnSave.Width = 80;
            btnSave.Location = new Point(290, 100);
            btnSave.Click += (s, args) => {
                sharedFolderPath = txtPath.Text;
                SaveConfig();
                MessageBox.Show("设置已保存，重新加载文件列表。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadSharedFolderFiles(); // 重新加载文件
                settingsForm.Close();
            };
            
            // 添加取消按钮
            Button btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Width = 80;
            btnCancel.Location = new Point(380, 100);
            btnCancel.Click += (s, args) => {
            settingsForm.Close();
            };
            
            // 添加控件到表单
            settingsForm.Controls.Add(lblPath);
            settingsForm.Controls.Add(txtPath);
            settingsForm.Controls.Add(btnTest);
            settingsForm.Controls.Add(btnSave);
            settingsForm.Controls.Add(btnCancel);
            
            // 显示设置对话框
            settingsForm.ShowDialog();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFilter();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (dataGridView.DataSource is DataView dv)
            {
                foreach (DataRowView rowView in dv)
                {
                    rowView["选择"] = chkSelectAll.Checked;
                }
            }
            else if (dataGridView.DataSource is DataTable dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    row["选择"] = chkSelectAll.Checked;
                }
            }
            dataGridView.Refresh();
        }

        private void AutoFitDataGridViewColumns()
        {
            // 先自适应内容
            dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            foreach (DataGridViewColumn col in dataGridView.Columns)
            {
                if (!col.Visible) continue;

                if (col.Name == "选择")
                {
                    col.Width = 40;
                    col.MinimumWidth = 40;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else if (col.Name == "序号")
                {
                    col.Width = 60;
                    col.MinimumWidth = 50;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else if (col.Name == "合同名称")
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // 填充剩余空间
                    col.MinimumWidth = 120;
                    col.FillWeight = 60;
                }
                else if (col.Name == "修改时间")
                {
                    col.Width = 130;
                    col.MinimumWidth = 100;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }
        }

        // 新建一个 UserControl 继承 Panel，重写 OnPaint 实现圆角
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                int radius = 12;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.LightGray, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }
    }
}