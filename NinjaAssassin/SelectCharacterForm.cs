using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormNavigation
{
    public class SelectCharacterForm : Form
    {
        private Button backButton;

        public SelectCharacterForm()
        {
            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = "Select Character";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            int buttonWidth = 120;
            int buttonHeight = 40;
            int verticalSpacing = 10;

            // Label untuk memilih karakter
            Label label = new Label
            {
                Text = "Choose Your Character",
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true // Ensure that the label resizes to fit its text
            };
            this.Controls.Add(label); // Add label first so we can measure its width

            // Set label location after it's added to the controls
            label.Location = new Point((this.ClientSize.Width - label.Width) / 2, 20);

            // Tombol untuk memilih karakter
            Button character1Button = new Button
            {
                Text = "Character 1",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, label.Bottom + verticalSpacing)
            };
            character1Button.Click += (sender, args) => MessageBox.Show("Character 1 Selected");
            this.Controls.Add(character1Button);

            Button character2Button = new Button
            {
                Text = "Character 2",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, character1Button.Bottom + verticalSpacing)
            };
            character2Button.Click += (sender, args) => MessageBox.Show("Character 2 Selected");
            this.Controls.Add(character2Button);
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
            this.Close(); // Menutup SelectCharacterForm dan kembali ke form sebelumnya
        }
    }
}
