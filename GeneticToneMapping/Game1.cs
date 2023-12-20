using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenCvSharp;
using SharpEXR;

namespace GeneticToneMapping
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch           _spriteBatch;

        private HDRImage              _trainingImage;
        private HDRImage              _displayImage;

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
            //_trainingImage.Slice(128, 128);
            _displayImage = new HDRImage("Images/Sample1.exr");
            GeneticAlgorithm.SpecieParameters sp = new GeneticAlgorithm.SpecieParameters();

            sp.C1        = 1.0f;
            sp.C2        = 1.0f;
            sp.C3        = 4.0f;
            sp.N         = 1.0f;
            sp.Threshold = 1.5f;

            _algorithm = new GeneticAlgorithm(150, 0.5f, 0.01f, 0.1f, 0.01f, sp);
            _ldrTexture = new Texture2D(GraphicsDevice, _displayImage.Width, _displayImage.Height, false, SurfaceFormat.Color);

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

        private unsafe void RunGeneticAlgorithm()
        {
            Vec3f[] data = new Vec3f[_trainingImage.Width * _trainingImage.Height];
            var colorData = new Color[data.Length];

            int gen = 0;

            while (_appRunning)
            {
                _algorithm.Epoch(_trainingImage);
                var ldrImage = ToneMapper.ToneMap(_displayImage, _algorithm.PreviousBest.Genes.Select(x => x.ToneMap));
                _textureMutex.WaitOne();
                _bestFitness = _algorithm.PreviousBest.Fitness;

                fixed (void* ptr = data)
                    Unsafe.CopyBlock(ptr, ldrImage.Data.DataPointer, (uint)ldrImage.Width * (uint)ldrImage.Height * 3 * sizeof(float));

                for (var i = 0; i < data.Length; i++)
                    colorData[i] = new Color(data[i].Item0, data[i].Item1, data[i].Item2, 1.0f);

                _ldrTexture.SetData(colorData, 0, colorData.Length);
                _textureMutex.ReleaseMutex();

                gen++;
            }
        }
    }
}
