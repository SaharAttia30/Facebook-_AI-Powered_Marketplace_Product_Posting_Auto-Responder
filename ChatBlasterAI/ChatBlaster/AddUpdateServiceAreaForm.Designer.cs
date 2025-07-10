namespace ChatBlaster
{
    partial class AddUpdateServiceAreaForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxAreaName;
        private System.Windows.Forms.Label lblAreaName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private void InitializeComponent()
        {
            textBoxAreaCity = new TextBox();
            lblAreaName = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            textBoxAreaState = new TextBox();
            textBoxAreaZip = new TextBox();
            textBoxAreaSCities = new TextBox();
            textBoxAreaRadius = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // textBoxAreaCity
            // 
            textBoxAreaCity.Location = new Point(100, 15);
            textBoxAreaCity.Name = "textBoxAreaCity";
            textBoxAreaCity.Size = new Size(220, 27);
            textBoxAreaCity.TabIndex = 1;
            // 
            // lblAreaName
            // 
            lblAreaName.Location = new Point(12, 18);
            lblAreaName.Name = "lblAreaName";
            lblAreaName.Size = new Size(80, 23);
            lblAreaName.TabIndex = 0;
            lblAreaName.Text = "City:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(100, 196);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 33);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(234, 196);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 33);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // textBoxAreaState
            // 
            textBoxAreaState.Location = new Point(100, 48);
            textBoxAreaState.Name = "textBoxAreaState";
            textBoxAreaState.Size = new Size(220, 27);
            textBoxAreaState.TabIndex = 4;
            // 
            // textBoxAreaZip
            // 
            textBoxAreaZip.Location = new Point(100, 81);
            textBoxAreaZip.Name = "textBoxAreaZip";
            textBoxAreaZip.Size = new Size(220, 27);
            textBoxAreaZip.TabIndex = 5;
            // 
            // textBoxAreaSCities
            // 
            textBoxAreaSCities.Location = new Point(100, 114);
            textBoxAreaSCities.Name = "textBoxAreaSCities";
            textBoxAreaSCities.Size = new Size(220, 27);
            textBoxAreaSCities.TabIndex = 6;
            // 
            // textBoxAreaRadius
            // 
            textBoxAreaRadius.Location = new Point(100, 147);
            textBoxAreaRadius.Name = "textBoxAreaRadius";
            textBoxAreaRadius.Size = new Size(220, 27);
            textBoxAreaRadius.TabIndex = 7;
            // 
            // label1
            // 
            label1.Location = new Point(12, 51);
            label1.Name = "label1";
            label1.Size = new Size(80, 23);
            label1.TabIndex = 8;
            label1.Text = "State:";
            // 
            // label2
            // 
            label2.Location = new Point(12, 84);
            label2.Name = "label2";
            label2.Size = new Size(80, 23);
            label2.TabIndex = 9;
            label2.Text = "Zip Code:";
            // 
            // label3
            // 
            label3.Location = new Point(12, 117);
            label3.Name = "label3";
            label3.Size = new Size(80, 23);
            label3.TabIndex = 10;
            label3.Text = "Cities:";
            // 
            // label4
            // 
            label4.Location = new Point(12, 151);
            label4.Name = "label4";
            label4.Size = new Size(80, 23);
            label4.TabIndex = 11;
            label4.Text = "Radius:";
            // 
            // AddUpdateServiceAreaForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(340, 241);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxAreaRadius);
            Controls.Add(textBoxAreaSCities);
            Controls.Add(textBoxAreaZip);
            Controls.Add(textBoxAreaState);
            Controls.Add(lblAreaName);
            Controls.Add(textBoxAreaCity);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "AddUpdateServiceAreaForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Area";
            ResumeLayout(false);
            PerformLayout();
        }
        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox textBoxAreaCity;
        private TextBox textBoxAreaState;
        private TextBox textBoxAreaZip;
        private TextBox textBoxAreaSCities;
        private TextBox textBoxAreaRadius;
    }
}
