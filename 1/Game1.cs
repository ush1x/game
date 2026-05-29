using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using FontStashSharp;
using System;
using System.IO;

namespace _1
{
    public enum GameState { MainMenu, Intro, Playing, Settings, Dead, LevelComplete, GameFinished }
    
    public class Game1 : Game
    {
        private MouseState prevMouseState;
        private Song menuMusic;
        private Song gameMusic;
        private SpriteFontBase font;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        private GameState currentState;
        private MainMenu mainMenu;
        private SettingsMenu settingsMenu;
        private Player player;
        
        // Текстуры
        private Texture2D[] playerTextures;
        private Texture2D[] enemyTextures;
        private Texture2D portalTexture;
        private Texture2D spikeTexture;
        private Texture2D[] platformTextures;
        private Texture2D[] backgrounds;
        private Texture2D pixelTexture;
        
        private List<Rectangle> platforms;
        private List<int> platformTypes;
        private List<Rectangle> spikes;
        private List<Enemy> enemies;
        
        private Rectangle portal;
        private Rectangle deathZone;
        private Vector2 startPosition;
        
        private float deathTimer;
        private const float deathDelay = 1f;
        
        private int currentLevel;
        private const int totalLevels = 6;

        private KeyboardState prevKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Пиксельная текстура для UI
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            
            // Загрузка текстур игрока
            playerTextures = new Texture2D[2];
            playerTextures[0] = Content.Load<Texture2D>("Sprites/Player/player_walk1");
            playerTextures[1] = Content.Load<Texture2D>("Sprites/Player/player_walk2");
            
            // Загрузка текстур врагов
            enemyTextures = new Texture2D[2];
            enemyTextures[0] = Content.Load<Texture2D>("Sprites/Enemies/robot_blueDrive1");
            enemyTextures[1] = Content.Load<Texture2D>("Sprites/Enemies/robot_redDrive1");
            
            // Загрузка объектов
            portalTexture = Content.Load<Texture2D>("Sprites/Objects/portal");
            spikeTexture = Content.Load<Texture2D>("Sprites/Objects/spike");
            
            // Загрузка платформ
            platformTextures = new Texture2D[5];
            platformTextures[0] = Content.Load<Texture2D>("Sprites/Platforms/ground");
            platformTextures[1] = Content.Load<Texture2D>("Sprites/Platforms/ground2");
            platformTextures[2] = Content.Load<Texture2D>("Sprites/Platforms/rock");
            platformTextures[3] = Content.Load<Texture2D>("Sprites/Platforms/rock2");
            platformTextures[4] = Content.Load<Texture2D>("Sprites/Platforms/stone_ground");
            
            // Загрузка фонов
            backgrounds = new Texture2D[6];
            backgrounds[0] = Content.Load<Texture2D>("Backgrounds/forest");
            backgrounds[1] = Content.Load<Texture2D>("Backgrounds/cave");
            backgrounds[2] = Content.Load<Texture2D>("Backgrounds/factory");
            backgrounds[3] = Content.Load<Texture2D>("Backgrounds/temple");
            backgrounds[4] = Content.Load<Texture2D>("Backgrounds/laboratory");
            backgrounds[5] = Content.Load<Texture2D>("Backgrounds/tower");
            
            mainMenu = new MainMenu(GraphicsDevice, backgrounds[0]);
            settingsMenu = new SettingsMenu(GraphicsDevice, backgrounds[3]);
            
            deathZone = new Rectangle(-100, 500, 1000, 100);
            // Загрузка шрифта
            try
            {
                string fontPath = Path.Combine(AppContext.BaseDirectory, "Content", "arial.ttf");
                if (!File.Exists(fontPath))
                    fontPath = @"Content\arial.ttf";
                
                var fontSystem = new FontSystem();
                fontSystem.AddFont(File.ReadAllBytes(fontPath));
                font = fontSystem.GetFont(22);
            }
            catch
            {
                font = null;
            }
            try
            {
                menuMusic = Content.Load<Song>("menu_music");
                gameMusic = Content.Load<Song>("background_music");
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.5f;
                PlayMusic(menuMusic);
            }
            catch
            {
                // Музыка не загрузилась — ничего страшного
            }
        }

        private void PlayMusic(Song song)
        {
            if (MediaPlayer.Queue.ActiveSong != song)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(song);
            }
        }

        private void BuildLevel(int level)
        {
            platforms = new List<Rectangle>();
            platformTypes = new List<int>();
            spikes = new List<Rectangle>();
            enemies = new List<Enemy>();
            
            switch (level)
            {
                case 1:
                    startPosition = new Vector2(50, 300);
                    platforms.Add(new Rectangle(0, 440, 350, 50)); platformTypes.Add(1);
                    platforms.Add(new Rectangle(500, 440, 300, 50)); platformTypes.Add(1);
                    platforms.Add(new Rectangle(340, 370, 140, 30)); platformTypes.Add(2);
                    portal = new Rectangle(760, 390, 40, 60);
                    break;

                case 2:
                    startPosition = new Vector2(50, 300);
                    platforms.Add(new Rectangle(0, 440, 350, 50)); platformTypes.Add(4);
                    platforms.Add(new Rectangle(500, 440, 300, 50)); platformTypes.Add(4);
                    platforms.Add(new Rectangle(330, 370, 140, 30)); platformTypes.Add(3);
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(620, 405), 35, 35, EnemyType.Turret, 1f, 0f, enemyTextures));
                    portal = new Rectangle(760, 390, 40, 60);
                    break;

                case 3:
                    startPosition = new Vector2(50, 400);
                    platforms.Add(new Rectangle(0, 440, 800, 50)); platformTypes.Add(0);
                    platforms.Add(new Rectangle(230, 380, 160, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(500, 320, 160, 25)); platformTypes.Add(2);
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(300, 405), 35, 35, EnemyType.Chaser, 0f, 120f, enemyTextures));
                    portal = new Rectangle(760, 390, 40, 60);
                    break;

                case 4:
                    startPosition = new Vector2(50, 300);
                    platforms.Add(new Rectangle(0, 440, 300, 50)); platformTypes.Add(3);
                    platforms.Add(new Rectangle(420, 440, 380, 50)); platformTypes.Add(3);
                    platforms.Add(new Rectangle(290, 380, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(490, 320, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(670, 260, 120, 25)); platformTypes.Add(2);
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(150, 405), 35, 35, EnemyType.Turret, 2f, 0f, enemyTextures));
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(700, 405), 35, 35, EnemyType.Turret, 2f, 0f, enemyTextures));
                    portal = new Rectangle(760, 210, 40, 60);
                    break;

                case 5:
                    startPosition = new Vector2(50, 400);
                    platforms.Add(new Rectangle(0, 440, 800, 50)); platformTypes.Add(0);
                    platforms.Add(new Rectangle(130, 360, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(330, 290, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(130, 230, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(370, 230, 120, 25)); platformTypes.Add(2);
                    spikes.Add(new Rectangle(450, 222, 25, 10));
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(680, 405), 35, 35, EnemyType.Turret, 1.5f, 0f, enemyTextures));
                    portal = new Rectangle(570, 180, 40, 60);
                    break;

                case 6:
                    startPosition = new Vector2(50, 400);
                    platforms.Add(new Rectangle(0, 440, 220, 50)); platformTypes.Add(4);
                    platforms.Add(new Rectangle(370, 440, 220, 50)); platformTypes.Add(4);
                    platforms.Add(new Rectangle(650, 440, 150, 50)); platformTypes.Add(4);
                    platforms.Add(new Rectangle(210, 360, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(420, 290, 120, 25)); platformTypes.Add(2);
                    platforms.Add(new Rectangle(580, 220, 120, 25)); platformTypes.Add(2);
                    spikes.Add(new Rectangle(250, 352, 20, 10));
                    spikes.Add(new Rectangle(480, 282, 20, 10));
                    spikes.Add(new Rectangle(620, 212, 20, 10));
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(450, 405), 35, 35, EnemyType.Turret, 1.2f, 0f, enemyTextures));
                    enemies.Add(new Enemy(GraphicsDevice, new Vector2(700, 405), 35, 35, EnemyType.Chaser, 0f, 150f, enemyTextures));
                    portal = new Rectangle(720, 130, 40, 60);
                    break;
            }
        }

        private void StartLevel(int level)
        {
            currentLevel = level;
            BuildLevel(level);
            player = new Player(GraphicsDevice, playerTextures);
            player.Position = startPosition;
            currentState = GameState.Playing;
        }

        private void NextLevel()
        {
            if (currentLevel >= totalLevels)
            {
                System.Console.WriteLine("Game Finished!");
                currentState = GameState.GameFinished;
            }
            else
                StartLevel(currentLevel + 1);
        }

        protected override void Update(GameTime gameTime)
        {
            prevMouseState = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            bool escJustPressed = keyboard.IsKeyDown(Keys.Escape) && 
                                  !prevKeyboardState.IsKeyDown(Keys.Escape);

            if (escJustPressed)
            {
                if (currentState == GameState.Intro || currentState == GameState.Playing || 
                    currentState == GameState.Settings || currentState == GameState.Dead ||
                    currentState == GameState.GameFinished)
                {
                    currentState = GameState.MainMenu;
                    PlayMusic(menuMusic);
                }
                else if (currentState == GameState.MainMenu)
                    Exit();
            }

            prevKeyboardState = keyboard;

            switch (currentState)
            {
                case GameState.MainMenu:
                    mainMenu.Update(gameTime);
                    if (mainMenu.SelectedAction == MainMenu.MenuAction.Play)
                        currentState = GameState.Intro;
                    else if (mainMenu.SelectedAction == MainMenu.MenuAction.Settings)
                        currentState = GameState.Settings;
                    else if (mainMenu.SelectedAction == MainMenu.MenuAction.Exit)
                        Exit();
                    break;

                case GameState.Intro:
                    if (keyboard.GetPressedKeys().Length > 0 || 
                        Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        PlayMusic(gameMusic);
                        StartLevel(1);
                    }
                    break;

                case GameState.Playing:
                    player.Update(gameTime, platforms);

                    foreach (var enemy in enemies)
                    {
                        enemy.Update(gameTime, player.Position, platforms);
                        if (player.Bounds.Intersects(enemy.Bounds))
                        {
                            currentState = GameState.Dead;
                            deathTimer = 0f;
                        }
                        if (enemy.HasBullet && player.Bounds.Intersects(enemy.BulletBounds))
                        {
                            currentState = GameState.Dead;
                            deathTimer = 0f;
                        }
                    }
                    
                    if (player.Bounds.Top > deathZone.Top)
                    {
                        currentState = GameState.Dead;
                        deathTimer = 0f;
                    }
                    
                    foreach (var spike in spikes)
                    {
                        if (player.Bounds.Intersects(spike))
                        {
                            currentState = GameState.Dead;
                            deathTimer = 0f;
                        }
                    }
                    
                    if (player.Bounds.Intersects(portal))
                        NextLevel();
                    break;

                case GameState.Dead:
                    deathTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (deathTimer >= deathDelay)
                        StartLevel(currentLevel);
                    break;

                case GameState.Settings:
                    settingsMenu.Update(gameTime);
                    MediaPlayer.Volume = settingsMenu.GetVolume();
                    if (settingsMenu.BackPressed)
                        currentState = GameState.MainMenu;
                    break;

                case GameState.GameFinished:
                    if (prevKeyboardState.GetPressedKeys().Length > 0 && 
                        keyboard.GetPressedKeys().Length == 0)
                    {
                        currentState = GameState.MainMenu;
                        PlayMusic(menuMusic);
                    }
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            switch (currentState)
            {
                case GameState.MainMenu:
                    mainMenu.Draw(_spriteBatch);
                    break;

                case GameState.Intro:
                    _spriteBatch.Draw(backgrounds[0], new Rectangle(0, 0, 800, 480), Color.DarkGray);
                    _spriteBatch.Draw(pixelTexture, new Rectangle(40, 40, 720, 400), Color.Black * 0.85f);
                    _spriteBatch.Draw(pixelTexture, new Rectangle(50, 50, 700, 380), Color.Green * 0.2f);
                    
                    if (font != null)
                    {
                        string line1 = "Вы - курьер. Во время доставки контейнера";
                        string line2 = "в лабораторию произошла авария.";
                        string line3 = "Нестабильная энергия вырвалась наружу.";
                        string line4 = "Город разрушен. Вам нужно добраться";
                        string line5 = "до центра лаборатории и остановить";
                        string line6 = "распространение энергии.";
                        string line7 = "Нажмите любую клавишу, чтобы начать...";
                        
                        float y = 100;
                        _spriteBatch.DrawString(font, line1, new Vector2(100, y), Color.White);
                        _spriteBatch.DrawString(font, line2, new Vector2(100, y + 30), Color.White);
                        _spriteBatch.DrawString(font, line3, new Vector2(100, y + 60), Color.White);
                        _spriteBatch.DrawString(font, line4, new Vector2(100, y + 100), Color.White);
                        _spriteBatch.DrawString(font, line5, new Vector2(100, y + 130), Color.White);
                        _spriteBatch.DrawString(font, line6, new Vector2(100, y + 160), Color.White);
                        _spriteBatch.DrawString(font, line7, new Vector2(150, 350), Color.LimeGreen);
                    }
                    break;

                case GameState.Playing:
                case GameState.Dead:
                    if (currentLevel >= 1 && currentLevel <= 6)
                        _spriteBatch.Draw(backgrounds[currentLevel - 1], new Rectangle(0, 0, 800, 480), Color.White);
                    
                    for (int i = 0; i < platforms.Count; i++)
                        _spriteBatch.Draw(platformTextures[platformTypes[i]], platforms[i], Color.White);
                    
                    foreach (var spike in spikes)
                        _spriteBatch.Draw(spikeTexture, spike, Color.White);
                    
                    foreach (var enemy in enemies)
                        enemy.Draw(_spriteBatch);
                    
                    _spriteBatch.Draw(portalTexture, portal, Color.White);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.Settings:
                    settingsMenu.Draw(_spriteBatch);
                    break;

                case GameState.GameFinished:
                    _spriteBatch.Draw(backgrounds[5], new Rectangle(0, 0, 800, 480), Color.White);
                    _spriteBatch.Draw(pixelTexture, new Rectangle(40, 40, 720, 400), Color.Black * 0.9f);
                    _spriteBatch.Draw(pixelTexture, new Rectangle(50, 50, 700, 380), Color.Gold * 0.3f);
                    
                    if (font != null)
                    {
                        string line1 = "Вы добрались до центра лаборатории.";
                        string line2 = "Источник нестабильной энергии отключён.";
                        string line3 = "Город спасён!";
                        string line4 = "Спасибо за игру!";
                        string line5 = "Нажмите любую клавишу для выхода в меню...";
                        
                        _spriteBatch.DrawString(font, line1, new Vector2(100, 120), Color.White);
                        _spriteBatch.DrawString(font, line2, new Vector2(100, 160), Color.White);
                        _spriteBatch.DrawString(font, line3, new Vector2(280, 210), Color.Gold);
                        _spriteBatch.DrawString(font, line4, new Vector2(280, 250), Color.Gold);
                        _spriteBatch.DrawString(font, line5, new Vector2(120, 330), Color.Gray);
                    }
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}