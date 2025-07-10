namespace ChatBlaster
{
    partial class AddUpdateServiceForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxServiceName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private void InitializeComponent()
        {
            textBoxServiceName = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            PhoneNumbertextBox = new TextBox();
            lblServiceName = new Label();
            label3 = new Label();
            FacebookCategorytextBox = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // textBoxServiceName
            // 
            textBoxServiceName.Location = new Point(170, 15);
            textBoxServiceName.Name = "textBoxServiceName";
            textBoxServiceName.Size = new Size(220, 27);
            textBoxServiceName.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(234, 114);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 32);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(315, 114);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 32);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // PhoneNumbertextBox
            // 
            PhoneNumbertextBox.Location = new Point(170, 81);
            PhoneNumbertextBox.Name = "PhoneNumbertextBox";
            PhoneNumbertextBox.Size = new Size(220, 27);
            PhoneNumbertextBox.TabIndex = 6;
            // 
            // lblServiceName
            // 
            lblServiceName.Location = new Point(12, 18);
            lblServiceName.Name = "lblServiceName";
            lblServiceName.Size = new Size(80, 23);
            lblServiceName.TabIndex = 0;
            lblServiceName.Text = "Industry:";
            // 
            // label3
            // 
            label3.Location = new Point(12, 84);
            label3.Name = "label3";
            label3.Size = new Size(134, 23);
            label3.TabIndex = 9;
            label3.Text = "PhoneNumber:";
            // 
            // FacebookCategorytextBox
            // 
            FacebookCategorytextBox.Location = new Point(170, 48);
            FacebookCategorytextBox.Name = "FacebookCategorytextBox";
            FacebookCategorytextBox.Size = new Size(220, 27);
            FacebookCategorytextBox.TabIndex = 10;
            // 
            // label1
            // 
            label1.Location = new Point(12, 52);
            label1.Name = "label1";
            label1.Size = new Size(152, 23);
            label1.TabIndex = 11;
            label1.Text = "Facebook Category:";
            // 
            // AddUpdateServiceForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(402, 161);
            Controls.Add(label1);
            Controls.Add(FacebookCategorytextBox);
            Controls.Add(label3);
            Controls.Add(PhoneNumbertextBox);
            Controls.Add(lblServiceName);
            Controls.Add(textBoxServiceName);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "AddUpdateServiceForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Service";
            ResumeLayout(false);
            PerformLayout();
        }
        private TextBox PhoneNumbertextBox;
        private Label lblServiceName;
        private Label label3;
        private TextBox FacebookCategorytextBox;
        private Label label1;
    }
}
