using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormNavigation
{
    public class SpriteAnimation
    {
        private Image spriteSheet;
        private int frameWidth;
        private int frameHeight;
        private int currentFrame;
        private int totalFrames;
        private Timer animationTimer;
        private Rectangle sourceRect;

        public SpriteAnimation(Image spriteSheet, int frameWidth, int frameHeight, int totalFrames, int interval = 100)
        {
            this.spriteSheet = spriteSheet;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.totalFrames = totalFrames;
            this.currentFrame = 0;
            this.sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);

            animationTimer = new Timer();
            animationTimer.Interval = interval; // Use custom interval
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            currentFrame = (currentFrame + 1) % totalFrames;
            sourceRect.X = currentFrame * frameWidth;
        }

        public void Start()
        {
            animationTimer.Start();
        }

        public void Stop()
        {
            animationTimer.Stop();
        }

        public void DrawFrame(Graphics g, Rectangle destRect)
        {
            // Set pixel-perfect rendering modes
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.AssumeLinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            g.DrawImage(spriteSheet, destRect, sourceRect, GraphicsUnit.Pixel);
        }

        public void Dispose()
        {
            animationTimer.Dispose();
            spriteSheet.Dispose();
        }
    }
}
