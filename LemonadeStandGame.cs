using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace LemonadeStand
{
    public partial class LemonadeStandGame : Form
    {
        // Game state variables
        private int day = 0;
        private int playerCount = 1;
        private decimal[] assets;
        private bool[] bankrupt;
        private decimal lemonadeCost = 0.02m; // Initial cost per glass
        private decimal signCost = 0.15m;
        private readonly decimal initialAssets = 2.00m;
        private int weatherCondition = 2; // 2 = sunny, 7 = hot and dry, 10 = cloudy, 5 = thunderstorm
        private decimal weatherMultiplier = 1.0m;
        private Random random = new Random();

        // Player decision variables
        private int[] glassesToMake;
        private int[] signsToMake;
        private int[] pricePerGlass;
        private int currentPlayer = 0;

        // UI elements
        private Panel introPanel;
        private Panel gamePanel;
        private Panel weatherPanel;
        private Panel reportPanel;
        private Label dayLabel;
        private TextBox glassesInput;
        private TextBox signsInput;
        private TextBox priceInput;
        private Button continueButton;
        private Label assetsLabel;
        private Label reportLabel;
        private Label costLabel;
        
        // Game mode
        private bool classicMode = false;

        public LemonadeStandGame()
        {
            InitializeComponent();
            SetupUI();
            this.Text = "Lemonade Stand";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // Draw a lemonade stand
        private void DrawLemonadeStand(Graphics g, Rectangle bounds, bool isDetailed = true)
        {
            // Only draw graphics in modern mode
            if (classicMode)
                return;
                
            // Draw stand base/counter
            using (SolidBrush standBrush = new SolidBrush(Color.FromArgb(160, 82, 45)))
            using (SolidBrush roofBrush = new SolidBrush(Color.FromArgb(210, 180, 140)))
            using (SolidBrush lemonadeBrush = new SolidBrush(Color.FromArgb(255, 250, 205)))
            using (SolidBrush lemonBrush = new SolidBrush(Color.FromArgb(255, 255, 0)))
            using (Pen darkPen = new Pen(Color.FromArgb(101, 67, 33), 2))
            {
                // Counter/stand
                g.FillRectangle(standBrush, bounds.X + bounds.Width / 4, bounds.Y + bounds.Height / 2, 
                                bounds.Width / 2, bounds.Height / 2);
                
                // Roof
                Point[] roofPoints = new Point[]
                {
                    new Point(bounds.X + bounds.Width / 8, bounds.Y + bounds.Height / 2),
                    new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 4),
                    new Point(bounds.X + 7 * bounds.Width / 8, bounds.Y + bounds.Height / 2)
                };
                g.FillPolygon(roofBrush, roofPoints);
                g.DrawPolygon(darkPen, roofPoints);
                
                // Stand outline
                g.DrawRectangle(darkPen, bounds.X + bounds.Width / 4, bounds.Y + bounds.Height / 2, 
                               bounds.Width / 2, bounds.Height / 2);
                
                if (isDetailed)
                {
                    // Lemonade pitcher
                    g.FillRectangle(lemonadeBrush, bounds.X + bounds.Width / 2 - 15, 
                                   bounds.Y + bounds.Height / 2 + 10, 30, 40);
                    g.DrawRectangle(darkPen, bounds.X + bounds.Width / 2 - 15, 
                                   bounds.Y + bounds.Height / 2 + 10, 30, 40);
                    
                    // Lemon slice
                    g.FillEllipse(lemonBrush, bounds.X + bounds.Width / 2 + 25, 
                                  bounds.Y + bounds.Height / 2 + 15, 20, 20);
                    g.DrawEllipse(darkPen, bounds.X + bounds.Width / 2 + 25, 
                                  bounds.Y + bounds.Height / 2 + 15, 20, 20);
                    
                    // Sign
                    // Sign - wider sign
                    g.FillRectangle(new SolidBrush(Color.White), 
                                   bounds.X + bounds.Width / 4, 
                                   bounds.Y + bounds.Height / 2 - 30, 
                                   bounds.Width / 2, 
                                   25);
                    g.DrawRectangle(darkPen, 
                                   bounds.X + bounds.Width / 4, 
                                   bounds.Y + bounds.Height / 2 - 30, 
                                   bounds.Width / 2, 
                                   25);

                    using (Font signFont = new Font("Arial", 8, FontStyle.Bold))
                    using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        // Center the text in the sign
                        g.DrawString("LEMONADE", signFont, new SolidBrush(Color.Black),
                                    new RectangleF(bounds.X + bounds.Width / 4, bounds.Y + bounds.Height / 2 - 30,
                                                  bounds.Width / 2, 25), sf);
                    }
                }
            }
        }

        // Create a custom weather panel with graphics
        private Panel CreateWeatherPanel()
        {
            Panel customWeatherPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.SkyBlue,
                Visible = false
            };
            
            customWeatherPanel.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                if (classicMode)
                {
                    // Classic mode - simple text
                    customWeatherPanel.BackColor = Color.Black;
                    
                    string weatherText = "LEMONSVILLE WEATHER REPORT\n\n";
                    switch (weatherCondition)
                    {
                        case 2: weatherText += "SUNNY"; break;
                        case 7: weatherText += "HOT AND DRY"; break;
                        case 10: weatherText += "CLOUDY"; break;
                        case 5: weatherText += "THUNDERSTORM!"; break;
                    }
                    
                    using (Font weatherFont = new Font("Courier New", 18, FontStyle.Bold))
                    using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        g.DrawString(weatherText, weatherFont, Brushes.Lime, 
                                    new RectangleF(0, 200, customWeatherPanel.Width, 100), sf);
                        
                        // Draw ASCII art for classic version
                        string asciiArt = GetWeatherAsciiArt(weatherCondition);
                        using (Font asciiFont = new Font("Courier New", 12, FontStyle.Regular))
                        {
                            g.DrawString(asciiArt, asciiFont, Brushes.Lime,
                                        new RectangleF(0, 300, customWeatherPanel.Width, 200), sf);
                        }
                    }
                    return;
                }
                
                // Draw based on current weather condition
                switch (weatherCondition)
                {
                    case 2: // Sunny
                        DrawSunnyWeather(g, customWeatherPanel.ClientRectangle);
                        break;
                    case 7: // Hot and Dry
                        DrawHotWeather(g, customWeatherPanel.ClientRectangle);
                        break;
                    case 10: // Cloudy
                        DrawCloudyWeather(g, customWeatherPanel.ClientRectangle);
                        break;
                    case 5: // Thunderstorm
                        DrawStormyWeather(g, customWeatherPanel.ClientRectangle);
                        break;
                }
                
                // Draw stand in the bottom of the screen
                Rectangle standRect = new Rectangle(
                    customWeatherPanel.ClientRectangle.Width / 4,
                    customWeatherPanel.ClientRectangle.Height - 200,
                    customWeatherPanel.ClientRectangle.Width / 2,
                    150
                );
                DrawLemonadeStand(g, standRect);
            };
            
            return customWeatherPanel;
        }

        // ASCII art for classic mode
        private string GetWeatherAsciiArt(int condition)
        {
            switch (condition)
            {
                case 2: // Sunny
                    return @"    \   /    
     .-.     
  ― (   ) ―  
     `-'     
    /   \    ";
                
                case 7: // Hot and Dry
                    return @"    \   /    
     .-.     
  ― (   ) ―  
     `-'     
   ~ ~ ~ ~   ";
                
                case 10: // Cloudy
                    return @"   .--.      
  .-(    ).  
 (___.__)__) 
             
             ";
                
                case 5: // Thunderstorm
                    return @"   .--.      
  .-(    ).  
 (___.__)__) 
  ⚡ ⚡ ⚡ ⚡  
   ' ' ' '   ";
                
                default:
                    return "";
            }
        }

        // Weather drawing methods
        private void DrawSunnyWeather(Graphics g, Rectangle bounds)
        {
            // Sky background already set in panel BackColor
            
            // Sun
            using (SolidBrush sunBrush = new SolidBrush(Color.Yellow))
            using (Pen sunOutline = new Pen(Color.Orange, 2))
            {
                g.FillEllipse(sunBrush, bounds.Width / 2 - 40, 50, 80, 80);
                g.DrawEllipse(sunOutline, bounds.Width / 2 - 40, 50, 80, 80);
                
                // Sun rays
                for (int i = 0; i < 8; i++)
                {
                    double angle = Math.PI * i / 4;
                    float startX = bounds.Width / 2 + (float)(60 * Math.Cos(angle));
                    float startY = 90 + (float)(60 * Math.Sin(angle));
                    float endX = bounds.Width / 2 + (float)(90 * Math.Cos(angle));
                    float endY = 90 + (float)(90 * Math.Sin(angle));
                    g.DrawLine(new Pen(Color.Orange, 3), startX, startY, endX, endY);
                }
            }
            
            // Ground
            using (SolidBrush groundBrush = new SolidBrush(Color.FromArgb(120, 200, 80)))
            {
                g.FillRectangle(groundBrush, 0, bounds.Height - 100, bounds.Width, 100);
            }
            
            // Text
            using (Font weatherFont = new Font("Arial", 16, FontStyle.Bold))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
            {
                g.DrawString("LEMONSVILLE WEATHER REPORT", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 250, bounds.Width, 30), sf);
                g.DrawString("SUNNY", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 220, bounds.Width, 30), sf);
            }
        }

        private void DrawHotWeather(Graphics g, Rectangle bounds)
        {
            // Hot sky background
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 180, 100)), 0, 0, bounds.Width, bounds.Height);
            
            // Harsh sun
            using (SolidBrush sunBrush = new SolidBrush(Color.FromArgb(255, 90, 0)))
            using (Pen sunOutline = new Pen(Color.Red, 2))
            {
                g.FillEllipse(sunBrush, bounds.Width / 2 - 50, 40, 100, 100);
                g.DrawEllipse(sunOutline, bounds.Width / 2 - 50, 40, 100, 100);
                
                // Heat waves
                using (Pen wavePen = new Pen(Color.FromArgb(255, 150, 50), 2))
                {
                    for (int i = 0; i < bounds.Width; i += 80)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int y = 170 + j * 30;
                            DrawWavyLine(g, wavePen, i, y, i + 60, y);
                        }
                    }
                }
            }
            
            // Dry cracked ground
            using (SolidBrush groundBrush = new SolidBrush(Color.FromArgb(210, 180, 140)))
            using (Pen crackPen = new Pen(Color.FromArgb(160, 120, 90), 2))
            {
                g.FillRectangle(groundBrush, 0, bounds.Height - 100, bounds.Width, 100);
                
                // Draw cracks
                Random rand = new Random(5); // Fixed seed for consistent cracks
                for (int i = 0; i < 15; i++)
                {
                    int startX = rand.Next(bounds.Width);
                    int startY = bounds.Height - rand.Next(80);
                    DrawCracks(g, crackPen, startX, startY, 3);
                }
            }
            
            // Text
            using (Font weatherFont = new Font("Arial", 16, FontStyle.Bold))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
            {
                g.DrawString("LEMONSVILLE WEATHER REPORT", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 250, bounds.Width, 30), sf);
                g.DrawString("HOT AND DRY", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 220, bounds.Width, 30), sf);
            }
        }

        private void DrawCloudyWeather(Graphics g, Rectangle bounds)
        {
            // Sky background
            g.FillRectangle(new SolidBrush(Color.LightGray), 0, 0, bounds.Width, bounds.Height);
            
            // Clouds
            using (SolidBrush cloudBrush = new SolidBrush(Color.White))
            using (Pen cloudOutline = new Pen(Color.Gray, 1))
            {
                // Cloud 1
                DrawCloud(g, cloudBrush, cloudOutline, 100, 80, 180);
                
                // Cloud 2
                DrawCloud(g, cloudBrush, cloudOutline, bounds.Width - 250, 50, 200);
                
                // Cloud 3
                DrawCloud(g, cloudBrush, cloudOutline, bounds.Width / 2 - 100, 150, 250);
            }
            
            // Ground
            using (SolidBrush groundBrush = new SolidBrush(Color.FromArgb(100, 180, 70)))
            {
                g.FillRectangle(groundBrush, 0, bounds.Height - 100, bounds.Width, 100);
            }
            
            // Text
            using (Font weatherFont = new Font("Arial", 16, FontStyle.Bold))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
            {
                g.DrawString("LEMONSVILLE WEATHER REPORT", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 250, bounds.Width, 30), sf);
                g.DrawString("CLOUDY", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 220, bounds.Width, 30), sf);
            }
        }

        private void DrawStormyWeather(Graphics g, Rectangle bounds)
        {
            // Dark sky background
            g.FillRectangle(new SolidBrush(Color.DarkGray), 0, 0, bounds.Width, bounds.Height);
            
            // Storm clouds
            using (SolidBrush cloudBrush = new SolidBrush(Color.FromArgb(70, 70, 90)))
            using (Pen cloudOutline = new Pen(Color.Black, 1))
            {
                // Storm clouds
                DrawCloud(g, cloudBrush, cloudOutline, 50, 60, 250);
                DrawCloud(g, cloudBrush, cloudOutline, bounds.Width - 300, 40, 300);
                DrawCloud(g, cloudBrush, cloudOutline, bounds.Width / 2 - 150, 90, 320);
            }
            
            // Lightning
            Random rand = new Random(8); // Fixed seed for consistent lightning
            using (Pen lightningPen = new Pen(Color.Yellow, 3))
            {
                DrawLightning(g, lightningPen, bounds.Width / 2 - 50, 120, 6);
                DrawLightning(g, lightningPen, bounds.Width / 4, 90, 5);
            }
            
            // Rain
            using (Pen rainPen = new Pen(Color.FromArgb(80, 150, 220), 2))
            {
                for (int i = 0; i < bounds.Width; i += 20)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        int x = i + rand.Next(-10, 10);
                        int y = 150 + j * 30 + rand.Next(-10, 10);
                        g.DrawLine(rainPen, x, y, x - 5, y + 15);
                    }
                }
            }
            
            // Wet ground
            using (SolidBrush groundBrush = new SolidBrush(Color.FromArgb(80, 120, 60)))
            {
                g.FillRectangle(groundBrush, 0, bounds.Height - 100, bounds.Width, 100);
                
                // Puddles
                using (SolidBrush puddleBrush = new SolidBrush(Color.FromArgb(70, 130, 180)))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int x = rand.Next(bounds.Width - 80);
                        int y = bounds.Height - 70 + rand.Next(-20, 20);
                        g.FillEllipse(puddleBrush, x, y, 80 + rand.Next(40), 20 + rand.Next(10));
                    }
                }
            }
            
            // Text
            using (Font weatherFont = new Font("Arial", 16, FontStyle.Bold))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
            {
                g.DrawString("LEMONSVILLE WEATHER REPORT", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 250, bounds.Width, 30), sf);
                g.DrawString("THUNDERSTORMS!", weatherFont, Brushes.Black, 
                            new RectangleF(0, bounds.Height - 220, bounds.Width, 30), sf);
            }
        }

        // Helper drawing methods
        private void DrawCloud(Graphics g, Brush brush, Pen outline, int x, int y, int width)
        {
            int height = width / 2;
            float radius = height / 2;
            
            // Create path for the entire cloud shape
            GraphicsPath cloudPath = new GraphicsPath();
            
            // Add the base rectangle
            cloudPath.AddRectangle(new RectangleF(x, y + radius, width, height));
            
            // Add the top bubbles
            for (int i = 0; i < width; i += (int)(radius * 1.5))
            {
                cloudPath.AddEllipse(x + i, y, radius * 2, radius * 2);
            }
            
            // Add bottom arcs
            cloudPath.AddEllipse(x, y + height, radius * 2, radius * 2);
            cloudPath.AddEllipse(x + width - radius * 2, y + height, radius * 2, radius * 2);
            
            // Fill and outline the entire cloud
            g.FillPath(brush, cloudPath);
            if (outline != null)
            {
                g.DrawPath(outline, cloudPath);
            }
        }

        private void DrawLightning(Graphics g, Pen pen, int startX, int startY, int segments)
        {
            Random rand = new Random(startX + startY);
            int x = startX;
            int y = startY;
            
            for (int i = 0; i < segments; i++)
            {
                int nextX = x + rand.Next(-20, 20);
                int nextY = y + rand.Next(20, 40);
                g.DrawLine(pen, x, y, nextX, nextY);
                x = nextX;
                y = nextY;
            }
        }

        private void DrawCracks(Graphics g, Pen pen, int startX, int startY, int depth)
        {
            if (depth <= 0) return;
            
            Random rand = new Random(startX + startY);
            
            for (int i = 0; i < rand.Next(2, 4); i++)
            {
                int length = rand.Next(10, 30);
                double angle = rand.NextDouble() * Math.PI;
                
                int endX = startX + (int)(length * Math.Cos(angle));
                int endY = startY + (int)(length * Math.Sin(angle));
                
                g.DrawLine(pen, startX, startY, endX, endY);
                
                if (depth > 1)
                {
                    DrawCracks(g, pen, endX, endY, depth - 1);
                }
            }
        }

        private void DrawWavyLine(Graphics g, Pen pen, float startX, float startY, float endX, float endY)
        {
            int steps = (int)((endX - startX) / 10);
            if (steps < 2) steps = 2;
            
            Point[] points = new Point[steps];
            
            for (int i = 0; i < steps; i++)
            {
                float x = startX + ((endX - startX) * i / (steps - 1));
                float waveFactor = (float)Math.Sin(i * Math.PI) * 5;
                float y = startY + waveFactor;
                points[i] = new Point((int)x, (int)y);
            }
            
            g.DrawCurve(pen, points);
        }

        private void SetupUI()
        {
            // Setup Introduction Panel with Mode Selection
            introPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 200, 100)
            };

            Label titleLabel = new Label
            {
                Text = "LEMONADE STAND",
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(800, 50),
                Location = new Point(0, 30)
            };

            // Mode selection
            GroupBox modeGroupBox = new GroupBox
            {
                Text = "Game Mode",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Size = new Size(300, 80),
                Location = new Point(250, 90)
            };

            RadioButton modernRadio = new RadioButton
            {
                Text = "Modern Graphics",
                Checked = true,
                Font = new Font("Arial", 10),
                Size = new Size(250, 20),
                Location = new Point(20, 20)
            };

            RadioButton classicRadio = new RadioButton
            {
                Text = "Classic (80's Green CRT Style)",
                Font = new Font("Arial", 10),
                Size = new Size(250, 20),
                Location = new Point(20, 45)
            };

            modeGroupBox.Controls.Add(modernRadio);
            modeGroupBox.Controls.Add(classicRadio);

            Label introLabel = new Label
            {
                Text = "Hi! Welcome to Lemonsville, California!\r\n\r\n" +
                      "In this small town, you are in charge of running your own lemonade stand. " +
                      "You can compete with as many other people as you wish, but how much profit " +
                      "you make is up to you. If you make the most money, you're the winner!!\r\n\r\n" +
                      "How many people will be playing?",
                Font = new Font("Arial", 12),
                Size = new Size(600, 200),
                Location = new Point(100, 180)
            };

            NumericUpDown playerCountInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 30,
                Value = 1,
                Size = new Size(60, 30),
                Location = new Point(370, 380),
                Font = new Font("Arial", 12)
            };

            Button startGameButton = new Button
            {
                Text = "Start Game",
                Size = new Size(150, 40),
                Location = new Point(325, 430),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            startGameButton.Click += (s, e) =>
            {
                playerCount = (int)playerCountInput.Value;
                classicMode = classicRadio.Checked; // Set game mode based on selection
                StartNewGame();
            };

            introPanel.Controls.Add(titleLabel);
            introPanel.Controls.Add(modeGroupBox);
            introPanel.Controls.Add(introLabel);
            introPanel.Controls.Add(playerCountInput);
            introPanel.Controls.Add(startGameButton);
            
            // Add a lemonade stand to the intro panel (only in modern mode)
            introPanel.Paint += (s, e) => {
                if (classicMode) 
                {
                    // Draw classic style logo
                    e.Graphics.FillRectangle(new SolidBrush(Color.Black), 
                        new Rectangle(0, introPanel.Height - 170, introPanel.Width, 150));
                    
                    using (Font asciiFont = new Font("Courier New", 12, FontStyle.Regular))
                    using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        string asciiStand = @"
         _____
        /     \
       /       \
      /_________\
      |  LEMON  |
      |  -ADE   |
      |_________|
      |         |
      |  (|)    |
      |_________|";
                        
                        e.Graphics.DrawString(asciiStand, asciiFont, Brushes.Lime,
                                     new RectangleF(0, introPanel.Height - 170, introPanel.Width, 150), sf);
                    }
                    return;
                }
                
                // Modern graphics
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw a bigger lemonade stand at the bottom
                Rectangle standRect = new Rectangle(
                    introPanel.ClientRectangle.Width / 4,
                    introPanel.ClientRectangle.Height - 220,
                    introPanel.ClientRectangle.Width / 2,
                    200
                );
                DrawLemonadeStand(g, standRect, true);
            };
            
            this.Controls.Add(introPanel);

            // Setup Game Panel - will be populated when game starts
            gamePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 220, 150),
                Visible = false
            };
            this.Controls.Add(gamePanel);

            // Setup Weather Panel with custom graphics
            weatherPanel = CreateWeatherPanel();
            this.Controls.Add(weatherPanel);

            // Setup Report Panel
            reportPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(230, 255, 230),
                Visible = false
            };
            
            reportLabel = new Label
            {
                Font = new Font("Consolas", 12),
                Size = new Size(700, 400),
                Location = new Point(50, 50),
                AutoSize = false
            };
            
            Button nextDayButton = new Button
            {
                Text = "Next Day",
                Size = new Size(150, 40),
                Location = new Point(325, 480),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            
            nextDayButton.Click += (s, e) =>
            {
                if (CheckGameOver())
                {
                    EndGame();
                }
                else
                {
                    currentPlayer = 0;
                    ShowWeatherReport();
                }
            };
            
            reportPanel.Controls.Add(reportLabel);
            reportPanel.Controls.Add(nextDayButton);
            this.Controls.Add(reportPanel);
        }

        private void StartNewGame()
        {
            // Initialize game state
            day = 0;
            assets = new decimal[playerCount];
            bankrupt = new bool[playerCount];
            glassesToMake = new int[playerCount];
            signsToMake = new int[playerCount];
            pricePerGlass = new int[playerCount];

            for (int i = 0; i < playerCount; i++)
            {
                assets[i] = initialAssets;
                bankrupt[i] = false;
            }

            // Apply classic mode theme if selected
            if (classicMode)
            {
                this.BackColor = Color.Black;
                gamePanel.BackColor = Color.Black;
                reportPanel.BackColor = Color.Black;
                
                // Update font colors for all existing labels
                foreach (Control c in this.Controls)
                {
                    ApplyClassicTheme(c);
                }
            }

            // Show instructions if this is a new game
            MessageBox.Show(
                "To manage your lemonade stand, you will need to make these decisions every day:\n\n" +
                "1. How many glasses of lemonade to make (only one batch is made each morning)\n" +
                "2. How many advertising signs to make (the signs cost fifteen cents each)\n" +
                "3. What price to charge for each glass\n\n" +
                "You will begin with $2.00 cash (assets).\n" +
                "Because your mother gave you some sugar, your cost to make lemonade is two cents a glass (this may change in the future).",
                "Game Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information);

            MessageBox.Show(
                "Your expenses are the sum of the cost of the lemonade and the cost of the signs.\n\n" +
                "Your profits are the difference between the income from sales and your expenses.\n\n" +
                "The number of glasses you sell each day depends on the price you charge, and on the number of advertising signs you use.\n\n" +
                "Keep track of your assets, because you can't spend more money than you have!",
                "Game Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information);

            introPanel.Visible = false;
            ShowWeatherReport();
        }
        
        // Apply classic theme to controls recursively
        private void ApplyClassicTheme(Control control)
        {
            if (control is Label || control is Button || control is TextBox || 
                control is NumericUpDown || control is RadioButton || control is GroupBox)
            {
                control.ForeColor = Color.Lime;
                control.BackColor = Color.Black;
                
                if (control is Button)
                {
                    ((Button)control).FlatStyle = FlatStyle.Flat;
                    ((Button)control).FlatAppearance.BorderColor = Color.Lime;
                    ((Button)control).FlatAppearance.BorderSize = 1;
                }
                
                if (control is TextBox || control is NumericUpDown)
                {
                    control.Font = new Font("Courier New", control.Font.Size);
                }
                
                if (control is Label)
                {
                    control.Font = new Font("Courier New", control.Font.Size);
                }
            }
            
            // Apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyClassicTheme(child);
            }
        }

        private void ShowWeatherReport()
        {
            // Determine weather for the day
            weatherCondition = GetRandomWeather();
            
            // Update weather multiplier
            switch (weatherCondition)
            {
                case 2: // Sunny
                    weatherMultiplier = 1.0m;
                    break;
                case 7: // Hot and Dry
                    weatherMultiplier = 2.0m;
                    break;
                case 10: // Cloudy
                    weatherMultiplier = 0.6m;
                    break;
                case 5: // Thunderstorm
                    weatherMultiplier = 0.0m;
                    break;
            }

            // Force refresh of the weather panel to show the new graphics
            weatherPanel.Invalidate();
            weatherPanel.Visible = true;
            
            // Delay before showing the game panel
            Timer weatherTimer = new Timer();
            weatherTimer.Interval = 2000;
            weatherTimer.Tick += (s, e) =>
            {
                weatherTimer.Stop();
                weatherPanel.Visible = false;
                ShowGamePanel();
            };
            weatherTimer.Start();
        }

        private int GetRandomWeather()
        {
            double chance = random.NextDouble();
            if (chance < 0.4)
                return 2; // Sunny
            else if (chance < 0.7)
                return 10; // Cloudy
            else
                return 7; // Hot and Dry
                
            // Thunderstorm (5) is a special event handled separately
        }

        private void ShowGamePanel()
        {
            day++;
            
            // Clear existing controls
            gamePanel.Controls.Clear();
            
            // If player is bankrupt, skip to next player
            if (bankrupt[currentPlayer])
            {
                NextPlayer();
                return;
            }

            // Update cost of lemonade
            if (day == 3)
            {
                MessageBox.Show("(YOUR MOTHER QUIT GIVING YOU FREE SUGAR)", "News Flash", MessageBoxButtons.OK);
                lemonadeCost = 0.04m;
            }
            else if (day == 7)
            {
                MessageBox.Show("(THE PRICE OF LEMONADE MIX JUST WENT UP)", "News Flash", MessageBoxButtons.OK);
                lemonadeCost = 0.05m;
            }

            // Set up game interface for current player
            dayLabel = new Label
            {
                Text = $"Day {day} - Stand {currentPlayer + 1}",
                Font = new Font(classicMode ? "Courier New" : "Arial", 16, FontStyle.Bold),
                Size = new Size(800, 30),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = classicMode ? Color.Lime : Color.Black
            };

            assetsLabel = new Label
            {
                Text = $"Assets: ${assets[currentPlayer]:F2}",
                Font = new Font(classicMode ? "Courier New" : "Arial", 14),
                Size = new Size(400, 30),
                Location = new Point(50, 70),
                ForeColor = classicMode ? Color.Lime : Color.Black
            };

            costLabel = new Label
            {
                Text = $"Cost per glass of lemonade: ${lemonadeCost:F2}",
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(400, 30),
                Location = new Point(50, 100),
                ForeColor = classicMode ? Color.Lime : Color.Black
            };

            Label glassesLabel = new Label
            {
                Text = "How many glasses of lemonade do you wish to make?",
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(400, 30),
                Location = new Point(50, 150),
                ForeColor = classicMode ? Color.Lime : Color.Black
            };

            glassesInput = new TextBox
            {
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(100, 30),
                Location = new Point(500, 150),
                Text = "0",
                ForeColor = classicMode ? Color.Lime : Color.Black,
                BackColor = classicMode ? Color.Black : Color.White
            };

            Label signsLabel = new Label
            {
                Text = $"How many advertising signs (${signCost:F2} each) do you want to make?",
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(450, 30),
                Location = new Point(50, 200),
                ForeColor = classicMode ? Color.Lime : Color.Black
            };

            signsInput = new TextBox
            {
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(100, 30),
                Location = new Point(500, 200),
                Text = "0",
                ForeColor = classicMode ? Color.Lime : Color.Black,
                BackColor = classicMode ? Color.Black : Color.White
            };

            Label priceLabel = new Label
            {
                Text = "What price (in cents) do you wish to charge for lemonade?",
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(450, 30),
                Location = new Point(50, 250),
                ForeColor = classicMode ? Color.Lime : Color.Black
            };

            priceInput = new TextBox
            {
                Font = new Font(classicMode ? "Courier New" : "Arial", 12),
                Size = new Size(100, 30),
                Location = new Point(500, 250),
                Text = "0",
                ForeColor = classicMode ? Color.Lime : Color.Black,
                BackColor = classicMode ? Color.Black : Color.White
            };

            continueButton = new Button
            {
                Text = "Continue",
                Size = new Size(150, 40),
                Location = new Point(325, 350),
                Font = new Font(classicMode ? "Courier New" : "Arial", 12, FontStyle.Bold),
                ForeColor = classicMode ? Color.Lime : Color.Black,
                BackColor = classicMode ? Color.Black : SystemColors.Control
            };

            if (classicMode)
            {
                continueButton.FlatStyle = FlatStyle.Flat;
                continueButton.FlatAppearance.BorderColor = Color.Lime;
                continueButton.FlatAppearance.BorderSize = 1;
            }

            continueButton.Click += (s, e) => ValidateAndContinue();

            // Add lemonade stand graphic to game panel (only in modern mode)
            if (!classicMode)
            {
                Panel standGraphicPanel = new Panel
                {
                    Size = new Size(200, 150),
                    Location = new Point(550, 300),
                    BackColor = Color.Transparent
                };

                standGraphicPanel.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    DrawLemonadeStand(g, standGraphicPanel.ClientRectangle);
                };
                
                gamePanel.Controls.Add(standGraphicPanel);
            }
            else
            {
                // ASCII art for classic mode
                Label asciiArtLabel = new Label
                {
                    Text = @"
         _____
        /     \
       /       \
      /_________\
      |  LEMON  |
      |  -ADE   |
      |_________|
      |         |
      |  (|)    |
      |_________|",
                    Font = new Font("Courier New", 10),
                    Size = new Size(200, 200),
                    Location = new Point(550, 300),
                    ForeColor = Color.Lime,
                    BackColor = Color.Black
                };
                
                gamePanel.Controls.Add(asciiArtLabel);
            }

            // Add controls to the game panel
            gamePanel.Controls.Add(dayLabel);
            gamePanel.Controls.Add(assetsLabel);
            gamePanel.Controls.Add(costLabel);
            gamePanel.Controls.Add(glassesLabel);
            gamePanel.Controls.Add(glassesInput);
            gamePanel.Controls.Add(signsLabel);
            gamePanel.Controls.Add(signsInput);
            gamePanel.Controls.Add(priceLabel);
            gamePanel.Controls.Add(priceInput);
            gamePanel.Controls.Add(continueButton);

            gamePanel.Visible = true;
        }

        private void ValidateAndContinue()
        {
            // Validate inputs
            bool validInput = true;
            int glasses = 0;
            int signs = 0;
            int price = 0;

            try
            {
                glasses = int.Parse(glassesInput.Text);
                signs = int.Parse(signsInput.Text);
                price = int.Parse(priceInput.Text);

                if (glasses < 0 || glasses > 1000)
                {
                    MessageBox.Show("Please enter a reasonable number of glasses (0-1000).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    validInput = false;
                }

                if (signs < 0 || signs > 50)
                {
                    MessageBox.Show("Please enter a reasonable number of signs (0-50).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    validInput = false;
                }

                if (price < 0 || price > 100)
                {
                    MessageBox.Show("Please enter a reasonable price (0-100 cents).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    validInput = false;
                }

                // Check if player has enough money
                decimal lemonadeCosts = glasses * lemonadeCost;
                decimal signCosts = signs * signCost;
                decimal totalCosts = lemonadeCosts + signCosts;

                if (totalCosts > assets[currentPlayer])
                {
                    MessageBox.Show($"You don't have enough money! You have ${assets[currentPlayer]:F2} but need ${totalCosts:F2}", "Not Enough Assets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    validInput = false;
                }
            }
            catch
            {
                MessageBox.Show("Please enter valid numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                validInput = false;
            }

            if (validInput)
            {
                // Store player decisions
                glassesToMake[currentPlayer] = glasses;
                signsToMake[currentPlayer] = signs;
                pricePerGlass[currentPlayer] = price;

                // Move to next player or calculate results
                NextPlayer();
            }
        }

        private void NextPlayer()
        {
            currentPlayer++;
            
            // If all players have made decisions, calculate results
            if (currentPlayer >= playerCount || allPlayersAreBankrupt())
            {
                CalculateResults();
            }
            else
            {
                ShowGamePanel();
            }
        }

        private bool allPlayersAreBankrupt()
        {
            for (int i = currentPlayer; i < playerCount; i++)
            {
                if (!bankrupt[i]) return false;
            }
            return true;
        }

        private void CalculateResults()
        {
            gamePanel.Visible = false;
            
            // Handle special weather event - thunderstorm
            bool thunderstorm = false;
            if (weatherCondition == 10 && random.NextDouble() < 0.25)
            {
                thunderstorm = true;
                weatherCondition = 5;
                MessageBox.Show("WEATHER REPORT: A SEVERE THUNDERSTORM HIT LEMONSVILLE EARLIER TODAY, JUST AS THE LEMONADE STANDS WERE BEING SET UP.\n\nUNFORTUNATELY, EVERYTHING WAS RUINED!!", "Thunderstorm!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Calculate results for each player
            string reportText = "$$ LEMONSVILLE DAILY FINANCIAL REPORT $$\n\n";
            
            for (int i = 0; i < playerCount; i++)
            {
                if (bankrupt[i])
                {
                    reportText += $"\nSTAND {i + 1}: BANKRUPT\n";
                    continue;
                }

                // Calculate sales
                int glassesSold;
                if (thunderstorm)
                {
                    glassesSold = 0;
                }
                else
                {
                    // Price factor
                    decimal priceFactor;
                    if (pricePerGlass[i] < 10) // Original threshold was 10 cents
                    {
                        priceFactor = (10 - pricePerGlass[i]) / 10m * 0.8m * 30 + 30;
                    }
                    else
                    {
                        priceFactor = (10m * 10m * 30) / (pricePerGlass[i] * pricePerGlass[i]);
                    }

                    // Signs factor
                    decimal w = -signsToMake[i] * 0.5m;
                    decimal v = 1 - (decimal)Math.Exp((double)w);
                    decimal baseSales = weatherMultiplier * (priceFactor + (priceFactor * v));
                    glassesSold = (int)Math.Min(baseSales, glassesToMake[i]);
                }

                // Calculate financials
                decimal income = glassesSold * pricePerGlass[i] * 0.01m;
                decimal expenses = signsToMake[i] * signCost + glassesToMake[i] * lemonadeCost;
                decimal profit = income - expenses;
                assets[i] += profit;

                // Check for bankruptcy
                if (assets[i] < 0)
                {
                    assets[i] = 0;
                }

                if (assets[i] < lemonadeCost)
                {
                    bankrupt[i] = true;
                    reportText += $"\nSTAND {i + 1}\n...YOU DON'T HAVE ENOUGH MONEY LEFT TO STAY IN BUSINESS. YOU'RE BANKRUPT!\n";
                }
                else
                {
                    // Generate report for this player
                    reportText += $"\nDAY {day,-6}STAND {i + 1}\n\n";
                    reportText += $"{glassesSold} GLASSES SOLD\n\n";
                    reportText += $"${pricePerGlass[i] / 100.0:F2} PER GLASS{"",-16}INCOME ${income:F2}\n\n";
                    reportText += $"{glassesToMake[i]} GLASSES MADE\n\n";
                    reportText += $"{signsToMake[i]} SIGNS MADE{"",-14}EXPENSES ${expenses:F2}\n\n";
                    reportText += $"{"",-24}PROFIT ${profit:F2}\n\n";
                    reportText += $"{"",-24}ASSETS ${assets[i]:F2}\n";
                    reportText += "-------------------------------------\n";
                }
            }

            reportLabel.Text = reportText;
            
            // Apply classic theme to the report panel
            if (classicMode)
            {
                reportPanel.BackColor = Color.Black;
                reportLabel.ForeColor = Color.Lime;
                reportLabel.Font = new Font("Courier New", 12);
                
                foreach (Control control in reportPanel.Controls)
                {
                    if (control is Button)
                    {
                        control.ForeColor = Color.Lime;
                        control.BackColor = Color.Black;
                        control.Font = new Font("Courier New", control.Font.Size);
                        
                        Button btn = (Button)control;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = Color.Lime;
                        btn.FlatAppearance.BorderSize = 1;
                    }
                }
            }
            else
            {
                // Add lemonade stand graphic to report panel
                Panel reportStandGraphic = new Panel
                {
                    Size = new Size(150, 100),
                    Location = new Point(600, 350),
                    BackColor = Color.Transparent
                };
                
                reportStandGraphic.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    DrawLemonadeStand(g, reportStandGraphic.ClientRectangle, false);
                };
                
                reportPanel.Controls.Add(reportStandGraphic);
            }
            
            // Add ASCII art in classic mode
            if (classicMode)
            {
                Label classicArtLabel = new Label
                {
                    Text = @"
         $
        / \
       /___\
      |STAND|
      |     |
      |_____|",
                    Font = new Font("Courier New", 10),
                    Size = new Size(150, 120),
                    Location = new Point(600, 350),
                    ForeColor = Color.Lime,
                    BackColor = Color.Black
                };
                
                reportPanel.Controls.Add(classicArtLabel);
            }
            
            reportPanel.Visible = true;
        }

        private bool CheckGameOver()
        {
            // Check if all players are bankrupt
            bool allBankrupt = true;
            foreach (bool b in bankrupt)
            {
                if (!b)
                {
                    allBankrupt = false;
                    break;
                }
            }
            
            return allBankrupt;
        }

        private void EndGame()
        {
            // Find the winner(s)
            decimal maxAssets = assets.Max();
            List<int> winners = new List<int>();
            
            for (int i = 0; i < playerCount; i++)
            {
                if (assets[i] == maxAssets)
                {
                    winners.Add(i);
                }
            }
            
            string winnerText;
            if (winners.Count == 1)
            {
                winnerText = $"Player {winners[0] + 1} wins with ${maxAssets:F2}!";
            }
            else
            {
                winnerText = "It's a tie between players: ";
                for (int i = 0; i < winners.Count; i++)
                {
                    winnerText += (winners[i] + 1).ToString();
                    if (i < winners.Count - 1)
                        winnerText += ", ";
                }
                winnerText += $" with ${maxAssets:F2} each!";
            }
            
            MessageBox.Show(winnerText, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Ask to play again
            if (MessageBox.Show("Would you like to play again?", "Play Again?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                gamePanel.Visible = false;
                reportPanel.Visible = false;
                weatherPanel.Visible = false;
                introPanel.Visible = true;
                
                // Reset classic mode flag for intro panel
                classicMode = false;
                this.BackColor = SystemColors.Control;
                introPanel.BackColor = Color.FromArgb(255, 200, 100);
                
                // Reset controls in intro panel
                foreach (Control control in introPanel.Controls)
                {
                    if (control is GroupBox)
                    {
                        foreach (Control c in control.Controls)
                        {
                            if (c is RadioButton radioBtn)
                            {
                                if (radioBtn.Text.Contains("Modern"))
                                {
                                    radioBtn.Checked = true;
                                }
                                else
                                {
                                    radioBtn.Checked = false;
                                }
                                radioBtn.ForeColor = Color.Black;
                                radioBtn.BackColor = SystemColors.Control;
                            }
                        }
                        control.ForeColor = Color.Black;
                        control.BackColor = SystemColors.Control;
                    }
                    else
                    {
                        control.ForeColor = Color.Black;
                        if (!(control is Panel))
                        {
                            control.BackColor = introPanel.BackColor;
                        }
                    }
                }
            }
            else
            {
                Application.Exit();
            }
        }
    }

    // Main form for classic mode display
    public class ClassicLemonadeStandGame : LemonadeStandGame
    {
        public ClassicLemonadeStandGame() : base()
        {
            // Set classic theme
            this.BackColor = Color.Black;
            this.ForeColor = Color.Lime;
            
            foreach (Control control in this.Controls)
            {
                SetClassicTheme(control);
            }
        }
        
        private void SetClassicTheme(Control control)
        {
            control.BackColor = Color.Black;
            control.ForeColor = Color.Lime;
            
            if (control is TextBox || control is Button)
            {
                control.Font = new Font("Courier New", control.Font.Size);
                
                if (control is Button)
                {
                    Button btn = (Button)control;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = Color.Lime;
                    btn.FlatAppearance.BorderSize = 1;
                }
            }
            
            foreach (Control child in control.Controls)
            {
                SetClassicTheme(child);
            }
        }
    }
}