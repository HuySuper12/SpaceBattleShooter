using Repository.Entities;
using Service.Services;
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
        private int damage = 100;
        private Rect playerHitBox;
        private int weaponLevel = 1;
        private bool hasCollectedBulletItem = false;
        private bool hasCollectedSecondBulletItem = false;
        private bool hasCollectedThirdBulletItem = false;
        private bool hasCollectedForthBulletItem = false;
        private int countEatBullet = 0;

        private MediaPlayer shootSound = new MediaPlayer();
        private MediaPlayer explosionSound = new MediaPlayer();
        private MediaPlayer introSound = new MediaPlayer();
        private MediaPlayer hitDamage = new MediaPlayer();
        private MediaPlayer lose = new MediaPlayer();

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

            // Load sounds (using relative paths)
            shootSound.Open(new Uri("Sounds/laser-gun-174976.mp3", UriKind.Relative));
            explosionSound.Open(new Uri("Sounds/medium-explosion-40472.mp3", UriKind.Relative));
            introSound.Open(new Uri("Sounds/intro.mp3", UriKind.Relative));
            hitDamage.Open(new Uri("Sounds/damage.mp3", UriKind.Relative));
            lose.Open(new Uri("Sounds/lose.mp3", UriKind.Relative));
            introSound.Play();

            // Set background image
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/purple.png"));
            MyCanvas.Background = bg;

            // Set player image
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/player.png"));
            player.Fill = playerImage;
        }

        private void ShootBullet(object sender, EventArgs e)
        {
            // Create the first bullet
            Rectangle newBullet1 = new Rectangle
            {
                Tag = "bullet",
                Height = 20,
                Width = 5 + (weaponLevel * 2), // Increase bullet size based on weapon level
                Fill = Brushes.White,
                Stroke = Brushes.Cyan
            };
            Canvas.SetTop(newBullet1, Canvas.GetTop(player) - newBullet1.Height);
            Canvas.SetLeft(newBullet1, Canvas.GetLeft(player) + player.Width / 2 - newBullet1.Width / 2); // Center the first bullet
            MyCanvas.Children.Add(newBullet1);

            if (hasCollectedBulletItem)
            {
                // Create the second bullet
                Rectangle newBullet2 = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5 + (weaponLevel * 2), // Increase bullet size based on weapon level
                    Fill = Brushes.White,
                    Stroke = Brushes.Cyan
                };
                Canvas.SetTop(newBullet2, Canvas.GetTop(player) - newBullet2.Height);
                Canvas.SetLeft(newBullet2, Canvas.GetLeft(player) + player.Width / 2 - newBullet2.Width - 10); // Position the second bullet to the left
                MyCanvas.Children.Add(newBullet2);
            }

            if (hasCollectedSecondBulletItem)
            {
                // Create the third bullet
                Rectangle newBullet3 = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5 + (weaponLevel * 2), // Increase bullet size based on weapon level
                    Fill = Brushes.White,
                    Stroke = Brushes.Cyan
                };
                Canvas.SetTop(newBullet3, Canvas.GetTop(player) - newBullet3.Height);
                Canvas.SetLeft(newBullet3, Canvas.GetLeft(player) + player.Width / 2 + 10); // Position the third bullet to the right
                MyCanvas.Children.Add(newBullet3);
            }

            if (hasCollectedThirdBulletItem)
            {
                // Create the forth bullet

                Rectangle newBullet4 = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5 + (weaponLevel * 2), // Increase bullet size based on weapon level
                    Fill = Brushes.White,
                    Stroke = Brushes.Cyan
                };

                Canvas.SetTop(newBullet4, Canvas.GetTop(player) - newBullet4.Height);
                Canvas.SetLeft(newBullet4, Canvas.GetLeft(player) + player.Width / 2 + 30); // Position the forth bullet to the right
                MyCanvas.Children.Add(newBullet4);
            }

            if (hasCollectedForthBulletItem)
            {
                //Create the fifth bullet
                Rectangle newBullet5 = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5 + (weaponLevel * 2), // Increase bullet size based on weapon level
                    Fill = Brushes.White,
                    Stroke = Brushes.Cyan
                };

                Canvas.SetTop(newBullet5, Canvas.GetTop(player) - newBullet5.Height);
                Canvas.SetLeft(newBullet5, Canvas.GetLeft(player) + player.Width / 2 - 30); // Position the fifth bullet to the left
                MyCanvas.Children.Add(newBullet5);
            }

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

                    // Check for collision with player
                    Rect bulletHitBox = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);
                    if (bulletHitBox.IntersectsWith(playerHitBox))
                    {
                        bulletsToRemove.Add(bullet);
                        damage -= 10; // Increase damage when player is hit
                        hitDamage.Play();
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
            damageText.Content = "Health: " + damage;

            if (enemyCounter < 0)
            {
                makeEnemies(1, 6);
                enemyCounter = rand.Next(200, 400); // Reset enemy counter
            }

            HandlePlayerMovement();
            HandleBulletsAndEnemies();
            RemoveOffScreenItems();
            MoveBullets();

            if (score > 5)
            {
                enemyCounter = 30; // Enemies spawn faster
            }

            if (damage == 0)
            {
                ScoreService service = new();
                service.AddScore(new Score() { Score1 = this.score, EntryDate = DateTime.Now });

                EndGame();
            }

            if (rand.Next(0, 800) < 1) // Adjusted probability to 2%
            {
                makeHealthItem();
            }

            if (rand.Next(0, 800) < 1) // Adjusted probability to 0.1%
            {
                makeBulletItem();
            }

            // Use bulletCollected boolean for further logic if needed

            // Check if there are no enemies left and create new ones
            if (!MyCanvas.Children.OfType<Rectangle>().Any(r => (string)r.Tag == "enemy"))
            {
                if (score >= 100)
                {
                    makeEnemies(12, 15);
                }
                else if (score >= 70)
                {
                    makeEnemies(8, 14);
                }
                else if (score >= 40)
                {
                    makeEnemies(6, 10);
                }
                else
                {
                    makeEnemies(1, 3);
                }
                enemyCounter = rand.Next(200, 400); // Reset enemy counter
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

        private void MoveBullets()
        {
            var bulletsToRemove = new List<Rectangle>();
            foreach (UIElement element in MyCanvas.Children)
            {
                if (element is Rectangle bullet && (string)bullet.Tag == "bullet")
                {
                    // Move the first bullet straight up
                    if (Canvas.GetLeft(bullet) == Canvas.GetLeft(player) + player.Width / 2 - 10)
                    {
                        Canvas.SetTop(bullet, Canvas.GetTop(bullet));
                    }
                    // Move the second bullet diagonally
                    else if (Canvas.GetLeft(bullet) == Canvas.GetLeft(player) + player.Width / 2 + 10)
                    {
                        Canvas.SetTop(bullet, Canvas.GetTop(bullet));
                        Canvas.SetLeft(bullet, Canvas.GetLeft(bullet) + 5); // Adjust for diagonal movement
                    }

                    if (Canvas.GetTop(bullet) < 10)
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

                    if (Canvas.GetTop(x) + 50 > 800)
                    {
                        // if so first remove the enemy object
                        itemsToRemove.Add(x);
                        damage -= 10; // add 10 to the damage
                    }

                    if (playerHitBox.IntersectsWith(enemy))
                    {
                        damage -= 10;
                        itemsToRemove.Add(x);
                        explosionSound.Position = TimeSpan.Zero;
                        explosionSound.Play();
                    }
                }

                if (x is Rectangle && (string)x.Tag == "healthItem")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 3); // Move health item down
                    Rect healthItem = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (playerHitBox.IntersectsWith(healthItem))
                    {
                        damage = Math.Min(damage + 20, 100); // Ensure health does not exceed 100
                        itemsToRemove.Add(x); // Add to itemsToRemove to remove it later
                    }
                }

                if (x is Rectangle && (string)x.Tag == "bulletItem")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 3); // Move bullet item down
                    Rect bulletItem = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (playerHitBox.IntersectsWith(bulletItem))
                    {
                        countEatBullet++;
                        if (!hasCollectedBulletItem && countEatBullet == 1)
                        {
                            hasCollectedBulletItem = true; // Set the flag to true when bullet item is collected
                        }
                        else if (hasCollectedBulletItem && !hasCollectedSecondBulletItem && countEatBullet == 2)
                        {
                            hasCollectedSecondBulletItem = true; // Set the flag to true when second bullet item is collected
                        }
                        else if (hasCollectedSecondBulletItem && !hasCollectedThirdBulletItem && countEatBullet == 3)
                        {
                            hasCollectedThirdBulletItem = true; // Set the flag to true when third bullet item is collected
                        }
                        else if (hasCollectedThirdBulletItem && !hasCollectedForthBulletItem && countEatBullet == 4)
                        {
                            hasCollectedForthBulletItem = true; // Set the flag to true when forth bullet item is collected
                        }
                        itemsToRemove.Add(x); // Add to itemsToRemove to remove it later
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
            enemyShootingTimer.Stop();
            shootingTimer.Stop();
            bulletMoveTimer.Stop();
            lose.Play();
            introSound.Stop();
            shootSound.Stop();
            MessageBoxResult result = MessageBox.Show("You have lost!" + Environment.NewLine + "You have destroyed " + score + " Alien ships", "Game Over", MessageBoxButton.OK);

            if (result == MessageBoxResult.OK)
            {
                ScoreBoard scoreBoard = new();
                scoreBoard.ShowDialog();

                if (scoreBoard.choice == true)
                {
                    score = 0;
                    damage = 100;
                    enemyCounter = 100;

                    // Clear all bullets and enemies from the Canvas
                    foreach (var item in MyCanvas.Children.OfType<Rectangle>().Where(r => (string)r.Tag == "bullet" || (string)r.Tag == "enemy" || (string)r.Tag == "enemyBullet" || (string)r.Tag == "healthItem" || (string)r.Tag == "bulletItem").ToList())
                    {
                        MyCanvas.Children.Remove(item);
                    }

                    // Clear any bullets that may have been shot
                    itemsToRemove.Clear(); // Ensure itemsToRemove is cleared to reset bullet state

                    // Reset player position
                    Canvas.SetLeft(player, MyCanvas.ActualWidth / 2 - player.Width / 2);
                    Canvas.SetTop(player, MyCanvas.ActualHeight - player.Height - 10);

                    // Reset bullet collection flags
                    hasCollectedBulletItem = false;
                    hasCollectedSecondBulletItem = false;
                    hasCollectedThirdBulletItem = false;
                    hasCollectedForthBulletItem = false;

                    // Reset movement flags
                    moveLeft = false;
                    moveRight = false;
                    moveUp = false;
                    moveDown = false;

                    // Restart timers
                    gameTimer.Start();
                    shootingTimer.Start();
                    enemyShootingTimer.Start();
                    bulletMoveTimer.Start();

                    introSound.Play();
                    shootSound.Play();

                    // Ensure the player can control the plane again
                    MyCanvas.Focus();
                }
                else
                {
                    MessageBoxResult result1 = MessageBox.Show("Do you really want to out the game?", "Close?", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                    if (result1 == MessageBoxResult.Yes)
                    {
                        introSound.Stop();
                        Menu menu = new Menu();
                        menu.Show();
                        this.Close();
                    }
                }
            }
        }

        private void makeEnemies(int a, int b)
        {
            int numberOfEnemies = rand.Next(a, b); // Random number of enemies to create

            for (int i = 0; i < numberOfEnemies; i++)
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

                Canvas.SetTop(newEnemy, rand.Next(-100, -50)); // Randomize the initial position
                Canvas.SetLeft(newEnemy, rand.Next(0, (int)MyCanvas.ActualWidth - 50));
                MyCanvas.Children.Add(newEnemy);
            }
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
            damage = 100;
            enemyCounter = 100;

            // Clear all bullets and enemies from the Canvas
            foreach (var item in MyCanvas.Children.OfType<Rectangle>().Where(r => (string)r.Tag == "bullet" || (string)r.Tag == "enemy" || (string)r.Tag == "enemyBullet" || (string)r.Tag == "healthItem" || (string)r.Tag == "bulletItem").ToList())
            {
                MyCanvas.Children.Remove(item);
            }

            // Clear any bullets that may have been shot
            itemsToRemove.Clear(); // Ensure itemsToRemove is cleared to reset bullet state



            // Reset player position
            Canvas.SetLeft(player, MyCanvas.ActualWidth / 2 - player.Width / 2);
            Canvas.SetTop(player, MyCanvas.ActualHeight - player.Height - 10);

            // Reset bullet collection flags
            hasCollectedBulletItem = false;
            hasCollectedSecondBulletItem = false;

            // Restart timers
            gameTimer.Start();
            shootingTimer.Start();
            enemyShootingTimer.Start();
            bulletMoveTimer.Start();
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if (MenuPanel.Visibility == Visibility.Collapsed)
            {
                MenuPanel.Visibility = Visibility.Visible; // Hiện menu
            }
            else
            {
                MenuPanel.Visibility = Visibility.Collapsed; // Ẩn menu
            }
        }

        // Close the game
        private void CloseGame(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to out the game?", "Close?", MessageBoxButton.YesNo, MessageBoxImage.Stop);
            if (result == MessageBoxResult.Yes)
            {
                introSound.Stop();
                Menu menu = new Menu();
                menu.Show();
                this.Close();
            }
        }

        private void makeHealthItem()
        {
            ImageBrush healthSprite = new ImageBrush();
            healthSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/z6035094980366_cbabde7ef03bb75023d5a53014280bce.jpg"));

            Rectangle healthItem = new Rectangle
            {
                Tag = "healthItem",
                Width = 20,
                Height = 20,
                Fill = healthSprite
            };

            Canvas.SetTop(healthItem, -20);
            Canvas.SetLeft(healthItem, rand.Next(0, (int)MyCanvas.ActualWidth - 20));
            MyCanvas.Children.Add(healthItem);
        }
        private void makeBulletItem()
        {
            ImageBrush bulletSprite = new ImageBrush();
            bulletSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/lzTai.jpg"));

            Rectangle bulletItem = new Rectangle
            {
                Tag = "bulletItem",
                Width = 20,
                Height = 20,
                Fill = bulletSprite
            };

            Canvas.SetTop(bulletItem, -20);
            Canvas.SetLeft(bulletItem, rand.Next(0, (int)MyCanvas.ActualWidth - 20));
            MyCanvas.Children.Add(bulletItem);
        }

    }
}