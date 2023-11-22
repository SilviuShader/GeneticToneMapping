using System.Collections.Generic;

namespace GeneticToneMapping
{
    internal class Chromosome
    {
        public List<Gene> Genes   { get; }
        public float      Fitness { get; set; }

        public Chromosome()
        {
            Fitness = 0.0f;
            Genes = new List<Gene>();
        }
    }
}
