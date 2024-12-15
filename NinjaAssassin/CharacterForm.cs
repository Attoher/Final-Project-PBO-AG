using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormNavigation
{
    public class CharacterForm : Form
    {
        private Bitmap characterRight; // Sprite karakter menghadap kanan
        private Bitmap characterLeft;  // Sprite karakter menghadap kiri
        private Point characterPosition; // Posisi karakter di form

        public CharacterForm()
        {
            this.Text = "Sprite Animation Example";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.characterPosition = new Point(400, 300); // Set initial character position

            // Memuat sprite karakter (misal karakter berada pada file PNG)
            characterRight = new Bitmap("character_right.png");
            characterLeft = new Bitmap("character_left.png");

            this.DoubleBuffered = true; // Mengaktifkan double buffering untuk mencegah flickering
            this.MouseMove += CharacterForm_MouseMove;
        }

        // Event handler untuk menggerakkan karakter berdasarkan posisi mouse
        private void CharacterForm_MouseMove(object sender, MouseEventArgs e)
        {
            this.Invalidate(); // Meminta agar form digambar ulang
        }

        // Event handler untuk menggambar sprite karakter pada form
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Tentukan sprite yang akan digambar berdasarkan posisi mouse
            if (MousePosition.X > this.Left + characterPosition.X)
            {
                // Jika mouse di sebelah kanan karakter, gambar karakter menghadap kanan
                g.DrawImage(characterRight, characterPosition);
            }
            else
            {
                // Jika mouse di sebelah kiri karakter, gambar karakter menghadap kiri
                g.DrawImage(characterLeft, characterPosition);
            }
        }
    }
}
