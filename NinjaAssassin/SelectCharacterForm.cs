using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace FormNavigation
{
    public class SelectCharacterForm : Form
    {
        private Button backButton;
        private Panel ninjaPanel;
        private Panel puppeteerPanel;
        private Panel samuraiPanel;
        private Panel scarecrowPanel;
        private Panel shamanPanel;
        private string selectedCharacter = "";
        private const string CHARACTERS_PATH = "Resources/Characters";
        private SpriteAnimation ninjaAnimation;
        private SpriteAnimation puppeteerAnimation;
        private SpriteAnimation samuraiAnimation;
        private SpriteAnimation scarecrowAnimation;
        private SpriteAnimation shamanAnimation;
        private Panel characterPanel; // Add this field at the top with other fields
        private Image arrowImage;
        private const string ARROW_PATH = "Resources/Arrow.png";
        private Panel arrowPanel;
        private Point centerPoint;

        public SelectCharacterForm()
        {
            InitializeForm();
            InitializeControls();
            LoadCharacterPreviews();
        }

        private void InitializeForm()
        {
            this.Text = "Select Character";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label label = new Label
            {
                Text = "Choose Your Character",
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(label);
            label.Location = new Point((this.ClientSize.Width - label.Width) / 2, 20);

            try
            {
                arrowImage = Image.FromFile(ARROW_PATH);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading arrow image: {ex.Message}");
            }
        }

        private void LoadCharacterPreviews()
        {
            characterPanel = new Panel // Store reference to the panel
            {
                Size = new Size(800, 600),
                Location = new Point(50, 50)
            };
            this.Controls.Add(characterPanel);

            // Calculate positions for pentagon arrangement
            int centerX = characterPanel.Width / 2;
            int centerY = characterPanel.Height / 2;
            int radius = 200; // Radius of the pentagon
            int panelSize = 150; // Size of each character panel

            // Calculate positions (pentagon vertices)
            Point[] positions = new Point[5];
            for (int i = 0; i < 5; i++)
            {
                double angle = i * 2 * Math.PI / 5 - Math.PI / 2; // Start from top
                positions[i] = new Point(
                    (int)(centerX + radius * Math.Cos(angle)) - panelSize/2,
                    (int)(centerY + radius * Math.Sin(angle)) - panelSize/2
                );
            }

            // Create character panels
            ninjaPanel = CreateCharacterPanel(positions[0], panelSize);
            puppeteerPanel = CreateCharacterPanel(positions[1], panelSize);
            samuraiPanel = CreateCharacterPanel(positions[2], panelSize);
            scarecrowPanel = CreateCharacterPanel(positions[3], panelSize);
            shamanPanel = CreateCharacterPanel(positions[4], panelSize);

            characterPanel.Controls.AddRange(new Control[] { 
                ninjaPanel, puppeteerPanel, samuraiPanel, scarecrowPanel, shamanPanel 
            });

            // Create a transparent panel for the arrow in the center
            arrowPanel = new Panel
            {
                Size = new Size(150, 150),
                BackColor = Color.Transparent
            };
            
            // Calculate center position
            centerPoint = new Point(
                characterPanel.Width / 2,
                characterPanel.Height / 2
            );
            
            // Position the arrow panel in the center
            arrowPanel.Location = new Point(
                centerPoint.X - arrowPanel.Width / 2 + characterPanel.Left,
                centerPoint.Y - arrowPanel.Height / 2 + characterPanel.Top
            );

            this.Controls.Add(arrowPanel);
            arrowPanel.BringToFront();
            
            // Add mouse move handler and paint handler
            this.MouseMove += SelectCharacterForm_MouseMove;
            arrowPanel.Paint += ArrowPanel_Paint;

            // Add arrow panel to refresh timer
            Timer refreshTimer = new Timer
            {
                Interval = 16 // 60fps
            };
            refreshTimer.Tick += (s, e) => arrowPanel.Invalidate();
            refreshTimer.Start();

            // Initialize animations
            try
            {
                InitializeAnimations();

                Timer characterRefreshTimer = new Timer
                {
                    Interval = 50
                };
                characterRefreshTimer.Tick += (s, e) => {
                    foreach (Control control in characterPanel.Controls)
                    {
                        control.Invalidate();
                    }
                };
                characterRefreshTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animations: {ex.Message}");
            }

            // Add labels for all characters
            AddCharacterLabel(ninjaPanel, "Ninja");
            AddCharacterLabel(puppeteerPanel, "Puppeteer");
            AddCharacterLabel(samuraiPanel, "Samurai");
            AddCharacterLabel(scarecrowPanel, "Scarecrow");
            AddCharacterLabel(shamanPanel, "Shaman");
        }

        private Panel CreateCharacterPanel(Point location, int size)
        {
            return new Panel
            {
                Size = new Size(size, size),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            // Removed padding since labels are now outside
        }

        private void InitializeAnimations()
        {
            ninjaAnimation = new SpriteAnimation(LoadCharacterImage("Ninja_Idle.png"), 25, 25, 9, 100);
            puppeteerAnimation = new SpriteAnimation(LoadCharacterImage("Puppeteer_Idle.png"), 25, 50, 4, 100);
            samuraiAnimation = new SpriteAnimation(LoadCharacterImage("Samurai_Idle.png"), 50, 50, 4, 100);
            scarecrowAnimation = new SpriteAnimation(LoadCharacterImage("Scarecrow_Idle.png"), 25, 25, 4, 100);
            shamanAnimation = new SpriteAnimation(LoadCharacterImage("Shaman_Idle.png"), 25, 50, 4, 100);

            // Wire up paint events
            ninjaPanel.Paint += (s, e) => DrawCharacter(e.Graphics, ninjaAnimation, 25, 25, ninjaPanel);
            puppeteerPanel.Paint += (s, e) => DrawCharacter(e.Graphics, puppeteerAnimation, 25, 50, puppeteerPanel);
            samuraiPanel.Paint += (s, e) => DrawCharacter(e.Graphics, samuraiAnimation, 50, 50, samuraiPanel);
            scarecrowPanel.Paint += (s, e) => DrawCharacter(e.Graphics, scarecrowAnimation, 25, 25, scarecrowPanel);
            shamanPanel.Paint += (s, e) => DrawCharacter(e.Graphics, shamanAnimation, 25, 50, shamanPanel);

            // Wire up click events
            ninjaPanel.Click += (s, e) => SelectCharacter("Ninja");
            puppeteerPanel.Click += (s, e) => SelectCharacter("Puppeteer");
            samuraiPanel.Click += (s, e) => SelectCharacter("Samurai");
            scarecrowPanel.Click += (s, e) => SelectCharacter("Scarecrow");
            shamanPanel.Click += (s, e) => SelectCharacter("Shaman");

            // Start all animations
            ninjaAnimation.Start();
            puppeteerAnimation.Start();
            samuraiAnimation.Start();
            scarecrowAnimation.Start();
            shamanAnimation.Start();
        }

        private void DrawCharacter(Graphics g, SpriteAnimation animation, int width, int height, Panel panel)
        {
            if (animation != null)
            {
                int scaleFactor = Math.Min(panel.Width / width, panel.Height / height);
                int scaledWidth = width * scaleFactor;
                int scaledHeight = height * scaleFactor;
                int x = (panel.Width - scaledWidth) / 2;
                int y = (panel.Height - scaledHeight) / 2;

                animation.DrawFrame(g, new Rectangle(x, y, scaledWidth, scaledHeight));
            }
        }

        private Image LoadCharacterImage(string filename) =>
            Image.FromFile(Path.Combine(CHARACTERS_PATH, filename));

        private void AddCharacterLabel(Panel panel, string text)
        {
            Label label = new Label
            {
                Text = text,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,  // Make background transparent
                Font = new Font("Arial", 12, FontStyle.Bold),  // Slightly larger font
                ForeColor = Color.Black  // Black text color
            };

            // Calculate position to be below the panel
            int margin = 10;
            label.Location = new Point(
                panel.Left + (panel.Width - label.PreferredWidth) / 2,
                panel.Bottom + margin
            );

            // Create a shadow effect for better visibility
            Label shadowLabel = new Label
            {
                Text = text,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(
                    label.Location.X + 1,
                    label.Location.Y + 1
                )
            };

            // Add labels to character panel
            characterPanel.Controls.Add(shadowLabel);
            characterPanel.Controls.Add(label);
            
            // Bring labels to front
            shadowLabel.BringToFront();
            label.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            ninjaAnimation?.Dispose();
            puppeteerAnimation?.Dispose();
            samuraiAnimation?.Dispose();
            scarecrowAnimation?.Dispose();
            shamanAnimation?.Dispose();
            arrowImage?.Dispose();
        }

        private void SelectCharacter(string character)
        {
            // Reset all borders using the stored panel reference
            foreach (Control control in characterPanel.Controls)
            {
                if (control is Panel)
                    ((Panel)control).BorderStyle = BorderStyle.FixedSingle;
            }

            selectedCharacter = character;
            GameState.SelectedCharacter = character; // Store the selection globally

            // Highlight selected character
            Panel selectedPanel = null;
            switch (character)
            {
                case "Ninja": selectedPanel = ninjaPanel; break;
                case "Puppeteer": selectedPanel = puppeteerPanel; break;
                case "Samurai": selectedPanel = samuraiPanel; break;
                case "Scarecrow": selectedPanel = scarecrowPanel; break;
                case "Shaman": selectedPanel = shamanPanel; break;
            }
            if (selectedPanel != null)
                selectedPanel.BorderStyle = BorderStyle.Fixed3D;

            MessageBox.Show($"{character} selected as your character!");
        }

        private void InitializeControls()
        {
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
            this.Close();
        }

        private void SelectCharacterForm_MouseMove(object sender, MouseEventArgs e)
        {
            arrowPanel.Invalidate();
        }

        private void ArrowPanel_Paint(object sender, PaintEventArgs e)
        {
            if (arrowImage != null)
            {
                Point mousePos = this.PointToClient(Cursor.Position);
                float angle = (float)Math.Atan2(
                    mousePos.Y - (centerPoint.Y + characterPanel.Top),
                    mousePos.X - (centerPoint.X + characterPanel.Left)
                );

                // Save graphics state
                var state = e.Graphics.Save();

                // Set up transformation
                e.Graphics.TranslateTransform(arrowPanel.Width / 2, arrowPanel.Height / 2);
                e.Graphics.RotateTransform((float)(angle * 180 / Math.PI));

                // Draw the arrow
                e.Graphics.DrawImage(arrowImage, 
                    -arrowPanel.Width / 2, 
                    -arrowPanel.Height / 2, 
                    arrowPanel.Width, 
                    arrowPanel.Height);

                // Restore graphics state
                e.Graphics.Restore(state);
            }
        }
    }
}
