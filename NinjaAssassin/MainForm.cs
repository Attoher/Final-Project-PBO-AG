using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormNavigation
{
    public class MainForm : Form
    {
        private Button startGameButton;
        private Button selectCharacterButton;
        private Button difficultyButton;
        private Button exitButton;
        private Button highScoreButton;

        public MainForm()
        {
            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = "Main Menu";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeControls()
        {
            int buttonWidth = 120;
            int buttonHeight = 40;
            int verticalSpacing = 10;

            // Tombol Start Game
            startGameButton = new Button
            {
                Text = "Start Game",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, (this.ClientSize.Height - buttonHeight * 5 - verticalSpacing * 4) / 2)
            };
            startGameButton.Click += StartGameButton_Click;
            this.Controls.Add(startGameButton);

            // Tombol Select Character
            selectCharacterButton = new Button
            {
                Text = "Select Character",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, startGameButton.Bottom + verticalSpacing)
            };
            selectCharacterButton.Click += SelectCharacterButton_Click;
            this.Controls.Add(selectCharacterButton);

            // Tombol Difficulty Selection
            difficultyButton = new Button
            {
                Text = "Select Difficulty",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, selectCharacterButton.Bottom + verticalSpacing)
            };
            difficultyButton.Click += DifficultyButton_Click;
            this.Controls.Add(difficultyButton);

            // Tombol High Score
            highScoreButton = new Button
            {
                Text = "High Scores",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, difficultyButton.Bottom + verticalSpacing)
            };
            highScoreButton.Click += HighScoreButton_Click;
            this.Controls.Add(highScoreButton);

            // Tombol Exit
            exitButton = new Button
            {
                Text = "Exit",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point((this.ClientSize.Width - buttonWidth) / 2, highScoreButton.Bottom + verticalSpacing)
            };
            exitButton.Click += ExitButton_Click;
            this.Controls.Add(exitButton);
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            GameForm gameForm = new GameForm();
            gameForm.FormClosed += (s, args) => this.Show();
            gameForm.Show();
            this.Hide();
        }

        private void SelectCharacterButton_Click(object sender, EventArgs e)
        {
            SelectCharacterForm selectCharacterForm = new SelectCharacterForm();
            selectCharacterForm.ShowDialog();
        }

        private void DifficultyButton_Click(object sender, EventArgs e)
        {
            DifficultyForm difficultyForm = new DifficultyForm();
            difficultyForm.ShowDialog();
        }

        private void HighScoreButton_Click(object sender, EventArgs e)
        {
            using (var highScoreForm = new HighScoreForm())
            {
                highScoreForm.ShowDialog();
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
