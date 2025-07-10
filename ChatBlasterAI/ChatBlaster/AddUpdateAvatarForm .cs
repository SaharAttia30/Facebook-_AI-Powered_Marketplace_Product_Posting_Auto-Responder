using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatBlaster.Models;



namespace ChatBlaster
{
    public partial class AddUpdateAvatarForm : Form
    {
  
        private Avatar _avatar;
        public AddUpdateAvatarForm(Avatar? avatarToEdit = null)
        {
            InitializeComponent();
            folderBrowserDialog1 = new FolderBrowserDialog();
            if (avatarToEdit != null)
            {
                _avatar = avatarToEdit;
                textBoxEmail.Text = _avatar._email;
                textBoxUsername.Text = _avatar._userName;
                textBoxPassword.Text = _avatar._password;
                this.Text = "Update Avatar";
            }
            else
            {
                this.Text = "Add New Avatar";
            }
        }

        public string Email => textBoxEmail.Text;
        public string Username => textBoxUsername.Text;
        public string Password => textBoxPassword.Text;
        public string PhotoFolder => photoFolderTextBox.Text;

        private void browseFolderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                photoFolderTextBox.Text = folderBrowserDialog1.SelectedPath;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

}
