using ChatBlaster.Models;
namespace ChatBlaster
{
    partial class AddUpdateCompanyForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtCompanyName;
        private TextBox txtOwnerName;
        private TextBox txtOwnerPhone;
        private TextBox txtOfficePhone;
        private Button btnSave;
        private Button btnCancel;
        private Label lblCompanyName;
        private Label lblOwnerName;
        private Label lblOwnerPhone;
        private Label lblOfficePhone;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtCompanyName = new TextBox();
            txtOwnerName = new TextBox();
            txtOwnerPhone = new TextBox();
            txtOfficePhone = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            lblCompanyName = new Label();
            lblOwnerName = new Label();
            lblOwnerPhone = new Label();
            lblOfficePhone = new Label();

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.Controls.Add(lblCompanyName);
            this.Controls.Add(txtCompanyName);
            this.Controls.Add(lblOwnerName);
            this.Controls.Add(txtOwnerName);
            this.Controls.Add(lblOwnerPhone);
            this.Controls.Add(txtOwnerPhone);
            this.Controls.Add(lblOfficePhone);
            this.Controls.Add(txtOfficePhone);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.StartPosition = FormStartPosition.CenterParent;

            // Labels
            lblCompanyName.Text = "Company Name:";
            lblCompanyName.Location = new System.Drawing.Point(20, 20);
            lblCompanyName.Size = new System.Drawing.Size(120, 20);

            lblOwnerName.Text = "Owner Name:";
            lblOwnerName.Location = new System.Drawing.Point(20, 60);
            lblOwnerName.Size = new System.Drawing.Size(120, 20);

            lblOwnerPhone.Text = "Owner Phone:";
            lblOwnerPhone.Location = new System.Drawing.Point(20, 100);
            lblOwnerPhone.Size = new System.Drawing.Size(120, 20);

            lblOfficePhone.Text = "Office Phone:";
            lblOfficePhone.Location = new System.Drawing.Point(20, 140);
            lblOfficePhone.Size = new System.Drawing.Size(120, 20);

            // Textboxes
            txtCompanyName.Location = new System.Drawing.Point(150, 20);
            txtCompanyName.Size = new System.Drawing.Size(220, 22);

            txtOwnerName.Location = new System.Drawing.Point(150, 60);
            txtOwnerName.Size = new System.Drawing.Size(220, 22);

            txtOwnerPhone.Location = new System.Drawing.Point(150, 100);
            txtOwnerPhone.Size = new System.Drawing.Size(220, 22);

            txtOfficePhone.Location = new System.Drawing.Point(150, 140);
            txtOfficePhone.Size = new System.Drawing.Size(220, 22);

            // Buttons
            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(150, 190);
            btnSave.Size = new System.Drawing.Size(100, 30);
            btnSave.Click += new EventHandler(btnSave_Click);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(270, 190);
            btnCancel.Size = new System.Drawing.Size(100, 30);
            btnCancel.Click += new EventHandler(btnCancel_Click);

            this.Text = "Company Form";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}