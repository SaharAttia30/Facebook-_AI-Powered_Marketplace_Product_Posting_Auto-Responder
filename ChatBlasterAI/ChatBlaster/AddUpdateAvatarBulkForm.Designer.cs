namespace ChatBlaster
{
    partial class AddUpdateAvatarBulkForm
    {
        System.ComponentModel.IContainer components = null;

        System.Windows.Forms.OpenFileDialog openFileDialog1;

        System.Windows.Forms.Label titleLabel, csvLabel, mappingLabel, numberLabel, IdLabel, twoFaLabel,
                                   usernameLabel, emailLabel, passwordLabel;

        System.Windows.Forms.TextBox csvPathTextBox;
        System.Windows.Forms.Button browseButton, uploadButton;

        System.Windows.Forms.ComboBox usernameComboBox, emailComboBox, numberComboBox, idComboBox, twoFaComboBox,passwordComboBox;
        System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label photoFolderLabel;
        private System.Windows.Forms.TextBox photoFolderTextBox;
        private System.Windows.Forms.Button browseFolderButton;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            openFileDialog1 = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            titleLabel = new System.Windows.Forms.Label
            {
                Text = "Bulk Upload Avatars",
                Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(80, 20)
            };

            csvLabel = new System.Windows.Forms.Label
            {
                Text = "CSV file:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 80)
            };

            csvPathTextBox = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(100, 75),
                Size = new System.Drawing.Size(200, 23)
            };

            browseButton = new System.Windows.Forms.Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(310, 74),
                Size = new System.Drawing.Size(90, 25)
            };
            browseButton.Click += browseButton_Click;

            mappingLabel = new System.Windows.Forms.Label
            {
                Text = "Column mappings",
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(30, 130)
            };

            numberLabel = new System.Windows.Forms.Label
            {
                Text = "Number:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 160)
            };
            IdLabel = new System.Windows.Forms.Label
            {
                Text = "User Id:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 195)
            };
            usernameLabel = new System.Windows.Forms.Label
            {
                Text = "User Name:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 230)
            };
            emailLabel = new System.Windows.Forms.Label
            {
                Text = "Email:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 265)
            };

            passwordLabel = new System.Windows.Forms.Label
            {
                Text = "Password:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 300)
            };

            twoFaLabel = new System.Windows.Forms.Label
            {
                Text = "2FA Hash:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 335)
            };
            numberComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(110, 165),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            idComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(110, 200),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            usernameComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(110, 235),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            emailComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(110, 270),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            passwordComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(110, 305),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            twoFaComboBox = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(110, 340),
                Size = new System.Drawing.Size(200, 23),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            photoFolderLabel = new System.Windows.Forms.Label
            {
                Text = "Photos Folder:",
                AutoSize = true,
                Location = new System.Drawing.Point(30, 110)
            };
            photoFolderTextBox = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(120, 105),
                Size = new System.Drawing.Size(180, 23),
                ReadOnly = true
            };
            browseFolderButton = new System.Windows.Forms.Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(310, 104),
                Size = new System.Drawing.Size(90, 25)
            };
            browseFolderButton.Click += browseFolderButton_Click;

            foreach (var cb in new[]
                     { numberComboBox, idComboBox, emailComboBox, passwordComboBox, twoFaComboBox })
                cb.Items.Add("<none>");

            uploadButton = new System.Windows.Forms.Button
            {
                Text = "Upload",
                Location = new System.Drawing.Point(150, 340),
                Size = new System.Drawing.Size(100, 30)
            };
            uploadButton.Click += uploadButton_Click;
            uploadButton = new System.Windows.Forms.Button
            {
                Text = "Upload",
                Location = new System.Drawing.Point(150, 360),
                Size = new System.Drawing.Size(100, 30)
            };
            uploadButton.Click += uploadButton_Click;
            Controls.AddRange(new System.Windows.Forms.Control[]
            {
                titleLabel, csvLabel, csvPathTextBox, browseButton,
                photoFolderLabel,
                photoFolderTextBox,
                browseFolderButton,
                mappingLabel,
                numberLabel,   numberComboBox,
                IdLabel,       idComboBox,
                usernameLabel,  usernameComboBox,
                emailLabel,    emailComboBox,
                passwordLabel, passwordComboBox,
                twoFaLabel,    twoFaComboBox,
                uploadButton
                
            });

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(550,600);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Bulk Upload Avatars";
        }
    }
}
