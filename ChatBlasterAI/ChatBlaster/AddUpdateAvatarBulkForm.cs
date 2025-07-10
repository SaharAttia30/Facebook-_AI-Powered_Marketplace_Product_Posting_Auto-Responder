using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ChatBlaster
{
    public partial class AddUpdateAvatarBulkForm : Form
    {
        readonly string[] _standardCols = { "Number", "Id", "Email / Phone", "User Name", "Password", "2FA hash" };

        public sealed class CsvAvatar
        {
            public string Number { get; init; } = string.Empty;
            public string Id { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string Password { get; init; } = string.Empty;
            public string TwoFa { get; init; } = string.Empty;
            public string UserName { get; init; } = string.Empty;
            public string PhotosPath { get; init; } = string.Empty;
        }

        readonly DataTable _table = new();
        public IReadOnlyList<CsvAvatar> Avatars { get; private set; } = Array.Empty<CsvAvatar>();

        public AddUpdateAvatarBulkForm()
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            InitializeComponent();
        }

        /* browse csv */
        void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            csvPathTextBox.Text = openFileDialog1.FileName;
            LoadCsv(openFileDialog1.FileName);
            BindColumnCombos();
        }
        void browseFolderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                photoFolderTextBox.Text = folderBrowserDialog1.SelectedPath;
        }
        public string PhotosPath => photoFolderTextBox.Text;
        void BindColumnCombos()
        {
            // headers found in the CSV (preserve original case)
            var headers = _table.Columns.Cast<DataColumn>()
                                        .Select(c => c.ColumnName)
                                        .ToList();

            // helper to wire one ComboBox
            void bind(System.Windows.Forms.ComboBox cb, string preferred)
            {
                cb.DataSource = new List<string>(new[] { "<none>" }.Concat(headers)); // <none> + real headers
                cb.SelectedItem = headers.FirstOrDefault(h =>
                                  h.Equals(preferred, StringComparison.OrdinalIgnoreCase))
                                  ?? "<none>";
            }

            bind(numberComboBox, "Number");
            bind(idComboBox, "Id");
            bind(emailComboBox, "Email / Phone");
            bind(usernameComboBox, "User Name");      // stays <none> if column absent
            bind(passwordComboBox, "Password");
            bind(twoFaComboBox, "2FA hash");
        }
        //void BindColumnCombos()
        //{
        //    var src = _table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

        //    // create <none> + standard suggestions first
        //    var baseList = _standardCols.Concat(src).Distinct(StringComparer.OrdinalIgnoreCase).Prepend("<none>").ToList();

        //    void bind(System.Windows.Forms.ComboBox cb, string defaultName)
        //    {
        //        cb.DataSource = new List<string>(baseList);               // clone
        //                                                                  // pick default column if present, else <none>
        //        cb.SelectedItem = src.FirstOrDefault(c => c.Equals(defaultName, StringComparison.OrdinalIgnoreCase))
        //                          ?? "<none>";
        //    }

        //    bind(numberComboBox, "Number");
        //    bind(idComboBox, "Id");
        //    bind(emailComboBox, "Email / Phone");
        //    bind(usernameComboBox, "User Name");
        //    bind(passwordComboBox, "Password");
        //    bind(twoFaComboBox, "2FA hash");
        //}

        /* upload / finish */
        void uploadButton_Click(object sender, EventArgs e)
        {
            // required columns
            var numCol = numberComboBox.SelectedItem as string ?? string.Empty;
            var user_name_col = usernameComboBox.SelectedItem as string ?? string.Empty;

            var idCol = idComboBox.SelectedItem as string ?? string.Empty;
            var mailCol = emailComboBox.SelectedItem as string ?? string.Empty;
            var passCol = passwordComboBox.SelectedItem as string ?? string.Empty;
            var photo_path = photoFolderTextBox.Text as string ?? string.Empty;
            if (Invalid(numCol) || Invalid(mailCol) || Invalid(passCol))
            {
                MessageBox.Show("Number, Email and Password columns are mandatory.");
                return;
            }

            var hashCol = twoFaComboBox.SelectedItem as string ?? string.Empty;

            Avatars = _table.AsEnumerable()
                            .Select(r => new CsvAvatar
                            {
                                Number = r[numCol]?.ToString() ?? string.Empty,
                                Id = r[idCol]?.ToString() ?? string.Empty,
                                Email = r[mailCol]?.ToString() ?? string.Empty,
                                Password = r[passCol]?.ToString() ?? string.Empty,
                                TwoFa = (UseCol(hashCol) ? r[hashCol!].ToString() : string.Empty) ?? string.Empty,
                                UserName = (UseCol(user_name_col) ? r[user_name_col!].ToString() : string.Empty) ?? string.Empty,
                                PhotosPath = photo_path.ToString() ?? string.Empty 
                            })
                            .Where(a => a.Email.Length > 0 && a.Password.Length > 0
                                        && a.Number.Length > 0 && a.Id.Length > 0)
                            .ToList();

            if (Avatars.Count == 0)
            {
                MessageBox.Show("No valid rows found."); return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        /* helpers */
        void LoadCsv(string path)
        {
            _table.Clear();
            using var p = new TextFieldParser(path) { HasFieldsEnclosedInQuotes = true };
            p.SetDelimiters(",");
            var table_column = p.ReadFields() ?? new List<string>().ToArray(); 
            foreach (var h in table_column) 
            { 
                _table.Columns.Add(h); 
            }
            while (!p.EndOfData) _table.Rows.Add(table_column);
        }

        static bool Invalid(string col) => string.IsNullOrWhiteSpace(col) || col == "<none>";
        static bool UseCol(string col) => !string.IsNullOrWhiteSpace(col) && col != "<none>";
    }
}
