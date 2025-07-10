using System;
using System.Windows.Forms;
using ChatBlaster.Models;

namespace ChatBlaster
{
    public partial class AddUpdateServiceAreaForm : Form
    {
        private int? _CompanyId;
        private ServiceArea _area;
        public AddUpdateServiceAreaForm(int? company_id = null, ServiceArea areaToEdit = null)
        {
            InitializeComponent();
            if (areaToEdit != null)
            {
                _area = areaToEdit;
                _CompanyId = areaToEdit.CompanyId;
                textBoxAreaCity.Text = _area.City;
                textBoxAreaState.Text = _area.State;
                textBoxAreaZip.Text = _area.ZipCode;
                textBoxAreaSCities.Text = _area.SurroundingCities;
                textBoxAreaRadius.Text = _area.Radius.ToString();
                this.Text = "Update Area of Service";
            }
            else if (company_id != null)
            {
                _CompanyId = company_id;
                this.Text = "Add New Area of Service";
            }
            else
            {
                MessageBox.Show("Either companyId or areaToEdit must be provided.");
            }
        }

        public string City => textBoxAreaCity.Text;
        public string State => textBoxAreaState.Text;
        public string ZipCode => textBoxAreaZip.Text;
        public string SurroundingCities => textBoxAreaSCities.Text;
        public string RadiusText => textBoxAreaRadius.Text;
        public int? CompanyId => _CompanyId;
        public ServiceArea Area => _area;

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