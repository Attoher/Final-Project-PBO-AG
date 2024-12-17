using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FormNavigation.HighScore;
using System.Linq;

namespace FormNavigation
{
    public class HighScoreForm : Form
    {
        private Button backButton;
        private Label titleLabel;
        private Panel scorePanel;

        public HighScoreForm()
        {
            InitializeForm();
            LoadHighScores();
        }

        private void InitializeForm()
        {
            this.Text = "High Scores";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 40); // Dark blue background

            titleLabel = new Label
            {
                Text = "HIGH SCORES",
                Font = new Font("Arial", 28, FontStyle.Bold),
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 20)
            };
            this.Controls.Add(titleLabel);

            scorePanel = new Panel
            {
                Location = new Point(50, 80),
                Size = new Size(300, 320),
                BackColor = Color.FromArgb(30, 30, 60),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(scorePanel);

            backButton = new Button
            {
                Text = "Back",
                Size = new Size(100, 35),
                Location = new Point((this.ClientSize.Width - 100) / 2, 420),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            backButton.Click += (s, e) => this.Close();
            this.Controls.Add(backButton);
        }

        private void LoadHighScores()
        {
            var scores = ScoreManager.GetHighScores();
            int y = 10;

            for (int i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                AddScoreLabel(i + 1, score.name, score.score, y);
                y += 30;
            }
        }

        private void AddScoreLabel(int rank, string name, int score, int y)
        {
            Panel scoreItemPanel = new Panel
            {
                Location = new Point(5, y),
                Size = new Size(290, 30),
                BackColor = rank <= 3 ? 
                    Color.FromArgb(40, 40, 80) : 
                    Color.FromArgb(35, 35, 70)
            };

            // Fixed widths for perfect alignment
            const int RANK_WIDTH = 40;
            const int NAME_WIDTH = 150;
            const int SCORE_WIDTH = 100;
            
            // Calculate center position
            int totalWidth = RANK_WIDTH + NAME_WIDTH + SCORE_WIDTH;
            int startX = (scoreItemPanel.Width - totalWidth) / 2;

            // Rank label
            Label rankLabel = new Label
            {
                Text = $"#{rank}",
                Font = rank <= 3 ? 
                    new Font("Arial", 14, FontStyle.Bold) : 
                    new Font("Arial", 12),
                ForeColor = rank switch
                {
                    1 => Color.Gold,
                    2 => Color.Silver,
                    3 => Color.FromArgb(205, 127, 50),
                    _ => Color.White
                },
                Size = new Size(RANK_WIDTH, 30),
                Location = new Point(startX, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Name label
            Label nameLabel = new Label
            {
                Text = name,
                Font = new Font("Consolas", 12),
                ForeColor = rankLabel.ForeColor,
                Size = new Size(NAME_WIDTH, 30),
                Location = new Point(startX + RANK_WIDTH, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Score label
            Label scoreLabel = new Label
            {
                Text = score.ToString("N0"),
                Font = rank <= 3 ? 
                    new Font("Arial", 14, FontStyle.Bold) : 
                    new Font("Arial", 12),
                ForeColor = rankLabel.ForeColor,
                Size = new Size(SCORE_WIDTH, 30),
                Location = new Point(startX + RANK_WIDTH + NAME_WIDTH, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add divider for top 3
            if (rank == 3)
            {
                Panel divider = new Panel
                {
                    Size = new Size(totalWidth, 2),
                    Location = new Point(startX, 30),
                    BackColor = Color.FromArgb(60, 60, 100)
                };
                scoreItemPanel.Controls.Add(divider);
            }

            scoreItemPanel.Controls.Add(rankLabel);
            scoreItemPanel.Controls.Add(nameLabel);
            scoreItemPanel.Controls.Add(scoreLabel);
            scorePanel.Controls.Add(scoreItemPanel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Add gradient background
            using (LinearGradientBrush gradient = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(20, 20, 40),
                Color.FromArgb(40, 40, 80),
                45f))
            {
                e.Graphics.FillRectangle(gradient, this.ClientRectangle);
            }
        }
    }
}
