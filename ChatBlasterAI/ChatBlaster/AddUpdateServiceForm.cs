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
    public partial class AddUpdateServiceForm : Form
    {
        private Service _service;
        private string? _serviceAreaId;

        public AddUpdateServiceForm(string? service_area_id = null, Service serviceToEdit = null)
        {
            InitializeComponent();
            if (serviceToEdit != null)
            {
                _service = serviceToEdit;
                textBoxServiceName.Text = _service.Industry;
                PhoneNumbertextBox.Text = _service.PhoneNumber;
                FacebookCategorytextBox.Text = _service.FacebookCategory;
                this.Text = "Update Service";
            }
            else if(service_area_id != null)
            {
                this.Text = "Add New Service";
                _serviceAreaId = service_area_id;
            }
            else
            {
                MessageBox.Show("Either ServiceAreaId or serviceToEdit must be provided.");
            }
        }

        public string Industry => textBoxServiceName.Text;
        public string PhoneNumber => PhoneNumbertextBox.Text;
        public string FacebookCategory => FacebookCategorytextBox.Text;
        public string? ServiceAreaId => _serviceAreaId;
        public Service serviceToEdit => _service;


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_service != null)
            {
                _service.Industry = textBoxServiceName.Text;
                _service.PhoneNumber = PhoneNumbertextBox.Text;
                _service.FacebookCategory = FacebookCategorytextBox.Text;
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
