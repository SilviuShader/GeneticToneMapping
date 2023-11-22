using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal class GeneticAlgorithm
    {
        public  Chromosome       PreviousBest => _previousBest;

        private List<Chromosome> _population;
        private int              _populationSize;
        private Random           _random;

        private float            _crossoverRate;
        private float            _addGeneChance;
        private float            _removeGeneChance;
        private float            _weightMutation;

        private int              _innovationNumber;

        private Chromosome       _previousBest;

        public GeneticAlgorithm(int populationSize, float crossoverRate, float addGeneChance, float removeGeneChance, float weightMutation)
        {
            _populationSize = populationSize;
            _population = new List<Chromosome>();

            for (var i = 0; i < populationSize; i++)
                _population.Add(new Chromosome());

            _random = new Random(0);
            _crossoverRate = crossoverRate;
            _addGeneChance = addGeneChance;
            _removeGeneChance = removeGeneChance;
            _weightMutation = weightMutation;
            _innovationNumber = 0;
            _previousBest = _population[0];
        }

        public void Epoch(HDRImage referenceImage)
        {
            foreach (var individual in _population)
                SetFitness(individual, referenceImage);

            _previousBest = _population[0];
            for (var i = 1; i < _populationSize; i++)
                if (_population[i].Fitness > _previousBest.Fitness)
                    _previousBest = _population[i];

            var newPopulation    = new List<Chromosome>();
            var epochInnovations = new Dictionary<Type, int>();

            AddNBest(newPopulation, 2, 3);

            while (newPopulation.Count < _populationSize)
            {
                var parent1 = RouletteWheelSelection();
                var parent2 = RouletteWheelSelection();

                var child = Crossover(parent1, parent2);
                Mutate(epochInnovations, child);
                newPopulation.Add(child);
            }

            _population = newPopulation;
        }

        private void AddNBest(List<Chromosome> newPopulation, int copies, int best)
        {
            var individuals = _population.OrderByDescending(x => x.Fitness).Take(best);
            for (var i = 0; i < copies; i++) 
                newPopulation.AddRange(individuals);
        }

        private Chromosome RouletteWheelSelection()
        {
            var totalFitness = _population.Select(x => x.Fitness).Sum();
            var wheelPoint = _random.NextSingle() * totalFitness;
            var accumulatedFitness = 0.0f;

            foreach (var individual in _population)
            {
                accumulatedFitness += individual.Fitness;
                if (accumulatedFitness >= wheelPoint)
                    return individual;
            }

            return _population[0];
        }

        private Chromosome Crossover(Chromosome parent1, Chromosome parent2)
        {
            var child = new Chromosome();

            var bestParent  = parent1.Fitness > parent2.Fitness ?  parent1 : parent2;
            var worstParent = parent1.Fitness <= parent2.Fitness ? parent1 : parent2;
            
            var worstParentGenes = new Dictionary<int, Gene>();

            foreach (var gene in worstParent.Genes)
                worstParentGenes[gene.InnovationNumber] = gene;

            foreach (var gene in bestParent.Genes)
            {
                var newGene = new Gene
                {
                    ToneMap = (IToneMap)gene.ToneMap.Clone(),
                    InnovationNumber = gene.InnovationNumber
                };

                if (worstParentGenes.ContainsKey(gene.InnovationNumber))
                    if (_random.NextSingle() < _crossoverRate)
                        newGene.ToneMap = (IToneMap)worstParentGenes[gene.InnovationNumber].ToneMap.Clone();

                child.Genes.Add(newGene);
            }

            return child;
        }

        private void Mutate(IDictionary<Type, int> currentInnovations, Chromosome individual)
        {
            if (_random.NextSingle() < _addGeneChance)
                AddGene(currentInnovations, individual);

            if (_random.NextSingle() < _removeGeneChance)
                RemoveGene(individual);

            for (var geneIndex = 0; geneIndex < individual.Genes.Count; geneIndex++)
            {
                var gene = individual.Genes[geneIndex];

                for (var p = 0; p < gene.ToneMap.ParametersCount; p++)
                {
                    var val = gene.ToneMap.GetParameter(p);
                    val += (_random.NextSingle() * 2.0f - 1.0f) * _weightMutation;
                    gene.ToneMap.SetParameter(p, val);
                }

                gene.ToneMap.Weight += (_random.NextSingle() * 2.0f - 1.0f) * _weightMutation;
                gene.ToneMap.Weight = Math.Clamp(gene.ToneMap.Weight, _weightMutation, 1.0f);

                individual.Genes[geneIndex] = gene;
            }
        }

        private void AddGene(IDictionary<Type, int> currentInnovations, Chromosome individual)
        {
            var gene = new Gene
            {
                ToneMap = RandomToneMap()
            };
            int innovNumber;
            if (currentInnovations.ContainsKey(gene.ToneMap.GetType()))
            {
                innovNumber = currentInnovations[gene.ToneMap.GetType()];
            }
            else
            {
                innovNumber = _innovationNumber;
                currentInnovations[gene.ToneMap.GetType()] = innovNumber;
                _innovationNumber++;
            }
            gene.InnovationNumber = innovNumber;

            for (var i = 0; i < gene.ToneMap.ParametersCount; i++)
                gene.ToneMap.SetParameter(i, _random.NextSingle());

            gene.ToneMap.Weight = _weightMutation;

            individual.Genes.Add(gene);
        }

        private void RemoveGene(Chromosome individual)
        {
            if (individual.Genes.Count <= 0)
                return;

            var geneIndex = _random.Next(individual.Genes.Count);
            individual.Genes.RemoveAt(geneIndex);
        }

        private IToneMap RandomToneMap()
        {
            var rnd = _random.Next(3);
            return rnd switch
            {
                0 => new Reinhard(),
                1 => new TumblinRushmeier(),
                _ => new Uncharted2()
            };
        }

        private static void SetFitness(Chromosome individual, HDRImage referenceImage)
        {
            var toneMaps = individual.Genes.Select(x => x.ToneMap);
            var newColors = ToneMapper.ToneMap(referenceImage, toneMaps);

            var newFitness = ShannonEntropy(newColors) /* + CalculateColorfulness(newColors)*/;

            individual.Fitness = newFitness;
        }

        private static float ShannonEntropy(IReadOnlyCollection<Color> image)
        {
            var histogram = new int[256];
            var probabilities = new float[256];
            foreach (var col in image)
            {
                var gray = ((float)col.R + (float)col.G + (float)col.B) / 3.0f;
                var bin = (int)(gray);
                bin = Math.Clamp(bin, 0, 255);
                histogram[bin]++;
            }

            for (var i = 0; i < 256; i++)
                probabilities[i] = (float)histogram[i] / image.Count;

            var entropy = 0.0f;
            for (var i = 0; i < 256; i++)
            {
                if (probabilities[i] > 0)
                    entropy -= probabilities[i] * MathF.Log(probabilities[i], 2);
            }

            return entropy;
        }

        private static float CalculateColorfulness(IReadOnlyCollection<Color> image)
        {
            var meanColor = image.Aggregate(Vector3.Zero, (current, col) => current + col.ToVector3());
            meanColor /= image.Count;

            var colorfulness = 0.0f;

            foreach (var col in image)
            {
                var r = col.R / 255.0f;
                var g = col.G / 255.0f;
                var b = col.B / 255.0f;

                colorfulness += MathF.Pow(r - meanColor.X, 2);
                colorfulness += MathF.Pow(g - meanColor.Y, 2);
                colorfulness += MathF.Pow(b - meanColor.Z, 2);
            }

            colorfulness /= image.Count * 3;
            colorfulness = MathF.Sqrt(colorfulness);

            return colorfulness;
        }
    }
}
