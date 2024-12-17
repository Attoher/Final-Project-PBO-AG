using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using FormNavigation.Characters;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace FormNavigation
{
    public class SelectCharacterForm : Form
    {
        private Button backButton;
        private const string CHARACTERS_PATH = "Resources/Characters";
        private PictureBox characterPreview;
        private Label characterDescription;
        private Character[] characters;
        private int currentCharacterIndex = 0;
        private Button leftButton;
        private Button rightButton;
        private Button selectButton;
        private Label statsLabel;

        public SelectCharacterForm()
        {
            InitializeCharacters();
            InitializeForm();
            InitializeControls();
            LoadCharacterPreview(0);
        }

        private void InitializeCharacters()
        {
            characters = new Character[]
            {
                new Ninja(),
                new Puppeteer(),
                new Samurai(),
                new Scarecrow(),
                new Shaman()
            };
        }

        private void InitializeForm()
        {
            this.Text = "Select Character";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeControls()
        {
            // Back button
            backButton = new Button
            {
                Text = "Back",
                Location = new Point(10, 10),
                Size = new Size(80, 30)
            };
            backButton.Click += BackButton_Click;
            this.Controls.Add(backButton);

            // Character preview
            characterPreview = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(300, 100),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            this.Controls.Add(characterPreview);

            // Navigation buttons
            leftButton = new Button
            {
                Text = "<",
                Size = new Size(50, 50),
                Location = new Point(characterPreview.Left - 60, characterPreview.Top + 75)
            };
            leftButton.Click += (s, e) => NavigateCharacters(-1);
            this.Controls.Add(leftButton);

            rightButton = new Button
            {
                Text = ">",
                Size = new Size(50, 50),
                Location = new Point(characterPreview.Right + 10, characterPreview.Top + 75)
            };
            rightButton.Click += (s, e) => NavigateCharacters(1);
            this.Controls.Add(rightButton);

            // Character description
            characterDescription = new Label
            {
                AutoSize = false,
                Size = new Size(400, 60),
                Location = new Point(200, characterPreview.Bottom + 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12)
            };
            this.Controls.Add(characterDescription);

            // Stats label
            statsLabel = new Label
            {
                AutoSize = false,
                Size = new Size(400, 100),
                Location = new Point(200, characterDescription.Bottom + 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12)
            };
            this.Controls.Add(statsLabel);

            // Select button
            selectButton = new Button
            {
                Text = "Select Character",
                Size = new Size(150, 40),
                Location = new Point(325, statsLabel.Bottom + 10),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            selectButton.Click += SelectButton_Click;
            this.Controls.Add(selectButton);
        }

        private void LoadCharacterPreview(int index)
        {
            try
            {
                string imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    CHARACTERS_PATH,
                    $"{characters[index].Name}_Idle.png");

                if (File.Exists(imagePath))
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        int frameWidth = characters[index].SpriteWidth;
                        int frameHeight = characters[index].SpriteHeight;

                        // Create a new bitmap with the exact size we want
                        var previewImage = new Bitmap(200, 200);
                        using (var g = Graphics.FromImage(previewImage))
                        {
                            // Set up pixel-perfect rendering
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g.PixelOffsetMode = PixelOffsetMode.Half;
                            g.CompositingQuality = CompositingQuality.AssumeLinear;
                            g.SmoothingMode = SmoothingMode.None;

                            // Clear background
                            g.Clear(Color.Transparent);

                            // Calculate scale to fit in preview box while maintaining aspect ratio
                            float scale = Math.Min(
                                180f / frameWidth,  // Leave some padding
                                180f / frameHeight
                            );

                            // Center the scaled image
                            int destWidth = (int)(frameWidth * scale);
                            int destHeight = (int)(frameHeight * scale);
                            int destX = (200 - destWidth) / 2;
                            int destY = (200 - destHeight) / 2;

                            // Draw only the first frame scaled up
                            g.DrawImage(originalImage,
                                new Rectangle(destX, destY, destWidth, destHeight),
                                new Rectangle(0, 0, frameWidth, frameHeight),
                                GraphicsUnit.Pixel);
                        }

                        characterPreview.Image?.Dispose();
                        characterPreview.Image = previewImage;
                    }

                    // Update character info
                    characterDescription.Text = characters[index].GetDescription();
                    statsLabel.Text = $"Health: {characters[index].MaxHealth}\n" +
                                    $"Attack: {characters[index].Attack}\n" +
                                    $"Speed: {characters[index].Speed}";
                }
                else
                {
                    MessageBox.Show($"Character sprite not found at: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading character preview: {ex.Message}");
            }
        }

        private void NavigateCharacters(int direction)
        {
            currentCharacterIndex = (currentCharacterIndex + direction + characters.Length) % characters.Length;
            LoadCharacterPreview(currentCharacterIndex);
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            GameState.SelectedCharacter = characters[currentCharacterIndex].Name;
            this.Close();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
