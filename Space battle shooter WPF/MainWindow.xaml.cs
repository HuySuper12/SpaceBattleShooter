using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Space_battle_shooter_WPF
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer shootingTimer;
        private DispatcherTimer enemyShootingTimer;
        private DispatcherTimer bulletMoveTimer;
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private bool moveLeft, moveRight, moveUp, moveDown;
        private List<Rectangle> itemsToRemove = new List<Rectangle>();
        private Random rand = new Random();
        private int enemyCounter = 100;
        private int playerSpeed = 10;
        private int score = 0;
        private int damage = 0;
        private Rect playerHitBox;
        private int weaponLevel = 1;

        private MediaPlayer shootSound = new MediaPlayer();
        private MediaPlayer explosionSound = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            shootingTimer = new DispatcherTimer();
            shootingTimer.Interval = TimeSpan.FromSeconds(0.2);
            shootingTimer.Tick += ShootBullet;
            shootingTimer.Start();

            enemyShootingTimer = new DispatcherTimer();
            enemyShootingTimer.Interval = TimeSpan.FromSeconds(1);
            enemyShootingTimer.Tick += EnemyShootBullet;
            enemyShootingTimer.Start();

            bulletMoveTimer = new DispatcherTimer();
            bulletMoveTimer.Interval = TimeSpan.FromMilliseconds(30);
            bulletMoveTimer.Tick += (s, e) => MoveEnemyBulletsDown();
            bulletMoveTimer.Start();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += gameEngine;
            gameTimer.Start();
            MyCanvas.Focus();

            // Load sounds
            // For sounds (assuming 'laser-gun-174976.wav' and 'medium-explosion-40472.wav' are in the 'Sound' folder)
            shootSound.Open(new Uri("pack://application:,,,/Sound/laser-gun-174976.wav"));
            explosionSound.Open(new Uri("pack://application:,,,/Sound/medium-explosion-40472.wav"));

            // For setting background image (assuming 'purple.png' is in the 'Images' folder)
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/purple.png"));
            MyCanvas.Background = bg;

            // For setting player image (assuming 'player.png' is in the 'Images' folder)
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/player.png"));
            player.Fill = playerImage;
        }

        private void ShootBullet(object sender, EventArgs e)
        {
            Rectangle newBullet = new Rectangle
            {
                Tag = "bullet",
                Height = 20,
                Width = 5 + (weaponLevel * 2), // Increase bullet size based on weapon level
                Fill = Brushes.White,
                Stroke = Brushes.Cyan
            };
            Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
            Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);
            MyCanvas.Children.Add(newBullet);
            shootSound.Position = TimeSpan.Zero;
            shootSound.Play();
        }

        private void EnemyShootBullet(object sender, EventArgs e)
        {
            var bulletsToAdd = new List<Rectangle>();
            foreach (UIElement element in MyCanvas.Children)
            {
                if (element is Rectangle enemy && (string)enemy.Tag == "enemy")
                {
                    Rectangle enemyBullet = new Rectangle
                    {
                        Tag = "enemyBullet",
                        Height = 10,
                        Width = 10,
                        Fill = Brushes.Red,
                        Stroke = Brushes.White
                    };
                    Canvas.SetTop(enemyBullet, Canvas.GetTop(enemy) + enemy.Height);
                    Canvas.SetLeft(enemyBullet, Canvas.GetLeft(enemy) + enemy.Width / 2);
                    bulletsToAdd.Add(enemyBullet);
                }
            }
            foreach (var bullet in bulletsToAdd)
            {
                MyCanvas.Children.Add(bullet);
            }
        }

        private void MoveEnemyBulletsDown()
        {
            var bulletsToRemove = new List<Rectangle>();
            foreach (UIElement element in MyCanvas.Children)
            {
                if (element is Rectangle bullet && (string)bullet.Tag == "enemyBullet")
                {
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) + 5);
                    if (Canvas.GetTop(bullet) > MyCanvas.ActualHeight)
                    {
                        bulletsToRemove.Add(bullet);
                    }
                }
            }
            foreach (var bullet in bulletsToRemove)
            {
                MyCanvas.Children.Remove(bullet);
            }
        }

        private void gameEngine(object sender, EventArgs e)
        {
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            enemyCounter--;
            scoreText.Content = "Score: " + score;
            damageText.Content = "Damage: " + damage;

            if (enemyCounter < 0)
            {
                makeEnemies();
                enemyCounter = 100; // Reset enemy counter
            }

            HandlePlayerMovement();
            HandleBulletsAndEnemies();
            RemoveOffScreenItems();

            if (score > 5)
            {
                enemyCounter = 50; // Enemies spawn faster
            }

            if (damage > 50)
            {
                EndGame();
            }
        }

        private void HandlePlayerMovement()
        {
            if (moveLeft && Canvas.GetLeft(player) > 0)
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            if (moveRight && Canvas.GetLeft(player) + player.Width < Application.Current.MainWindow.Width)
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            if (moveUp && Canvas.GetTop(player) > 0)
                Canvas.SetTop(player, Canvas.GetTop(player) - playerSpeed);
            if (moveDown && Canvas.GetTop(player) + player.Height < Application.Current.MainWindow.Height)
                Canvas.SetTop(player, Canvas.GetTop(player) + playerSpeed);
        }

        private void HandleBulletsAndEnemies()
        {
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);
                    Rect bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) < 10)
                    {
                        itemsToRemove.Add(x);
                    }
                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            Rect enemy = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);
                            if (bullet.IntersectsWith(enemy))
                            {
                                itemsToRemove.Add(x);
                                itemsToRemove.Add(y);
                                score++;
                                explosionSound.Position = TimeSpan.Zero;
                                explosionSound.Play();
                            }
                        }
                    }
                }
                if (x is Rectangle && (string)x.Tag == "enemy")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 3);
                    Rect enemy = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) > Application.Current.MainWindow.Height)
                    {
                        itemsToRemove.Add(x);
                        damage += 5;
                    }
                    if (playerHitBox.IntersectsWith(enemy))
                    {
                        damage += 10;
                        itemsToRemove.Add(x);
                        explosionSound.Position = TimeSpan.Zero;
                        explosionSound.Play();
                    }
                }
            }
        }

        private void RemoveOffScreenItems()
        {
            foreach (var item in itemsToRemove)
            {
                MyCanvas.Children.Remove(item);
            }
            itemsToRemove.Clear();
        }

        private void EndGame()
        {
            gameTimer.Stop();
            MessageBox.Show("You have lost!" + Environment.NewLine + "You have destroyed " + score + " Alien ships");
        }

        private void makeEnemies()
        {
            ImageBrush enemySprite = new ImageBrush();
            int enemySpriteCounter = rand.Next(1, 6);
            switch (enemySpriteCounter)
            {
                case 1:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/1.png"));
                    break;
                case 2:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/2.png"));
                    break;
                case 3:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/3.png"));
                    break;
                case 4:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/4.png"));
                    break;
                case 5:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/5.png"));
                    break;
            }

            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Width = 50,
                Height = 50,
                Fill = enemySprite
            };

            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, rand.Next(0, (int)MyCanvas.ActualWidth - 50));
            MyCanvas.Children.Add(newEnemy);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) moveLeft = true;
            if (e.Key == Key.Right) moveRight = true;
            if (e.Key == Key.Up) moveUp = true;
            if (e.Key == Key.Down) moveDown = true;
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) moveLeft = false;
            if (e.Key == Key.Right) moveRight = false;
            if (e.Key == Key.Up) moveUp = false;
            if (e.Key == Key.Down) moveDown = false;
        }

        // Pause the game
        private bool isPaused = false;

        private void PauseGame(object sender, RoutedEventArgs e)
        {
            if (!isPaused)
            {

                gameTimer.Stop();
                shootingTimer.Stop();
                enemyShootingTimer.Stop();
                bulletMoveTimer.Stop();
                isPaused = true;
            }
            else
            {
                gameTimer.Start();
                shootingTimer.Start();
                enemyShootingTimer.Start();
                bulletMoveTimer.Start();
                isPaused = false;
            }
        }

        // Restart the game
        private void RestartGame(object sender, RoutedEventArgs e)
        {
            score = 0;
            damage = 0;
            enemyCounter = 100;

            // Clear all bullets and enemies from the Canvas
            foreach (var item in MyCanvas.Children.OfType<Rectangle>().Where(r => (string)r.Tag == "bullet" || (string)r.Tag == "enemy" || (string)r.Tag == "enemyBullet").ToList())
            {
                MyCanvas.Children.Remove(item);
            }

            // Reset player position
            Canvas.SetLeft(player, MyCanvas.ActualWidth / 2 - player.Width / 2);
            Canvas.SetTop(player, MyCanvas.ActualHeight - player.Height - 10);

            // Restart timers
            gameTimer.Start();
            shootingTimer.Start();
            enemyShootingTimer.Start();
            bulletMoveTimer.Start();
        }

        // Close the game
        private void CloseGame(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to quit the game?", "Quit?", MessageBoxButton.YesNo, MessageBoxImage.Stop);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}