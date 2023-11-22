﻿using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SharpEXR;

namespace GeneticToneMapping
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch           _spriteBatch;

        private HDRImage              _trainingImage;
        private GeneticAlgorithm      _algorithm;

        private Mutex                 _textureMutex;
        private Texture2D             _ldrTexture;

        private bool                  _appRunning;
        private Thread                _geneticThread;
        private float                 _bestFitness;

        private SpriteFont            _font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _appRunning = true;
            _bestFitness = 0.0f;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _trainingImage = new HDRImage("Images/Sample1.exr");

            _algorithm = new GeneticAlgorithm(150, 0.5f, 0.001f, 0.0005f, 0.01f);
            _ldrTexture = new Texture2D(GraphicsDevice, _trainingImage.Width, _trainingImage.Height, false, SurfaceFormat.Color);

            _font = Content.Load<SpriteFont>("AppFont");

            _textureMutex = new Mutex();
            _geneticThread = new Thread(RunGeneticAlgorithm);
            _geneticThread.Start();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                _appRunning = false;

            if (!_geneticThread.IsAlive && !_appRunning)
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            

            _spriteBatch.Begin();
            _textureMutex.WaitOne();
            _spriteBatch.Draw(_ldrTexture, Vector2.Zero, Color.White);
            _spriteBatch.DrawString(_font, _bestFitness.ToString(), new Vector2(10.0f, 10.0f), Color.White);
            _textureMutex.ReleaseMutex();
            _spriteBatch.End();


            base.Draw(gameTime);
        }

        private void RunGeneticAlgorithm()
        {
            while (_appRunning)
            {
                _algorithm.Epoch(_trainingImage);
                var colorData = ToneMapper.ToneMap(_trainingImage, _algorithm.PreviousBest.Genes.Select(x => x.ToneMap));
                _textureMutex.WaitOne();
                _bestFitness = _algorithm.PreviousBest.Fitness;
                _ldrTexture.SetData(colorData, 0, colorData.Length);
                _textureMutex.ReleaseMutex();
            }
        }
    }
}