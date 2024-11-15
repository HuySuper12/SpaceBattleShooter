using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Space_battle_shooter_WPF
{
    public partial class MainWindow : Window
    {

        private DispatcherTimer shootingTimer;
        private DispatcherTimer enemyShootingTimer;
        private DispatcherTimer bulletDown = new();


        private DispatcherTimer gameTimer = new DispatcherTimer();
        bool moveLeft, moveRight,moveUp,moveDown;
        List<Rectangle> itemstoremove = new List<Rectangle>();
        Random rand = new Random();
        int enemySpriteCounter;
        int enemyCounter = 100;
        int playerSpeed = 10;
        int limit = 50;
        int score = 0;
        int damage = 0;
        Rect playerHitBox;

        public MainWindow()
        {
            InitializeComponent();
            shootingTimer = new DispatcherTimer();
            shootingTimer.Interval = TimeSpan.FromSeconds(0.2);
            shootingTimer.Tick += ShootBullet;
            shootingTimer.Start();

            DispatcherTimer enemyShootTimer = new DispatcherTimer();
            enemyShootTimer.Interval = TimeSpan.FromSeconds(1); // Adjust as needed
            enemyShootTimer.Tick += EnemyShootBullet;
            enemyShootTimer.Start();

            DispatcherTimer bulletMoveTimer = new DispatcherTimer();
            bulletMoveTimer.Interval = TimeSpan.FromMilliseconds(30); // Adjust for smooth movement
            bulletMoveTimer.Tick += (s, e) => MoveEnemyBulletsDown();
            bulletMoveTimer.Start();


            enemyShootingTimer = new DispatcherTimer();
            enemyShootingTimer.Interval = TimeSpan.FromSeconds(1);
            enemyShootingTimer.Tick += EnemyShootBullet;
            enemyShootingTimer.Start();


            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += gameEngine;
            gameTimer.Start();
            MyCanvas.Focus();
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\purple.png"));
            bg.TileMode = TileMode.Tile;
            bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            MyCanvas.Background = bg;
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\player.png"));
            player.Fill = playerImage;
        }

        private void ShootBullet(object sender, EventArgs e)
        {
            Rectangle newBullet = new Rectangle
            {
                Tag = "bullet",
                Height = 20,
                Width = 5,
                Fill = Brushes.White,
                Stroke = Brushes.Cyan
            };
            Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
            Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);
            MyCanvas.Children.Add(newBullet);
        }

        private void EnemyShootBullet(object sender, EventArgs e)
        {
            // Temporary list to store new bullets to be added after iteration
            var bulletsToAdd = new List<Rectangle>();

            // Loop through the children to find enemies and queue bullets
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

                    // Position the bullet at the bottom of the enemy
                    Canvas.SetTop(enemyBullet, Canvas.GetTop(enemy) + enemy.Height);
                    Canvas.SetLeft(enemyBullet, Canvas.GetLeft(enemy) + enemy.Width / 2);

                    // Add bullet to temporary list
                    bulletsToAdd.Add(enemyBullet);
                }
            }

            // Now add all bullets to MyCanvas at once after the loop
            foreach (var bullet in bulletsToAdd)
            {
                MyCanvas.Children.Add(bullet);
            }
        }

        private void MoveEnemyBulletsDown()
        {
            var bulletsToRemove = new List<Rectangle>();

            // Iterate over MyCanvas.Children to find all bullets with Tag "enemyBullet"
            foreach (UIElement element in MyCanvas.Children)
            {
                if (element is Rectangle bullet && (string)bullet.Tag == "enemyBullet")
                {
                    // Move the bullet down
                    double currentTop = Canvas.GetTop(bullet);
                    Canvas.SetTop(bullet, currentTop + 5); // Adjust speed here if needed

                    // Check if the bullet is out of the game area and mark it for removal
                    if (currentTop > MyCanvas.ActualHeight)
                    {
                        bulletsToRemove.Add(bullet);
                    }
                }
            }

            // Remove bullets that are out of bounds
            foreach (var bullet in bulletsToRemove)
            {
                MyCanvas.Children.Remove(bullet);
            }
        }


        private void gameEngine(object? sender, EventArgs e)
        {
            // link the player hit box to the player rectangle
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            // reduce one from the enemy counter integer
            enemyCounter--;
            scoreText.Content = "Score: " + score; // link the score text to score integer
            damageText.Content = "Damaged " + damage; // link the damage text to damage integer
            // if enemy counter is less than 0
            if (enemyCounter < 0)
            {
                this.makeEnemies(); // run the make enemies function
                enemyCounter = limit; //reset the enemy counter to the limit integer
            }
            // player movement begins
            if (moveLeft && Canvas.GetLeft(player) > 0)
            {
                // if move left is true AND player is inside the boundary then move player to the left
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight && Canvas.GetLeft(player) + 90 < Application.Current.MainWindow.Width)
            {
                // if move right is true AND player left + 90 is less than the width of the form
                // then move the player to the right
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }
            if (moveUp && Canvas.GetTop(player) > 0)
            {
                // if move up is true AND player is inside the boundary then move player up
                Canvas.SetTop(player, Canvas.GetTop(player) - playerSpeed);
            }
            if (moveDown && Canvas.GetTop(player) + 90 < Application.Current.MainWindow.Height)
            {
                // if move down is true AND player top + 90 is less than the height of the form
                // then move the player down
                Canvas.SetTop(player, Canvas.GetTop(player) + playerSpeed);
            }

            // player movement ends
            // search for bullets, enemies and collision begins
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                // if any rectangle has the tag bullet in it
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    // move the bullet rectangle towards top of the screen
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);
                    // make a rect class with the bullet rectangles properties
                    Rect bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    // check if bullet has reached top part of the screen
                    if (Canvas.GetTop(x) < 10)
                    {
                        // if it has then add it to the item to remove list
                        itemstoremove.Add(x);
                    }
                    // run another for each loop inside of the main loop this one has a local variable called y
                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        // if y is a rectangle and it has a tag called enemy
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            // make a local rect called enemy and put the enemies properties into it
                            Rect enemy = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);
                            // now check if bullet and enemy is colliding or not
                            // if the bullet is colliding with the enemy rectangle
                            if (bullet.IntersectsWith(enemy))
                            {
                                itemstoremove.Add(x); // remove bullet
                                itemstoremove.Add(y); // remove enemy
                                score++; // add one to the score
                            }
                        }
                    }
                }
                // outside the second loop lets check for the enemy again
                if (x is Rectangle && (string)x.Tag == "enemy")
                {
                    // if we find a rectangle with the enemy tag
                    Canvas.SetTop(x, Canvas.GetTop(x) + 3); // move the enemy downwards
                    // make a new enemy rect for enemy hit box
                    Rect enemy = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    // first check if the enemy object has gone passed the player meaning
                    // its gone passed 700 pixels from the top
                    if (Canvas.GetTop(x) + 150 > 700)
                    {
                        // if so first remove the enemy object
                        itemstoremove.Add(x);
                        damage += 10; // add 10 to the damage
                    }
                    // if the player hit box and the enemy is colliding 
                    if (playerHitBox.IntersectsWith(enemy))
                    {
                        damage += 5; // add 5 to the damage
                        itemstoremove.Add(x); // remove the enemy object
                    }
                }
            }
            // search for bullets, enemies and collision ENDs
            // if the score is greater than 5
            if (score > 5)
            {
                limit = 20; // reduce the limit to 20
                // now the enemies will spawn faster
            }
            // if the damage integer is greater than 99
            if (damage > 50)
            {
                gameTimer.Stop(); // stop the main timer
                damageText.Content = "Damaged: 50"; // show this on the damaged text
                damageText.Foreground = Brushes.Red; // change the text colour to 100
                MessageBox.Show("You have lost!" + Environment.NewLine + "You have destroyed " + score + " Alien ships");
                // show the message box with the message inside of it
            }
            // removing the rectangles
            // check how many rectangles are inside of the item to remove list
            foreach (Rectangle y in itemstoremove)
            {
                // remove them permanently from the canvas
                MyCanvas.Children.Remove(y);
            }
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
                bulletDown.Stop();

                isPaused = true;
            }
            else
            {
                gameTimer.Start();
                shootingTimer.Start();
                enemyShootingTimer.Start();
                isPaused = false;
            }
        }

        // Restart the game
        private void RestartGame(object sender, RoutedEventArgs e)
        {
            // Reset game variables
            score = 0;
            damage = 0;
            enemyCounter = 100;
            limit = 50;

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
        }

        // Close the game
        private void CloseGame(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to quit the game?", "Quit?", MessageBoxButton.YesNo, MessageBoxImage.Stop);

            if(result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }
            if (e.Key == Key.Up)
            {
                moveUp = true;
            }
            if (e.Key == Key.Down)
            {
                moveDown = true;
            }
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }
            if (e.Key == Key.Up)
            {
                moveUp = false;
            }
            if (e.Key == Key.Down)
            {
                moveDown = false;
            }
            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
                Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);
                MyCanvas.Children.Add(newBullet);
            }
        }

        private void makeEnemies()
        {
            ImageBrush enemySprite = new ImageBrush();
            enemySpriteCounter = rand.Next(1, 6); // Adjusted to include 5 in the range
            switch (enemySpriteCounter)
            {
                case 1:
                    enemySprite.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\1.png"));
                    break;
                case 2:
                    enemySprite.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\2.png"));
                    break;
                case 3:
                    enemySprite.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\3.png"));
                    break;
                case 4:
                    enemySprite.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\4.png"));
                    break;
                case 5:
                    enemySprite.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\5.png"));
                    break;
                default:
                    enemySprite.ImageSource = new BitmapImage(new Uri("D:\\Space battle shooter WPF\\Space battle shooter WPF\\Space battle shooter WPF\\Images\\1.png"));
                    break;
            }

            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Width = 50,
                Height = 50,
                Fill = enemySprite
            };

            Canvas.SetTop(newEnemy, -100); // Start above the visible area
            Canvas.SetLeft(newEnemy, rand.Next(0, (int)MyCanvas.ActualWidth - 50)); // Random horizontal position

            MyCanvas.Children.Add(newEnemy);
        }
    }
}
