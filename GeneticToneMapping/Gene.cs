namespace GeneticToneMapping
{
    internal struct Gene
    {
        public IToneMap ToneMap { get; set; }
        public int      InnovationNumber;

        public static Gene Create<T>(int innov) where T : struct, IToneMap
        {
            var result = new Gene
            {
                ToneMap          = new T(),
                InnovationNumber = innov
            };

            return result;
        }
    }
}
