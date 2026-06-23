using System;
using System.Drawing;
using System.Windows.Forms;

namespace Panel.Helpers;

public static class UIThemeHelper
{
    public static void ApplyFlatTheme(Form form)
    {
        // Lưu lại chính xác thông số ClientSize nội dung ban đầu
        int cWidth = form.ClientSize.Width;
        int cHeight = form.ClientSize.Height;

        form.FormBorderStyle = FormBorderStyle.None;
        
        int barH = 34;
        form.ClientSize = new Size(cWidth, cHeight + barH);
        
        // Dịch chuyển các control hiện có xuống để nhường chỗ cho Title Bar
        foreach (Control c in form.Controls)
        {
            if (c.Dock == DockStyle.None)
            {
                c.Top += barH;
            }
        }

        // Tông nền sáng Option C
        form.BackColor = Color.FromArgb(241, 245, 249);
        // Không dùng Padding vì đã shift Top trực tiếp

        SetupFlatControls(form);
        BuildTitleBar(form, barH);
    }

    private static void BuildTitleBar(Form form, int barH)
    {
        var barBg   = Color.FromArgb(71, 85, 105);   // Slate-600
        var barText = Color.FromArgb(241, 245, 249); // Slate-100

        var titleBar = new System.Windows.Forms.Panel
        {
            Name      = "panelTitleBar",
            Dock      = DockStyle.Top,
            Height    = barH,
            BackColor = barBg
        };

        var btnClose = new Button
        {
            Text      = "",
            Width     = 46,
            Dock      = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand,
            TabStop   = false
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38); 
        var closeHover = false;
        btnClose.MouseEnter += (_, __) => { closeHover = true;  btnClose.Invalidate(); };
        btnClose.MouseLeave += (_, __) => { closeHover = false; btnClose.Invalidate(); };
        btnClose.Paint += (s, pe) => {
            var g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var c = closeHover ? Color.White : Color.FromArgb(252, 129, 129);
            using var pen = new Pen(c, 2f);
            int cx = btnClose.Width / 2;
            int cy = btnClose.Height / 2;
            int r = Math.Min(btnClose.Width, btnClose.Height) / 5;
            g.DrawLine(pen, cx - r, cy - r, cx + r, cy + r);
            g.DrawLine(pen, cx + r, cy - r, cx - r, cy + r);
        };
        btnClose.Click += (_, __) => form.Close();

        var lblTitle = new Label
        {
            Text      = form.Text,
            ForeColor = barText,
            Font      = new Font("Segoe UI", 9f, FontStyle.Regular),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            Padding   = new Padding(14, 0, 0, 0)
        };
        
        form.TextChanged += (s, e) => { lblTitle.Text = form.Text; };

        titleBar.Controls.Add(btnClose);
        titleBar.Controls.Add(lblTitle);

        Point dragOff = Point.Empty;
        bool  isDrag  = false;
        MouseEventHandler onDown = (s, e) => { isDrag = true;  dragOff = e.Location; };
        MouseEventHandler onMove = (s, e) => { if (isDrag) form.Location = new Point(form.Location.X + e.X - dragOff.X, form.Location.Y + e.Y - dragOff.Y); };
        MouseEventHandler onUp   = (s, e) => isDrag = false;

        titleBar.MouseDown += onDown;
        titleBar.MouseMove += onMove;
        titleBar.MouseUp   += onUp;
        
        lblTitle.MouseDown += onDown;
        lblTitle.MouseMove += onMove;
        lblTitle.MouseUp   += onUp;

        form.Controls.Add(titleBar);
        titleBar.SendToBack();
    }

    private static void SetupFlatControls(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is Button btn && btn.Name != "btnClose" && btn.Name != "btnMin")
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.FromArgb(71, 85, 105);   // Slate-600
                btn.ForeColor = Color.FromArgb(241, 245, 249); // Slate-100
                btn.Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular); 
                btn.FlatAppearance.BorderSize          = 1; 
                btn.FlatAppearance.BorderColor         = Color.FromArgb(100, 116, 139); 
                btn.FlatAppearance.MouseOverBackColor  = Color.FromArgb(100, 116, 139); 
                btn.FlatAppearance.MouseDownBackColor  = Color.FromArgb(51, 65, 85);  
                btn.Cursor    = Cursors.Hand;
                btn.UseVisualStyleBackColor = false;
            }
            else if (c is TextBox txt)
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is ComboBox cbo)
            {
                cbo.FlatStyle = FlatStyle.Flat;
            }
            else if (c is GroupBox grp)
            {
                grp.FlatStyle = FlatStyle.Flat;
                grp.ForeColor = Color.FromArgb(30, 41, 59);
            }
            else if (c is DataGridView dgv)
            {
                dgv.BackgroundColor = Color.FromArgb(248, 250, 252);
                dgv.BorderStyle = BorderStyle.None;
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248); // Sky-400
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                dgv.RowHeadersVisible = false;
                dgv.GridColor = Color.FromArgb(226, 232, 240); // Slate-200
            }
            else if (c is CheckBox chk)
            {
                chk.FlatStyle = FlatStyle.Flat;
                chk.ForeColor = Color.FromArgb(51, 65, 85);
            }
            else if (c is RadioButton rad)
            {
                rad.FlatStyle = FlatStyle.Flat;
                rad.ForeColor = Color.FromArgb(51, 65, 85);
            }
            else if (c is Label lbl && lbl.Name != "lblTitle")
            {
                lbl.ForeColor = Color.FromArgb(51, 65, 85);
            }
            else if (c is NumericUpDown nud)
            {
                nud.BackColor = Color.White;
                nud.ForeColor = Color.FromArgb(30, 41, 59);
            }
            else if (c is TabPage tabPage)
            {
                tabPage.BackColor = Color.FromArgb(248, 250, 252);
            }
            
            if (c.HasChildren)
            {
                SetupFlatControls(c);
            }
        }
    }
}
