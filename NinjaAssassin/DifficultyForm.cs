using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormNavigation
{
    public class DifficultyForm : Form
    {
        private Button backButton;

        public DifficultyForm()
        {
            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = "Select Difficulty";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            int buttonWidth = 120;
            int buttonHeight = 40;
            int verticalSpacing = 10;

            // Label untuk memilih tingkat kesulitan
            Label label = new Label
            {
                Text = "Choose Difficulty",
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true
            };

            // Menambahkan label ke kontrol terlebih dahulu
            this.Controls.Add(label);

            // Mengatur lokasi label agar berada di tengah secara horizontal
            label.Location = new Point((this.ClientSize.Width - label.Width) / 2, 20);

            // Tombol untuk memilih tingkat kesulitan
            Button easyButton = new Button
            {
                Text = "Easy",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, label.Bottom + verticalSpacing)
            };
            easyButton.Click += (sender, args) => MessageBox.Show("Easy Difficulty Selected");
            this.Controls.Add(easyButton);

            Button mediumButton = new Button
            {
                Text = "Medium",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, easyButton.Bottom + verticalSpacing)
            };
            mediumButton.Click += (sender, args) => MessageBox.Show("Medium Difficulty Selected");
            this.Controls.Add(mediumButton);

            Button hardButton = new Button
            {
                Text = "Hard",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, mediumButton.Bottom + verticalSpacing)
            };
            hardButton.Click += (sender, args) => MessageBox.Show("Hard Difficulty Selected");
            this.Controls.Add(hardButton);
        }

        private void InitializeControls()
        {
            // Tombol Back
            backButton = new Button
            {
                Text = "Back",
                Location = new Point(10, 10),
                Size = new Size(80, 30)
            };
            backButton.Click += BackButton_Click;
            this.Controls.Add(backButton);
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Menutup DifficultyForm dan kembali ke form sebelumnya
        }
    }
}
