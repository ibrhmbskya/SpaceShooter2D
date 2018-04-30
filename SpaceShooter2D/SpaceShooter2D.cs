using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpaceShooter2D
{
    public partial class SpaceShooter2D : Form
    {
        Timer gameTime;
        Timer düşmanTime;
        Timer hızlıDüşmanTime;

        List<Lazer> lazerList; // ekrandaki lazerleri tutan liste
        List<GüdümlüLazer> güdümlüLazerList; // ekrandaki güdümlü lazerleri tutan liste
        List<Düşman> düşmanList; // ekrandaki düşmanları tutan liste

        int SCREEN_WIDTH;
        int SCREEN_HEIGHT;

        // düşmanların ekrana gireceği random yönler için değişkenler
        Random randomYön;
        Random randomÇıkış;

        int gameTimeInterval; // ekranın refresh aralığı (frame rate)
        int düşmanTimeInterval; // kaç saniyede bir ekrana düşman gireceğinin zamanı
        int hızlıDüşmanTimeInterval; // kaç saniyede bir ekrana hızlı düşman gireceğinin zamanı

        Oyuncu oyuncu1;
        Hedef hedef;

        Label skorLabel;

        Label güdümlüLazerLabel;

        PictureBox backGroundPictureBox;

        PictureBox mainMenuPictureBox;
        PictureBox logoPictureBox;

        PictureBox controlsPictureBox;

        PictureBox difficultyPictureBox;

        PictureBox grafikAyarlarıPictureBox;

        // Laptop'ların "güç tasarrufu" modlarında oyun çok yavaş çalıştığından dolayı,
        // grafik seviyesi menüden düşürülerek, oyun normal hızda oynanabilir.
        int grafikSeviyesi; // 0: Düşük, 1: Orta, 2: Yüksek seçenekleri ifade edecek

        bool gamePaused;
        bool gameStarted;

        string cheatCode;

        bool immortal;

        Label immortalLabel;

        Label newGameLabel;
        Label controlsLabel;
        Label displayControlsLabel;
        Label backToMainMenuLabel;
        Label quitLabel;
        Label difficultyLabel;
        Label continueLabel;

        Label easyModeLabel;
        Label normalModeLabel;
        Label hardModeLabel;

        Label grafikAyarlarıLabel;
        Label düşükGrafikLabel;
        Label ortaGrafikLabel;
        Label yüksekGrafikLabel;

        bool easyMode;
        bool normalMode;
        bool hardMode;

        public SpaceShooter2D() // Constructor
        {
            InitializeComponent();

            SCREEN_WIDTH = 800; // ekran genişliği
            SCREEN_HEIGHT = 600; // ekran yüksekliği

            grafikSeviyesi = 2; // Başlangıçta grafikSeviyesi otomatik olarak "yüksek" seçiliyor. Sonra değiştirilebilir

            this.Text = "SpaceShooter2D";
            this.Width = SCREEN_WIDTH; // Formun genişliği ayarlandı
            this.Height = SCREEN_HEIGHT; // Formun yüksekliği ayarlandı
            this.StartPosition = FormStartPosition.CenterScreen; // Form, ekranın ortasında (center) açılacak
            this.BackColor = Color.Black; // Formun zemini siyah yapıldı. Transparan olursa çok yavaşlıyor    
            this.DoubleBuffered = true; // Bu olmazsa backgroundImage ekleyince çok yavaşlıyor           
            this.BackgroundImage = Properties.Resources.Space;

            // Form ekranda "tam ekran" olarak görüntüleniyor, border'ları kaldırılıyor
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            düşmanList = new List<Düşman>();
            lazerList = new List<Lazer>();
            güdümlüLazerList = new List<GüdümlüLazer>();

            randomYön = new Random();
            randomÇıkış = new Random();

            gameTimeInterval = 1000 / 60; // Timer'ın zaman aralığı (saniyede 60 kare oynatacak)
            gameTime = new Timer();
            gameTime.Tick += new EventHandler(gameTime_Tick);

            düşmanTime = new Timer();
            düşmanTime.Tick += new EventHandler(düşmanTimeTick);

            hızlıDüşmanTime = new Timer();
            hızlıDüşmanTime.Tick += new EventHandler(hızlıDüşmanTimeTick);

            this.KeyDown += new KeyEventHandler(playerKeyDown);
            this.KeyUp += new KeyEventHandler(playerKeyUp);

            easyMode = false;
            normalMode = true; // başlangıçta oyun zorluğu "normal" olarak ayarlanıyor
            hardMode = false;

            displayMainMenu();
        }

        public void labelEkle(Label newLabel, string labelString, Color labelColor, Font font, Point location, PictureBox pictureBox)
        {
            newLabel.BackColor = Color.Transparent;
            newLabel.Text = labelString;
            newLabel.ForeColor = labelColor;
            newLabel.Font = font;
            newLabel.AutoSize = true; // Text'in label'a sığması için AutoSize "true" yapıldı
            newLabel.Location = location;
            pictureBox.Controls.Add(newLabel);
        }

        public void tıklanabilirLabelEkle(Label newLabel, string labelString, Color labelColor, Point location, PictureBox pictureBox, EventHandler mouseEnter, EventHandler mouseLeave, MouseEventHandler mouseClick)
        {
            newLabel.BackColor = Color.Transparent;
            newLabel.Text = labelString;
            newLabel.ForeColor = labelColor;
            newLabel.Font = new Font("Arial", 20, FontStyle.Bold);
            newLabel.AutoSize = true; // Text'in label'a sığması için AutoSize "true" yapıldı
            newLabel.Location = location;
            pictureBox.Controls.Add(newLabel);
            newLabel.MouseEnter += mouseEnter;
            newLabel.MouseLeave += mouseLeave;
            newLabel.MouseClick += mouseClick;
        }

        public void hedefMovement()
        {
            Point point = MousePosition;
            hedef.x = point.X - hedef.genişlik / 2 - ((this.Width - SCREEN_WIDTH) / 2);
            hedef.y = point.Y - hedef.yükseklik / 2 - ((this.Height - SCREEN_HEIGHT) / 2);
            point = new Point(hedef.x, hedef.y);
            hedef.pictureBox.Location = point;
        }

        public void displayMainMenu()
        {
            this.Controls.Clear();

            gamePaused = false;
            gameStarted = false;

            düşmanTime.Enabled = false;
            hızlıDüşmanTime.Enabled = false;

            mainMenuPictureBox = new PictureBox();
            mainMenuPictureBox.BackColor = Color.Black;
            mainMenuPictureBox.Image = Properties.Resources.Space;
            mainMenuPictureBox.Size = new Size(SCREEN_WIDTH, SCREEN_HEIGHT);
            mainMenuPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            mainMenuPictureBox.Location = new Point(this.Width / 2 - mainMenuPictureBox.Width / 2, this.Height / 2 - mainMenuPictureBox.Height / 2);
            mainMenuPictureBox.Anchor = AnchorStyles.None;
            this.Controls.Add(mainMenuPictureBox);

            logoPictureBox = new PictureBox();
            logoPictureBox.BackColor = Color.Transparent;
            logoPictureBox.Image = Properties.Resources.SpaceShooter2DLogoTransparent;
            logoPictureBox.Size = new Size(900, 200);
            logoPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            logoPictureBox.Location = new Point(this.Width / 2 - logoPictureBox.Width / 2, 200);
            logoPictureBox.Anchor = AnchorStyles.None;
            this.Controls.Add(logoPictureBox);
            logoPictureBox.BringToFront();

            newGameLabel = new Label();
            Point location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 + 20);
            tıklanabilirLabelEkle(newGameLabel, "Yeni Oyun", Color.WhiteSmoke, location, mainMenuPictureBox, mouseOnNewGameLabel, mouseLeaveNewGameLabel, startNewGame);

            difficultyLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 + 50);
            tıklanabilirLabelEkle(difficultyLabel, "Zorluk", Color.WhiteSmoke, location, mainMenuPictureBox, mouseOnDifficultyLabel, mouseLeaveDifficultyLabel, displayDifficultyScreen);

            controlsLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 + 80);
            tıklanabilirLabelEkle(controlsLabel, "Kontroller", Color.WhiteSmoke, location, mainMenuPictureBox, mouseOnControlsLabel, mouseLeaveControlsLabel, displayControls);

            grafikAyarlarıLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 + 110);
            tıklanabilirLabelEkle(grafikAyarlarıLabel, "Grafik Ayarları", Color.WhiteSmoke, location, mainMenuPictureBox, mouseOnGrafikAyarlarıLabel, mouseLeaveGrafikAyarlarıLabel, grafikAyarlarınıGöster);

            quitLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 + 140);
            tıklanabilirLabelEkle(quitLabel, "Çıkış", Color.WhiteSmoke, location, mainMenuPictureBox, mouseOnQuitLabel, mouseLeaveQuitLabel, quitGame);
        }

        public void mouseOnGrafikAyarlarıLabel(object sender, EventArgs e)
        {
            grafikAyarlarıLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveGrafikAyarlarıLabel(object sender, EventArgs e)
        {
            grafikAyarlarıLabel.ForeColor = Color.WhiteSmoke;
        }

        public void grafikAyarlarınıGöster(object sender, MouseEventArgs e)
        {
            this.Controls.Clear();

            grafikAyarlarıPictureBox = new PictureBox();
            grafikAyarlarıPictureBox.BackColor = Color.Black;
            grafikAyarlarıPictureBox.Image = Properties.Resources.Space;
            grafikAyarlarıPictureBox.Size = new Size(SCREEN_WIDTH, SCREEN_HEIGHT);
            grafikAyarlarıPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            grafikAyarlarıPictureBox.Location = new Point(this.Width / 2 - grafikAyarlarıPictureBox.Width / 2, this.Height / 2 - grafikAyarlarıPictureBox.Height / 2);
            grafikAyarlarıPictureBox.Anchor = AnchorStyles.None;
            this.Controls.Add(grafikAyarlarıPictureBox);

            backToMainMenuLabel = new Label();
            Point location = new Point(5, 5);
            tıklanabilirLabelEkle(backToMainMenuLabel, "Ana Menü", Color.WhiteSmoke, location, grafikAyarlarıPictureBox, mouseOnBackToMainMenuLabel, mouseLeaveBackToMainMenuLabel, returnToMainMenu);

            düşükGrafikLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 - 110);
            tıklanabilirLabelEkle(düşükGrafikLabel, "Düşük", Color.WhiteSmoke, location, grafikAyarlarıPictureBox, mouseOnDüşükGrafikLabel, mouseLeaveDüşükGrafikLabel, chooseDüşükGrafikMode);

            ortaGrafikLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 - 80);
            tıklanabilirLabelEkle(ortaGrafikLabel, "Orta", Color.WhiteSmoke, location, grafikAyarlarıPictureBox, mouseOnOrtaGrafikLabel, mouseLeaveOrtaGrafikLabel, chooseOrtaGrafikMode);

            yüksekGrafikLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 - 50);
            tıklanabilirLabelEkle(yüksekGrafikLabel, "Yüksek", Color.WhiteSmoke, location, grafikAyarlarıPictureBox, mouseLeaveYüksekGrafikLabel, mouseLeaveYüksekGrafikLabel, chooseYüksekGrafikMode);

            switch (grafikSeviyesi)
            {
                case 0:
                    düşükGrafikLabel.ForeColor = Color.Red;
                    ortaGrafikLabel.ForeColor = Color.WhiteSmoke;
                    yüksekGrafikLabel.ForeColor = Color.WhiteSmoke;
                    break;
                case 1:
                    düşükGrafikLabel.ForeColor = Color.WhiteSmoke;
                    ortaGrafikLabel.ForeColor = Color.Red;
                    yüksekGrafikLabel.ForeColor = Color.WhiteSmoke;
                    break;
                case 2:
                    düşükGrafikLabel.ForeColor = Color.WhiteSmoke;
                    ortaGrafikLabel.ForeColor = Color.WhiteSmoke;
                    yüksekGrafikLabel.ForeColor = Color.Red;
                    break;
            }
        }

        public void mouseOnDüşükGrafikLabel(object sender, EventArgs e)
        {
            düşükGrafikLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveDüşükGrafikLabel(object sender, EventArgs e)
        {
            if (grafikSeviyesi == 0)
            {
                düşükGrafikLabel.ForeColor = Color.Red;
            }
            else
            {
                düşükGrafikLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        public void chooseDüşükGrafikMode(object sender, MouseEventArgs e)
        {
            grafikSeviyesi = 0;
            düşükGrafikLabel.ForeColor = Color.Red;
            ortaGrafikLabel.ForeColor = Color.WhiteSmoke;
            yüksekGrafikLabel.ForeColor = Color.WhiteSmoke;
        }

        public void mouseOnOrtaGrafikLabel(object sender, EventArgs e)
        {
            ortaGrafikLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveOrtaGrafikLabel(object sender, EventArgs e)
        {
            if (grafikSeviyesi == 1)
            {
                ortaGrafikLabel.ForeColor = Color.Red;
            }
            else
            {
                ortaGrafikLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        public void chooseOrtaGrafikMode(object sender, MouseEventArgs e)
        {
            grafikSeviyesi = 1;
            düşükGrafikLabel.ForeColor = Color.WhiteSmoke;
            ortaGrafikLabel.ForeColor = Color.Red;
            yüksekGrafikLabel.ForeColor = Color.WhiteSmoke;
        }

        public void mouseOnYüksekGrafikLabel(object sender, EventArgs e)
        {
            yüksekGrafikLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveYüksekGrafikLabel(object sender, EventArgs e)
        {
            if (grafikSeviyesi == 2)
            {
                yüksekGrafikLabel.ForeColor = Color.Red;
            }
            else
            {
                yüksekGrafikLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        public void chooseYüksekGrafikMode(object sender, MouseEventArgs e)
        {
            grafikSeviyesi = 2;
            düşükGrafikLabel.ForeColor = Color.WhiteSmoke;
            ortaGrafikLabel.ForeColor = Color.WhiteSmoke;
            yüksekGrafikLabel.ForeColor = Color.Red;
        }

        public void mouseOnNewGameLabel(object sender, EventArgs e)
        {
            newGameLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveNewGameLabel(object sender, EventArgs e)
        {
            newGameLabel.ForeColor = Color.WhiteSmoke;
        }

        public void startNewGame(object sender, MouseEventArgs e)
        {
            gameStarted = true;

            this.Controls.Clear();
            düşmanList.Clear();
            lazerList.Clear();
            güdümlüLazerList.Clear();

            oyuncu1 = new Oyuncu(SCREEN_WIDTH, SCREEN_HEIGHT, grafikSeviyesi);
            hedef = new Hedef(grafikSeviyesi);

            gamePaused = false;

            cheatCode = "";

            immortal = false;

            oyuncu1.skor = 0;

            backGroundPictureBox = new PictureBox();
            backGroundPictureBox.BackColor = Color.Black;
            if (grafikSeviyesi == 2 || grafikSeviyesi == 1)
            {
                backGroundPictureBox.Image = Properties.Resources.SpaceGif;
            }
            else
            {
                backGroundPictureBox.Image = Properties.Resources.PauseScreen;
            }
            backGroundPictureBox.Size = new Size(SCREEN_WIDTH, SCREEN_HEIGHT);
            backGroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            backGroundPictureBox.Location = new Point(this.Width / 2 - mainMenuPictureBox.Width / 2, this.Height / 2 - mainMenuPictureBox.Height / 2);
            backGroundPictureBox.Anchor = AnchorStyles.None;
            backGroundPictureBox.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(backGroundPictureBox);

            Cursor.Hide(); // oyun başlayınca mouse gizleniyor       

            gameTime.Interval = gameTimeInterval; // Timer'ın zaman aralığı ayarlandı
            gameTime.Enabled = true;

            if (easyMode)
            {
                düşmanTimeInterval = 1500;
                hızlıDüşmanTimeInterval = 5000;
                oyuncu1.güdümlüLazer = 3;
            }
            else if (normalMode)
            {
                düşmanTimeInterval = 1000;
                hızlıDüşmanTimeInterval = 4000;
                oyuncu1.güdümlüLazer = 0;
            }
            else
            {
                düşmanTimeInterval = 700;
                hızlıDüşmanTimeInterval = 2000;
                oyuncu1.güdümlüLazer = 0;
            }


            düşmanTime.Interval = düşmanTimeInterval; // Timer'ın zaman aralığı ayarlandı
            düşmanTime.Enabled = true;

            hızlıDüşmanTime.Interval = hızlıDüşmanTimeInterval; // Timer'ın zaman aralığı ayarlandı
            hızlıDüşmanTime.Enabled = true;

            backGroundPictureBox.MouseUp += new MouseEventHandler(lazerFire);

            backGroundPictureBox.Controls.Add(oyuncu1.pictureBox);

            hedef.pictureBox.MouseUp += new MouseEventHandler(lazerFire);
            backGroundPictureBox.Controls.Add(hedef.pictureBox);
            hedef.pictureBox.BringToFront();

            Point point = this.PointToClient(Cursor.Position);
            hedef.x = point.X - hedef.genişlik / 2;
            hedef.y = point.Y - hedef.yükseklik / 2;
            point = new Point(hedef.x, hedef.y);
            hedef.pictureBox.Location = point;

            skorLabel = new Label();
            Point location = new Point(330, 10);
            Font font = new Font("Arial", 20, FontStyle.Bold);
            labelEkle(skorLabel, "Skor: " + oyuncu1.skor, Color.White, font, location, backGroundPictureBox);

            güdümlüLazerLabel = new Label();
            location = new Point(620, 10);
            font = new Font("Arial", 12, FontStyle.Bold);
            labelEkle(güdümlüLazerLabel, "Güdümlü Lazer: " + oyuncu1.güdümlüLazer, Color.Turquoise, font, location, backGroundPictureBox);

            immortalLabel = new Label();
            location = new Point(20, 10);
            font = new Font("Arial", 12, FontStyle.Bold);
            labelEkle(immortalLabel, "Ölümsüzlük Modu!", Color.Red, font, location, backGroundPictureBox);
            immortalLabel.Visible = false;
        }

        public void mouseOnControlsLabel(object sender, EventArgs e)
        {
            controlsLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveControlsLabel(object sender, EventArgs e)
        {
            controlsLabel.ForeColor = Color.WhiteSmoke;
        }

        public void displayControls(object sender, MouseEventArgs e)
        {
            this.Controls.Clear();

            controlsPictureBox = new PictureBox();
            controlsPictureBox.BackColor = Color.Black;
            controlsPictureBox.Image = Properties.Resources.Space;
            controlsPictureBox.Size = new Size(SCREEN_WIDTH, SCREEN_HEIGHT);
            controlsPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            controlsPictureBox.Location = new Point(this.Width / 2 - controlsPictureBox.Width / 2, this.Height / 2 - controlsPictureBox.Height / 2);
            controlsPictureBox.Anchor = AnchorStyles.None;
            this.Controls.Add(controlsPictureBox);

            displayControlsLabel = new Label();
            Point location = new Point(SCREEN_WIDTH / 2 - 100, SCREEN_HEIGHT / 2 - 130);
            Font font = new Font("Arial", 20, FontStyle.Bold);
            string labelString = "Yukarı: W \r\nAşağı: S \r\nSol: A \r\nSağ: D \r\nAteş: Sol Mouse Tuşu \r\nÖzel Ateş: Sağ Mouse Tuşu";
            labelEkle(displayControlsLabel, labelString, Color.WhiteSmoke, font, location, controlsPictureBox);

            backToMainMenuLabel = new Label();
            location = new Point(5, 5);
            tıklanabilirLabelEkle(backToMainMenuLabel, "Ana Menü", Color.WhiteSmoke, location, controlsPictureBox, mouseOnBackToMainMenuLabel, mouseLeaveBackToMainMenuLabel, returnToMainMenu);
        }

        public void mouseOnBackToMainMenuLabel(object sender, EventArgs e)
        {
            backToMainMenuLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveBackToMainMenuLabel(object sender, EventArgs e)
        {
            backToMainMenuLabel.ForeColor = Color.WhiteSmoke;
        }

        public void returnToMainMenu(object sender, MouseEventArgs e)
        {
            displayMainMenu();
        }

        public void mouseOnDifficultyLabel(object sender, EventArgs e)
        {
            difficultyLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveDifficultyLabel(object sender, EventArgs e)
        {
            difficultyLabel.ForeColor = Color.WhiteSmoke;
        }

        public void displayDifficultyScreen(object sender, EventArgs e)
        {
            this.Controls.Clear();

            difficultyPictureBox = new PictureBox();
            difficultyPictureBox.BackColor = Color.Black;
            difficultyPictureBox.Image = Properties.Resources.Space;
            difficultyPictureBox.Size = new Size(SCREEN_WIDTH, SCREEN_HEIGHT);
            difficultyPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            difficultyPictureBox.Location = new Point(this.Width / 2 - difficultyPictureBox.Width / 2, this.Height / 2 - difficultyPictureBox.Height / 2);
            difficultyPictureBox.Anchor = AnchorStyles.None;
            this.Controls.Add(difficultyPictureBox);

            backToMainMenuLabel = new Label();
            Point location = new Point(5, 5);
            tıklanabilirLabelEkle(backToMainMenuLabel, "Ana Menü", Color.WhiteSmoke, location, difficultyPictureBox, mouseOnBackToMainMenuLabel, mouseLeaveBackToMainMenuLabel, returnToMainMenu);

            easyModeLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 - 110);
            tıklanabilirLabelEkle(easyModeLabel, "Kolay", Color.WhiteSmoke, location, difficultyPictureBox, mouseOnEasyLabel, mouseLeaveEasyLabel, chooseEasyMode);

            normalModeLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 - 80);
            tıklanabilirLabelEkle(normalModeLabel, "Normal", Color.WhiteSmoke, location, difficultyPictureBox, mouseOnNormalLabel, mouseLeaveNormalLabel, chooseNormalMode);

            hardModeLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 60, SCREEN_HEIGHT / 2 - 50);
            tıklanabilirLabelEkle(hardModeLabel, "Zor", Color.WhiteSmoke, location, difficultyPictureBox, mouseOnHardLabel, mouseLeaveHardLabel, chooseHardMode);

            if (easyMode)
            {
                easyModeLabel.ForeColor = Color.Red;
            }
            else if (normalMode)
            {
                normalModeLabel.ForeColor = Color.Red;
            }
            else
            {
                hardModeLabel.ForeColor = Color.Red;
            }
        }

        public void mouseOnHardLabel(object sender, EventArgs e)
        {
            hardModeLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveHardLabel(object sender, EventArgs e)
        {
            if (hardMode)
            {
                hardModeLabel.ForeColor = Color.Red;
            }
            else
            {
                hardModeLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        public void chooseHardMode(object sender, MouseEventArgs e)
        {
            hardModeLabel.ForeColor = Color.Red;
            hardMode = true;
            easyMode = false;
            easyModeLabel.ForeColor = Color.WhiteSmoke;
            normalMode = false;
            normalModeLabel.ForeColor = Color.WhiteSmoke;
        }

        public void mouseOnNormalLabel(object sender, EventArgs e)
        {
            normalModeLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveNormalLabel(object sender, EventArgs e)
        {
            if (normalMode)
            {
                normalModeLabel.ForeColor = Color.Red;
            }
            else
            {
                normalModeLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        public void chooseNormalMode(object sender, MouseEventArgs e)
        {
            normalModeLabel.ForeColor = Color.Red;
            normalMode = true;
            easyMode = false;
            easyModeLabel.ForeColor = Color.WhiteSmoke;
            hardMode = false;
            hardModeLabel.ForeColor = Color.WhiteSmoke;
        }

        public void mouseOnEasyLabel(object sender, EventArgs e)
        {
            easyModeLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveEasyLabel(object sender, EventArgs e)
        {
            if (easyMode)
            {
                easyModeLabel.ForeColor = Color.Red;
            }
            else
            {
                easyModeLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        public void chooseEasyMode(object sender, MouseEventArgs e)
        {
            easyModeLabel.ForeColor = Color.Red;
            easyMode = true;
            normalMode = false;
            normalModeLabel.ForeColor = Color.WhiteSmoke;
            hardMode = false;
            hardModeLabel.ForeColor = Color.White;
        }

        public void mouseOnQuitLabel(object sender, EventArgs e)
        {
            quitLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveQuitLabel(object sender, EventArgs e)
        {
            quitLabel.ForeColor = Color.WhiteSmoke;
        }

        public void quitGame(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }

        public void pauseGame()
        {
            backGroundPictureBox.Image = Properties.Resources.PauseScreen;
            gamePaused = true;

            gameTime.Enabled = false;
            düşmanTime.Enabled = false;
            Cursor.Show();

            backToMainMenuLabel = new Label();
            Point location = new Point(5, 5);
            tıklanabilirLabelEkle(backToMainMenuLabel, "Ana Menü", Color.WhiteSmoke, location, backGroundPictureBox, mouseOnBackToMainMenuLabel, mouseLeaveBackToMainMenuLabel, returnToMainMenu);
            backToMainMenuLabel.BringToFront();

            continueLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 - 40);
            tıklanabilirLabelEkle(continueLabel, "Devam", Color.WhiteSmoke, location, backGroundPictureBox, mouseOnContinueLabel, mouseLeaveContinueLabel, clickContinue);
            continueLabel.BringToFront();

            quitLabel = new Label();
            location = new Point(SCREEN_WIDTH / 2 - 70, SCREEN_HEIGHT / 2 - 10);
            tıklanabilirLabelEkle(quitLabel, "Çıkış", Color.WhiteSmoke, location, backGroundPictureBox, mouseOnQuitLabel, mouseLeaveQuitLabel, quitGame);
            quitLabel.BringToFront();
        }

        public void mouseOnContinueLabel(object sender, EventArgs e)
        {
            continueLabel.ForeColor = Color.Yellow;
        }

        public void mouseLeaveContinueLabel(object sender, EventArgs e)
        {
            continueLabel.ForeColor = Color.WhiteSmoke;
        }

        public void clickContinue(object sender, MouseEventArgs e)
        {
            continueGame();
        }

        public void continueGame()
        {
            gamePaused = false;
            backGroundPictureBox.Image = Properties.Resources.SpaceGif;

            gameTime.Enabled = true;
            düşmanTime.Enabled = true;
            Cursor.Hide();

            backToMainMenuLabel.Dispose();
            continueLabel.Dispose();
            quitLabel.Dispose();
        }

        public void lazerFire(object sender, MouseEventArgs e)
        {
            if (gameStarted && !gamePaused)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Lazer lazer = new Lazer(grafikSeviyesi);

                    lazer.x = oyuncu1.x + oyuncu1.genişlik / 2 - lazer.genişlik / 2;
                    lazer.y = oyuncu1.y + oyuncu1.yükseklik / 2 - lazer.yükseklik / 2;
                    lazer.pictureBox.Location = new Point((int)lazer.x, (int)lazer.y);
                    int x = hedef.x - lazer.pictureBox.Location.X;
                    int y = hedef.y - lazer.pictureBox.Location.Y;
                    double mesafe = Math.Sqrt(x * x + y * y);
                    lazer.speedX = lazer.speedMain * (x / mesafe);
                    lazer.speedY = lazer.speedMain * (y / mesafe);
                    lazerList.Add(lazer);
                    backGroundPictureBox.Controls.Add(lazer.pictureBox);
                    return;
                }

                if (e.Button == MouseButtons.Right && oyuncu1.güdümlüLazer > 0)
                {
                    GüdümlüLazer güdümlüLazer = new GüdümlüLazer(grafikSeviyesi);
                    güdümlüLazer.enYakınDüşmanıBul(düşmanList, hedef); // hedef'e en yakın olan düşman bulunacak
                    if (güdümlüLazer.kilitlenilenDüşman != null) // bir düşmana kilitlenildiyse güdümlüLazer oluşturulacak
                    {
                        güdümlüLazer.x = oyuncu1.x + oyuncu1.genişlik / 2 - güdümlüLazer.genişlik / 2; // Lazer oyuncunun bulunduğu yerden ateşleniyor
                        güdümlüLazer.y = oyuncu1.y + oyuncu1.yükseklik / 2 - güdümlüLazer.yükseklik / 2; // Lazer oyuncunun bulunduğu yerden ateşleniyor
                        güdümlüLazer.pictureBox.Location = new Point((int)güdümlüLazer.x, (int)güdümlüLazer.y);
                        güdümlüLazerList.Add(güdümlüLazer);
                        backGroundPictureBox.Controls.Add(güdümlüLazer.pictureBox);
                        oyuncu1.güdümlüLazer--;
                        güdümlüLazerLabel.Text = "Güdümlü Lazer: " + oyuncu1.güdümlüLazer;
                        return;
                    }
                    else
                    {
                        güdümlüLazer.pictureBox = new PictureBox();
                        güdümlüLazer.pictureBox.Size = güdümlüLazer.size;
                        güdümlüLazer.pictureBox.BackColor = güdümlüLazer.color;
                        güdümlüLazer.pictureBox.Image = Properties.Resources.GüdümlüLazer;
                        güdümlüLazer.pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        güdümlüLazer.x = oyuncu1.x + oyuncu1.genişlik / 2 - güdümlüLazer.genişlik / 2; // Lazer oyuncunun bulunduğu yerden ateşleniyor
                        güdümlüLazer.y = oyuncu1.y + oyuncu1.yükseklik / 2 - güdümlüLazer.yükseklik / 2; // Lazer oyuncunun bulunduğu yerden ateşleniyor
                        güdümlüLazer.pictureBox.Location = new Point((int)güdümlüLazer.x, (int)güdümlüLazer.y);
                        int x = hedef.x - güdümlüLazer.pictureBox.Location.X;
                        int y = hedef.y - güdümlüLazer.pictureBox.Location.Y;
                        double mesafe = Math.Sqrt(x * x + y * y);
                        güdümlüLazer.speedX = güdümlüLazer.speedMain * (x / mesafe);
                        güdümlüLazer.speedY = güdümlüLazer.speedMain * (y / mesafe);
                        güdümlüLazerList.Add(güdümlüLazer);
                        backGroundPictureBox.Controls.Add(güdümlüLazer.pictureBox);
                    }
                }
            }
        }

        public void LazerMovement()
        {
            if (lazerList.Count != 0)
            {
                for (int i = 0; i < lazerList.Count; i++)
                {
                    Lazer lazer = lazerList[i];
                    lazer.x += lazer.speedX;
                    lazer.y += lazer.speedY;
                    lazer.pictureBox.Location = new Point((int)lazer.x, (int)lazer.y);

                    if (lazer.x <= 0 || lazer.x + lazer.genişlik >= SCREEN_WIDTH || lazer.y <= 0 || lazer.y + lazer.yükseklik >= SCREEN_HEIGHT)
                    {
                        lazer.pictureBox.Dispose(); // lazer ekrandan çıkınca yokediliyor
                        lazerList.Remove(lazer);
                    }
                }
            }
        }

        public void güdümlüLazerHareket()
        {
            if (güdümlüLazerList.Count > 0)
            {
                for (int i = 0; i < güdümlüLazerList.Count; i++)
                {
                    GüdümlüLazer güdümlüLazer = güdümlüLazerList[i];

                    if (güdümlüLazer.kilitlenilenDüşman != null)
                    {
                        if (güdümlüLazer.kilitlenilenDüşman.pictureBox.IsDisposed)
                        {
                            if (düşmanList.Count > 0)
                            {
                                güdümlüLazer.enYakınDüşmanıBul(düşmanList, hedef); // eğer kilitlenilen düşman, güdümlüLazer onu vurmadan yok olduysa, başka bir düşmana kilitlenilecek
                            }
                            else // ekranda düşman kalmazsa, yeni düşman gelene kadar güdümlüLazer yavaşça ilerleyecek
                            {
                                güdümlüLazer.x += güdümlüLazer.speedX / 5;
                                güdümlüLazer.y += güdümlüLazer.speedY / 5;
                                güdümlüLazer.pictureBox.Location = new Point((int)güdümlüLazer.x, (int)güdümlüLazer.y);
                            }
                        }
                        else
                        {
                            int x = (int)güdümlüLazer.kilitlenilenDüşman.x - güdümlüLazer.pictureBox.Location.X;
                            int y = (int)güdümlüLazer.kilitlenilenDüşman.y - güdümlüLazer.pictureBox.Location.Y;
                            double mesafe = Math.Sqrt(x * x + y * y);
                            güdümlüLazer.speedX = güdümlüLazer.speedMain * (x / mesafe);
                            güdümlüLazer.speedY = güdümlüLazer.speedMain * (y / mesafe);

                            güdümlüLazer.x += güdümlüLazer.speedX;
                            güdümlüLazer.y += güdümlüLazer.speedY;
                            güdümlüLazer.pictureBox.Location = new Point((int)güdümlüLazer.x, (int)güdümlüLazer.y);

                            for (int k = 0; k < düşmanList.Count; k++)
                            {
                                Düşman düşman = düşmanList[k];
                                for (int j = 0; j < güdümlüLazerList.Count; j++)
                                {
                                    GüdümlüLazer güdümlüLazer2 = güdümlüLazerList[j];
                                    if (güdümlüLazer2.x + güdümlüLazer2.genişlik >= düşman.x && güdümlüLazer2.x <= düşman.x + düşman.genişlik && güdümlüLazer2.y <= düşman.y + düşman.yükseklik && güdümlüLazer2.y + güdümlüLazer2.yükseklik >= düşman.y)
                                    {
                                        düşman.pictureBox.Dispose(); // lazer değerse düşman PictureBox'u yokedilecek.
                                        güdümlüLazer2.pictureBox.Dispose();
                                        güdümlüLazerList.Remove(güdümlüLazer2);
                                        düşmanList.Remove(düşman);
                                        oyuncu1.skor += 10;
                                        skorLabel.Text = "Skor: " + oyuncu1.skor;

                                        düşman.enemyDestroyed(backGroundPictureBox);

                                        if (oyuncu1.skor % 50 == 0) // her 50 puanda bir, oyuncuya 1 güdümlüLazer veriliyor
                                        {
                                            oyuncu1.güdümlüLazer++;
                                            güdümlüLazerLabel.Text = "Güdümlü Lazer: " + oyuncu1.güdümlüLazer;
                                        }

                                        break; // sıradaki düşmana geçiliyor
                                    }

                                    if (güdümlüLazerList.Count == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (düşmanList.Count > 0)
                        {
                            güdümlüLazer.enYakınDüşmanıBul(düşmanList, hedef);
                        }
                        else
                        {
                            güdümlüLazer.x += güdümlüLazer.speedX / 5;
                            güdümlüLazer.y += güdümlüLazer.speedY / 5;
                            güdümlüLazer.pictureBox.Location = new Point((int)güdümlüLazer.x, (int)güdümlüLazer.y);
                        }

                    }
                }
            }
        }

        void gameTime_Tick(object sender, EventArgs e)
        {
            playerMovement();
            LazerMovement();
            düşmanMovement();
            güdümlüLazerHareket();
            hedefMovement();
        }

        void düşmanTimeTick(object sender, EventArgs e)
        {
            Düşman yeniDüşman = new Düşman(grafikSeviyesi);

            int yön = randomYön.Next(16) % 4; // 0,1,2 veya 3 gelecek buradan        
            int x = 0;
            int y = 0;
            switch (yön)
            {
                case 0: // üstten gelecek
                    y = -yeniDüşman.yükseklik;
                    x = randomÇıkış.Next(SCREEN_WIDTH);
                    break;
                case 1: // alttan gelecek
                    y = SCREEN_HEIGHT;
                    x = randomÇıkış.Next(SCREEN_WIDTH);
                    break;
                case 2: // soldan gelecek
                    y = randomÇıkış.Next(SCREEN_HEIGHT);
                    x = 0 - yeniDüşman.genişlik;
                    break;
                case 3: // sagdan gelecek
                    y = randomÇıkış.Next(SCREEN_HEIGHT);
                    x = SCREEN_WIDTH;
                    break;
            }
            yeniDüşman.x = x;
            yeniDüşman.y = y;
            yeniDüşman.pictureBox.Location = new Point(x, y);
            backGroundPictureBox.Controls.Add(yeniDüşman.pictureBox);
            düşmanList.Add(yeniDüşman);
        }

        void hızlıDüşmanTimeTick(object sender, EventArgs e)
        {
            if (oyuncu1.skor > 150)
            {
                Düşman yeniDüşman = new Düşman(grafikSeviyesi);
                switch (grafikSeviyesi)
                {
                    case 0:
                        yeniDüşman.pictureBox.BackColor = Color.Black;
                        yeniDüşman.pictureBox.Image = Properties.Resources.HızlıDüşman;
                        break;
                    case 1:
                        yeniDüşman.pictureBox.BackColor = Color.Black;
                        yeniDüşman.pictureBox.Image = Properties.Resources.HızlıDüşman;
                        break;
                    case 2:
                        yeniDüşman.pictureBox.BackColor = Color.Transparent;
                        yeniDüşman.pictureBox.Image = Properties.Resources.HızlıDüşmanTransparent;
                        break;
                }
                yeniDüşman.speedMain = 8;
                int yön = randomYön.Next(16) % 4; // 0,1,2 veya 3 gelecek buradan        
                int x = 0;
                int y = 0;
                switch (yön)
                {
                    case 0: // üstten gelecek
                        y = -yeniDüşman.yükseklik;
                        x = randomÇıkış.Next(SCREEN_WIDTH);
                        break;
                    case 1: // alttan gelecek
                        y = SCREEN_HEIGHT;
                        x = randomÇıkış.Next(SCREEN_WIDTH);
                        break;
                    case 2: // soldan gelecek
                        y = randomÇıkış.Next(SCREEN_HEIGHT);
                        x = 0 - yeniDüşman.genişlik;
                        break;
                    case 3: // sagdan gelecek
                        y = randomÇıkış.Next(SCREEN_HEIGHT);
                        x = SCREEN_WIDTH;
                        break;
                }
                yeniDüşman.x = x;
                yeniDüşman.y = y;
                yeniDüşman.pictureBox.Location = new Point(x, y);
                backGroundPictureBox.Controls.Add(yeniDüşman.pictureBox);
                düşmanList.Add(yeniDüşman);
            }
        }

        private void düşmanMovement()
        {
            if (düşmanList.Count != 0)
            {
                for (int i = 0; i < düşmanList.Count; i++)
                {
                    Düşman düşman = düşmanList[i];
                    int x = oyuncu1.x - düşman.pictureBox.Location.X;
                    int y = oyuncu1.y - düşman.pictureBox.Location.Y;
                    double mesafe = Math.Sqrt(x * x + y * y);
                    düşman.speedX = düşman.speedMain * (x / mesafe);
                    düşman.speedY = düşman.speedMain * (y / mesafe);

                    düşman.x += düşman.speedX;
                    düşman.y += düşman.speedY;
                    düşman.pictureBox.Location = new Point((int)düşman.x, (int)düşman.y);

                    for (int j = 0; j < lazerList.Count; j++)
                    {
                        Lazer lazer = lazerList[j];
                        if (lazer.x + lazer.genişlik >= düşman.x && lazer.x <= düşman.x + düşman.genişlik && lazer.y <= düşman.y + düşman.yükseklik && lazer.y + lazer.yükseklik >= düşman.y)
                        {
                            düşman.pictureBox.Dispose(); // lazer değerse düşman PictureBox'u yokedilecek.
                            lazer.pictureBox.Dispose();
                            lazerList.Remove(lazer);
                            düşmanList.Remove(düşman);
                            oyuncu1.skor += 10;
                            skorLabel.Text = "Skor: " + oyuncu1.skor;

                            düşman.enemyDestroyed(backGroundPictureBox);

                            if (oyuncu1.skor % 50 == 0) // her 50 puanda bir, oyuncuya 1 güdümlüLazer veriliyor
                            {
                                oyuncu1.güdümlüLazer++;
                                güdümlüLazerLabel.Text = "Güdümlü Lazer: " + oyuncu1.güdümlüLazer;
                            }

                            break; // sıradaki düşmana geçiliyor
                        }
                    }

                    if (oyuncu1.x + oyuncu1.genişlik >= düşman.x && oyuncu1.x <= düşman.x + düşman.genişlik && oyuncu1.y <= düşman.y + düşman.yükseklik && oyuncu1.y + oyuncu1.yükseklik >= düşman.y)
                    {
                        if (!immortal)
                        {
                            gameTime.Stop();
                            düşmanTime.Stop();
                            lazerList.Clear();
                            düşmanList.Clear();
                            backGroundPictureBox.Controls.Clear();
                            backGroundPictureBox.Image = Properties.Resources.PauseScreen;

                            Label gameOverLabel = new Label();
                            Point location = new Point(SCREEN_WIDTH / 2 - gameOverLabel.Width / 2, 250);
                            Font font = new Font("Arial", 20);
                            labelEkle(gameOverLabel, "Oyun Bitti\n\rSkor: " + oyuncu1.skor, Color.White, font, location, backGroundPictureBox);
                            gameOverLabel.BringToFront(); // Label'ın, diğer PictureBox'ların önünde yer alması için bu eklendi

                            backToMainMenuLabel = new Label();
                            location = new Point(5, 5);
                            tıklanabilirLabelEkle(backToMainMenuLabel, "Ana Menü", Color.WhiteSmoke, location, backGroundPictureBox, mouseOnBackToMainMenuLabel, mouseLeaveBackToMainMenuLabel, returnToMainMenu);
                            backToMainMenuLabel.BringToFront();

                            gameStarted = false;

                            Cursor.Show(); // oyun bitince mouse yeniden gösteriliyor
                        }
                        else
                        {
                            düşman.pictureBox.Dispose(); // lazer değerse düşman PictureBox'u yokedilecek.
                            düşmanList.Remove(düşman);
                            oyuncu1.skor += 10;
                            skorLabel.Text = "Skor: " + oyuncu1.skor;

                            düşman.enemyDestroyed(backGroundPictureBox);

                            if (oyuncu1.skor % 50 == 0) // her 50 puanda bir, oyuncuya 1 güdümlüLazer veriliyor
                            {
                                oyuncu1.güdümlüLazer++;
                                güdümlüLazerLabel.Text = "Güdümlü Lazer: " + oyuncu1.güdümlüLazer;
                            }
                            break; // sıradaki düşmana geçiliyor
                        }
                    }
                }
            }
        }

        private void playerKeyDown(object sender, KeyEventArgs e)
        {
            if (gameStarted && !gamePaused)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        oyuncu1.up = true;
                        break;
                    case Keys.S:
                        oyuncu1.down = true;
                        break;
                    case Keys.A:
                        oyuncu1.left = true;
                        break;
                    case Keys.D:
                        oyuncu1.right = true;
                        break;
                }
            }
        }

        private void playerKeyUp(object sender, KeyEventArgs e)
        {
            if (gameStarted && !gamePaused)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        oyuncu1.up = false;
                        break;
                    case Keys.S:
                        oyuncu1.down = false;
                        break;
                    case Keys.A:
                        oyuncu1.left = false;
                        break;
                    case Keys.D:
                        oyuncu1.right = false;
                        break;

                    case Keys.H:
                        if (cheatCode.Equals(""))
                            cheatCode += "h";
                        break;
                    case Keys.E:
                        if (cheatCode.Equals("h"))
                            cheatCode += "e";
                        if (cheatCode.Equals("helpm"))
                        {
                            cheatCode = "";
                            if (!immortal)
                            {
                                immortal = true;
                                immortalLabel.Visible = true;
                            }
                            else
                            {
                                immortal = false;
                                immortalLabel.Visible = false;
                            }
                        }
                        break;
                    case Keys.L:
                        if (cheatCode.Equals("he"))
                            cheatCode += "l";
                        break;
                    case Keys.P:
                        if (cheatCode.Equals("hel"))
                            cheatCode += "p";
                        break;
                    case Keys.M:
                        if (cheatCode.Equals("help"))
                            cheatCode += "m";
                        break;
                    default:
                        cheatCode = "";
                        break;
                    case Keys.Escape:
                        pauseGame();
                        break;
                }
                return; // oyunu durdurunca, alttaki if'e de girip pause'u iptal etmesin diye metoddan çıkılıyor return ile
            }
            if (gameStarted && gamePaused && e.KeyCode == Keys.Escape)
            {
                continueGame();
                return;
            }
        }

        public void playerMovement()
        {
            if (oyuncu1.up)
            {
                if (oyuncu1.y >= 0)
                {
                    oyuncu1.y -= oyuncu1.speed; // y için - olunca yukarı, + olunca aşağı gidiyor
                }
            }
            if (oyuncu1.down)
            {
                if (oyuncu1.y + oyuncu1.yükseklik <= SCREEN_HEIGHT)
                {
                    oyuncu1.y += oyuncu1.speed;
                }
            }
            if (oyuncu1.left)
            {
                if (oyuncu1.x >= 0)
                {
                    oyuncu1.x -= oyuncu1.speed; // x için - olunca sola, + olunca sağa gidiyor
                }
            }
            if (oyuncu1.right)
            {
                if (oyuncu1.x + oyuncu1.genişlik <= SCREEN_WIDTH)
                {
                    oyuncu1.x += oyuncu1.speed;
                }
            }

            oyuncu1.pictureBox.Location = new Point(oyuncu1.x, oyuncu1.y);
        }
    }

    public class Oyuncu
    {
        public PictureBox pictureBox;
        public int genişlik = 60;
        public int yükseklik = 60;
        public Size size = new Size(60, 60); // Oyuncunun PictureBox'ı için kullanılacak Size
        public Color color = Color.Transparent;
        public bool up = false;
        public bool down = false;
        public bool left = false;
        public bool right = false;
        public int x;
        public int y;
        public int speed = 6;
        public int skor = 0;
        public int güdümlüLazer = 0;

        public Oyuncu(int ScreenWidth, int ScreenHeight, int grafikSeviyesi)
        {
            pictureBox = new PictureBox();  // Oyuncu için PictureBox oluşturuldu
            pictureBox.Size = size; // Oyuncunun PictureBox size'ı ayarlandı
            x = ScreenWidth / 2 - genişlik / 2;
            y = ScreenHeight / 2 - yükseklik / 2;
            pictureBox.Location = new Point(x, y);
            pictureBox.BackColor = color;
            switch (grafikSeviyesi)
            {
                case 0:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Oyuncu;
                    break;
                case 1:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Oyuncu;
                    break;
                case 2:
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Image = Properties.Resources.OyuncuTransparent;
                    break;
            }
            pictureBox.Image = Properties.Resources.OyuncuTransparent;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }

    public class Düşman
    {
        public PictureBox pictureBox;
        public int genişlik = 50;
        public int yükseklik = 50;
        public Size size = new Size(50, 50);
        public Color color = Color.Transparent;
        public double x;
        public double y;
        public double speedMain = 5;
        public double speedX;
        public double speedY;

        public Düşman(int grafikSeviyesi)
        {
            pictureBox = new PictureBox();
            pictureBox.Size = size;

            switch (grafikSeviyesi)
            {
                case 0:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Düşman;
                    break;
                case 1:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Düşman;
                    break;
                case 2:
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Image = Properties.Resources.DüşmanTransparent;
                    break;
            }
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public void enemyDestroyed(PictureBox gamePictureBox)
        {
            Explosion explosion = new Explosion();
            explosion.pictureBox = new PictureBox();
            explosion.pictureBox.Image = Properties.Resources.Explosion;
            explosion.pictureBox.Size = explosion.size;
            explosion.pictureBox.Location = pictureBox.Location;
            explosion.pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            gamePictureBox.Controls.Add(explosion.pictureBox);
            explosion.pictureBox.BackColor = Color.Transparent;

            Timer explosionTimer = new Timer();
            explosionTimer.Enabled = true;
            explosionTimer.Interval = 600; // Timer'ın zaman aralığını ayarladı
            explosionTimer.Tick += new EventHandler((o, e) =>
            {
                explosion.pictureBox.Dispose();
                explosionTimer.Dispose();
            });
        }
    }

    public class Hedef
    {
        public PictureBox pictureBox;
        public int genişlik = 30;
        public int yükseklik = 30;
        public Size size = new Size(30, 30);
        public Color color = Color.Transparent;
        public int x;
        public int y;

        public Hedef(int grafikSeviyesi)
        {
            pictureBox = new PictureBox(); // Hedef için PictureBox oluşturuldu
            pictureBox.Size = size;
            switch (grafikSeviyesi)
            {
                case 0:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Hedef;
                    break;
                case 1:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Hedef;
                    break;
                case 2:
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Image = Properties.Resources.HedefTransparent;
                    break;
            }
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // resim, PictureBox'un size'ına eşitlenecek
        }
    }

    public class Lazer
    {
        public PictureBox pictureBox;
        public int genişlik = 15;
        public int yükseklik = 15;
        public Size size = new Size(15, 15);
        public Color color = Color.Transparent;
        public double x;
        public double y;
        public double speedMain = 7;
        public double speedX;
        public double speedY;

        public Lazer(int grafikSeviyesi)
        {
            pictureBox = new PictureBox();
            pictureBox.Size = size;
            switch (grafikSeviyesi)
            {
                case 0:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Lazer;
                    break;
                case 1:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.Lazer;
                    break;
                case 2:
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Image = Properties.Resources.LazerTransparent;
                    break;
            }
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }

    public class Explosion
    {
        public PictureBox pictureBox;
        public int genişlik = 50;
        public int yükseklik = 50;
        public Size size = new Size(50, 50);
    }

    public class GüdümlüLazer
    {
        public PictureBox pictureBox;
        public int genişlik = 25;
        public int yükseklik = 25;
        public Size size = new Size(25, 25);
        public Color color = Color.Transparent;
        public double x;
        public double y;
        public double speedMain = 7;
        public double speedX;
        public double speedY;
        public Düşman kilitlenilenDüşman;

        public int grafikSeviyesi;

        public GüdümlüLazer(int grafikSeviyesi)
        {
            this.grafikSeviyesi = grafikSeviyesi;

            pictureBox = new PictureBox();
            pictureBox.Size = size;
            switch (grafikSeviyesi)
            {
                case 0:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.GüdümlüLazer;
                    break;
                case 1:
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Image = Properties.Resources.GüdümlüLazer;
                    break;
                case 2:
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Image = Properties.Resources.GüdümlüLazerTransparent;
                    break;
            }
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        }


        public void enYakınDüşmanıBul(List<Düşman> düşmanList, Hedef hedef)
        {
            if (düşmanList.Count > 0)
            {
                Düşman düşman = new Düşman(grafikSeviyesi);
                kilitlenilenDüşman = new Düşman(grafikSeviyesi);
                int minX = Int32.MaxValue;
                int minY = Int32.MaxValue;
                double minMesafe = Int32.MaxValue;

                int mesafeX;
                int mesafeY;
                double mesafe;

                for (int i = 0; i < düşmanList.Count; i++)
                {
                    düşman = düşmanList[i];
                    mesafeX = (int)düşman.x - hedef.x;
                    mesafeY = (int)düşman.y - hedef.y;
                    mesafe = Math.Sqrt(mesafeX * mesafeX + mesafeY * mesafeY); // ekrandaki düşmanların hedef'e göre mesafesi bulunuyor
                    if (mesafe < minMesafe)
                    {
                        minX = mesafeX;
                        minY = mesafeY;
                        minMesafe = mesafe;
                        kilitlenilenDüşman = düşman; // hedef'e en yakın olan düşman seçiliyor
                    }
                    // Güdümlü Lazer ekrandan çıksa da yokedilmiyor geri döneceği için
                }

                speedX = speedMain * (minX / minMesafe);
                speedY = speedMain * (minY / minMesafe);

                x += speedX;
                y += speedY;
            }
        }
    }
}
