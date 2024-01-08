using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using OpenCvSharp;
using SharpEXR;

namespace GeneticToneMapping
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch           _spriteBatch;

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

            var gaParams = GeneticAlgorithm.GenericAlgorithmParameters.Default;
            var paramsFile = "config.json";

            bool loadedFile = false;

            if (File.Exists(paramsFile))
            {
                try
                {
                    gaParams = JsonConvert.DeserializeObject<GeneticAlgorithm.GenericAlgorithmParameters>(File.ReadAllText(paramsFile));
                    loadedFile = true;
                }
                catch
                {
                    loadedFile = false;
                }
            }

            if (!loadedFile)
            {
                gaParams = GeneticAlgorithm.GenericAlgorithmParameters.Default;
                File.WriteAllText(paramsFile, JsonConvert.SerializeObject(gaParams, Formatting.Indented));
            }

            _algorithm = new GeneticAlgorithm(gaParams);

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
            if (_ldrTexture != null)
                _spriteBatch.Draw(_ldrTexture, Vector2.Zero, Color.White);
            _spriteBatch.DrawString(_font, _bestFitness.ToString(), new Vector2(10.0f, 10.0f), Color.White);
            _textureMutex.ReleaseMutex();
            
            _spriteBatch.End();


            base.Draw(gameTime);
        }

        private unsafe void RunGeneticAlgorithm()
        {
            var gen = 0;

            while (_appRunning)
            {
                _algorithm.Epoch();

                var testImageIndex = gen % _algorithm.TestImages.Length;
                var testingImage = _algorithm.TestImages[testImageIndex];

                var data = new Vec3f[testingImage.Width * testingImage.Height];
                var colorData = new Color[data.Length];

                var ldrImage = ToneMapper.ToneMap(testingImage, _algorithm.PreviousBest.Genes.Select(x => x.ToneMap));
                _textureMutex.WaitOne();
                _bestFitness = _algorithm.PreviousBest.InitialFitness;

                fixed (void* ptr = data)
                    Unsafe.CopyBlock(ptr, ldrImage.Data.DataPointer, (uint)ldrImage.Width * (uint)ldrImage.Height * 3 * sizeof(float));

                for (var i = 0; i < data.Length; i++)
                    colorData[i] = new Color(data[i].Item0, data[i].Item1, data[i].Item2, 1.0f);

                _ldrTexture = new Texture2D(GraphicsDevice, testingImage.Width, testingImage.Height, false, SurfaceFormat.Color);
                _ldrTexture.SetData(colorData, 0, colorData.Length);
                _textureMutex.ReleaseMutex();

                gen++;
            }
        }
    }
}
