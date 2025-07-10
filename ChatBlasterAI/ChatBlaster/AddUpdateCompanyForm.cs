using System;
using System.Windows.Forms;
using ChatBlaster.Models;

namespace ChatBlaster
{
    public partial class AddUpdateCompanyForm : Form
    {
        public readonly Company _company;

        public AddUpdateCompanyForm(Company? companyToEdit = null)
        {
            InitializeComponent();

            if (companyToEdit != null)
            {
                _company = companyToEdit;
                txtCompanyName.Text = _company.Name;
                txtOwnerName.Text = _company.OwnerName;
                txtOwnerPhone.Text = _company.OwnerPhone;
                txtOfficePhone.Text = _company.OfficePhone;

                this.Text = "Update Company";
            }
            else
            {
                this.Text = "Add New Company";
            }
        }

        public new string CompanyName => txtCompanyName.Text ?? string.Empty;
        public string OwnerName => txtOwnerName.Text;
        public string OwnerPhone => txtOwnerPhone.Text;
        public string OfficePhone => txtOfficePhone.Text;

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                MessageBox.Show("Company name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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