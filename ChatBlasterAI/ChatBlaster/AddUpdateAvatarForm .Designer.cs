namespace ChatBlaster
{
    partial class AddUpdateAvatarForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;

        private void InitializeComponent()
        {
            textBoxEmail = new TextBox();
            textBoxUsername = new TextBox();
            textBoxPassword = new TextBox();
            lblEmail = new Label();
            lblUsername = new Label();
            lblPassword = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            photoFolderLabel = new Label();
            photoFolderTextBox = new TextBox();
            browseFolderButton = new Button();
            SuspendLayout();
            // 
            // textBoxEmail
            // 
            textBoxEmail.Location = new Point(118, 12);
            textBoxEmail.Name = "textBoxEmail";
            textBoxEmail.Size = new Size(220, 27);
            textBoxEmail.TabIndex = 1;
            // 
            // textBoxUsername
            // 
            textBoxUsername.Location = new Point(118, 55);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(220, 27);
            textBoxUsername.TabIndex = 3;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(118, 91);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(220, 27);
            textBoxPassword.TabIndex = 5;
            textBoxPassword.UseSystemPasswordChar = true;
            // 
            // lblEmail
            // 
            lblEmail.Location = new Point(12, 15);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(100, 23);
            lblEmail.TabIndex = 0;
            lblEmail.Text = "Email:";
            // 
            // lblUsername
            // 
            lblUsername.Location = new Point(12, 55);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(100, 23);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username:";
            // 
            // lblPassword
            // 
            lblPassword.Location = new Point(12, 95);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(100, 23);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(182, 203);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 33);
            btnSave.TabIndex = 6;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(263, 203);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 33);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // photoFolderLabel
            // 
            photoFolderLabel.Location = new Point(12, 127);
            photoFolderLabel.Name = "photoFolderLabel";
            photoFolderLabel.Size = new Size(100, 23);
            photoFolderLabel.TabIndex = 8;
            photoFolderLabel.Text = "PhotosPath:";
            // 
            // photoFolderTextBox
            // 
            photoFolderTextBox.Location = new Point(120, 124);
            photoFolderTextBox.Name = "photoFolderTextBox";
            photoFolderTextBox.ReadOnly = true;
            photoFolderTextBox.Size = new Size(220, 27);
            photoFolderTextBox.TabIndex = 9;
            // 
            // browseFolderButton
            // 
            browseFolderButton.Location = new Point(246, 157);
            browseFolderButton.Name = "browseFolderButton";
            browseFolderButton.Size = new Size(94, 29);
            browseFolderButton.TabIndex = 10;
            browseFolderButton.Text = "PhotoPath";
            browseFolderButton.UseVisualStyleBackColor = true;
            browseFolderButton.Click += browseFolderButton_Click;
            // 
            // AddUpdateAvatarForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(352, 248);
            Controls.Add(browseFolderButton);
            Controls.Add(photoFolderTextBox);
            Controls.Add(photoFolderLabel);
            Controls.Add(lblEmail);
            Controls.Add(textBoxEmail);
            Controls.Add(lblUsername);
            Controls.Add(textBoxUsername);
            Controls.Add(lblPassword);
            Controls.Add(textBoxPassword);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "AddUpdateAvatarForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Avatar";
            ResumeLayout(false);
            PerformLayout();
        }
        private Label photoFolderLabel;
        private Button browseFolderButton;
        public TextBox photoFolderTextBox;
    }
}
