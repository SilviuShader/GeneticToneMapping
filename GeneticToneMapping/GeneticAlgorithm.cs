using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using OpenCvSharp;

namespace GeneticToneMapping
{
    internal class GeneticAlgorithm
    {
        public struct SpecieParameters
        {
            public float C1;
            public float C2;
            public float C3;
            public float N;
            public float Threshold;
        }
        
        class Specie
        {
            public Chromosome       Representative => Inidividuals[0];

            public List<Chromosome> Inidividuals { get; }

            public Specie(Chromosome representative)
            {
                Inidividuals = new List<Chromosome>();
                Inidividuals.Add(representative);
            }
        }

        public  Chromosome       PreviousBest => _previousBest;

        private List<Specie>     _population;
        private int              _populationSize;
        private Random           _random;

        private float            _crossoverRate;
        private float            _addGeneChance;
        private float            _removeGeneChance;
        private float            _weightMutation;

        private SpecieParameters _specieParameters;

        private int              _innovationNumber;

        private Chromosome       _previousBest;

        public GeneticAlgorithm(
            int              populationSize, 
            float            crossoverRate, 
            float            addGeneChance, 
            float            removeGeneChance, 
            float            weightMutation,
            SpecieParameters specieParameters)
        {
            _populationSize = populationSize;
            _population = new List<Specie>();

            var firstSpecie = new Specie(new Chromosome());
            while (firstSpecie.Inidividuals.Count < populationSize)
                firstSpecie.Inidividuals.Add(new Chromosome());
            _population.Add(firstSpecie);
            
            _random           = new Random(0);
            _crossoverRate    = crossoverRate;
            _addGeneChance    = addGeneChance;
            _removeGeneChance = removeGeneChance;
            _weightMutation   = weightMutation;
            _innovationNumber = 0;
            _previousBest     = firstSpecie.Representative;
            _specieParameters = specieParameters;
        }

        public void Epoch(HDRImage referenceImage)
        {
            foreach (var specie in _population)
            {
                foreach (var individual in specie.Inidividuals)
                {
                    SetFitness(individual, referenceImage);
                    individual.Fitness /= specie.Inidividuals.Count;
                }
            }

            _previousBest = _population[0].Representative;
            foreach (var specie in _population)
                foreach (var individual in specie.Inidividuals)
                    if (individual.Fitness > _previousBest.Fitness)
                        _previousBest = individual;

            var newPopulation    = new List<Specie>();
            var epochInnovations = new Dictionary<Type, int>();

            AddNBest(newPopulation, 2, 3);

            var populationSize = newPopulation.Select(x=>x.Inidividuals.Count).Sum();
            while (populationSize < _populationSize)
            {
                var parent1 = RouletteWheelSelection();
                var parent2 = RouletteWheelSelection();

                var child = Crossover(parent1, parent2);
                Mutate(epochInnovations, child);

                InsertToPopulation(newPopulation, child);
                populationSize++;
            }

            _population = newPopulation;
        }

        private float CompatibilityDistance(Chromosome individual1, Chromosome individual2)
        {
            var individual1Genes = new Dictionary<int, Gene>();
            var individual2Genes = new Dictionary<int, Gene>();

            var maxGene1 = -1;
            var maxGene2 = -1;

            foreach (var gene in individual1.Genes)
            {
                individual1Genes[gene.InnovationNumber] = gene;
                maxGene1 = Math.Max(maxGene1, gene.InnovationNumber);
            }

            foreach (var gene in individual2.Genes)
            {
                individual2Genes[gene.InnovationNumber] = gene;
                maxGene2 = Math.Max(maxGene2, gene.InnovationNumber);
            }

            var excess   = 0;
            var disjoint = 0;
            var w        = 0.0f;

            foreach (var gene in individual1.Genes)
            {
                if (!individual2Genes.ContainsKey(gene.InnovationNumber))
                {
                    if (maxGene2 >= gene.InnovationNumber)
                        disjoint++;
                    else
                        excess++;
                }
            }

            foreach (var gene in individual2.Genes)
            {
                if (!individual1Genes.ContainsKey(gene.InnovationNumber))
                {
                    if (maxGene1 >= gene.InnovationNumber)
                        disjoint++;
                    else
                        excess++;
                }
            }

            foreach (var gene in individual1.Genes)
            {
                if (individual2Genes.ContainsKey(gene.InnovationNumber))
                {
                    var otherGene = individual2Genes[gene.InnovationNumber];
                    w += MathF.Abs( gene.ToneMap.Weight - otherGene.ToneMap.Weight);
                    for (var parameterIndex = 0; parameterIndex < gene.ToneMap.ParametersCount; parameterIndex++)
                    {
                        var parameter1 = gene.ToneMap.GetParameter(parameterIndex);
                        var parameter2 = otherGene.ToneMap.GetParameter(parameterIndex);
                        gene.ToneMap.GetParameterRange(parameterIndex, out var minVal, out var maxVal);
                        parameter1 = (parameter1 - minVal) / (maxVal - minVal);
                        parameter2 = (parameter2 - minVal) / (maxVal - minVal);
                        w += MathF.Abs(parameter2 - parameter1);
                    }
                }
            }

            return excess   * (_specieParameters.C1 / _specieParameters.N) +
                   disjoint * (_specieParameters.C2 / _specieParameters.N) +
                   _specieParameters.C3 * w;
        }

        private void InsertToPopulation(List<Specie> newPopulation, Chromosome individual)
        {
            Specie targetSpecie = null;
            foreach (var specie in newPopulation)
            {
                if (CompatibilityDistance(specie.Representative, individual) <= _specieParameters.Threshold)
                {
                    targetSpecie = specie;
                    break;
                }
            }

            if (targetSpecie != null)
                targetSpecie.Inidividuals.Add(individual);
            else
                newPopulation.Add(new Specie(individual));
        }

        private void AddNBest(List<Specie> newPopulation, int copies, int best)
        {
            var individuals = _population.SelectMany(x => x.Inidividuals).OrderByDescending(x => x.Fitness).Take(best).ToArray();
            for (var i = 0; i < copies; i++) 
                foreach (var individual in individuals)
                    InsertToPopulation(newPopulation, individual);;
        }

        private Chromosome RouletteWheelSelection()
        {
            var totalFitness = _population.SelectMany(x => x.Inidividuals).Select(x => x.Fitness).Sum();
            var wheelPoint = _random.NextSingle() * totalFitness;
            var accumulatedFitness = 0.0f;

            foreach (var specie in _population)
            {
                foreach (var individual in specie.Inidividuals)
                {
                    accumulatedFitness += individual.Fitness;
                    if (accumulatedFitness >= wheelPoint)
                        return individual;
                }
            }

            return _population[0].Representative;
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
                    gene.ToneMap.GetParameterRange(p, out var minVal, out var maxVal);
                    val += (_random.NextSingle() * 2.0f - 1.0f) * _weightMutation * (maxVal - minVal);
                    val = Math.Clamp(val, minVal, maxVal);
                    gene.ToneMap.SetParameter(p, val);
                }

                gene.ToneMap.Weight += (_random.NextSingle() * 2.0f - 1.0f) * _weightMutation;
                gene.ToneMap.Weight = Math.Clamp(gene.ToneMap.Weight, 0.0f, 1.0f);

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
            {
                gene.ToneMap.GetParameterRange(i, out var minVal, out var maxVal);
                gene.ToneMap.SetParameter(i, MathHelper.Lerp(minVal, maxVal, _random.NextSingle()));
            }

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
            
            var rnd = _random.Next(5);
            return rnd switch
            {
                0 => new Reinhard(),
                1 => new TumblinRushmeier(),
                2 => new Uncharted2(),
                3 => new Drago(),
                _ => new Mantiuk()
            };
        }

        private static void SetFitness(Chromosome individual, HDRImage referenceImage)
        {
            var toneMaps = individual.Genes.Select(x => x.ToneMap);
            var ldrImage = ToneMapper.ToneMap(referenceImage, toneMaps);

            // TODO: Calculate entropy here
            var newFitness = 
                ShannonEntropy(ldrImage) + 
                CalculateColorfulness(ldrImage) * 0.1f + 
                (1.0f / CalcBlurriness(ldrImage)) * 0.00001f;

            individual.Fitness = newFitness;
        }

        private static float ShannonEntropy(LDRImage ldr)
        {
            Vec3f[] data = new Vec3f[ldr.Width * ldr.Height]; // TODO: Optimize this new
            OpenCVHelper.CopyMat(ref data, ldr.Data);

            var histogram = new int[256];
            var probabilities = new float[256];
            foreach (var col in data)
            {
                var gray = (col.Item0 + col.Item1 + col.Item2) * (255.0f / 3.0f);
                var bin = (int)(gray);
                bin = Math.Clamp(bin, 0, 255);
                histogram[bin]++;
            }

            for (var i = 0; i < 256; i++)
                probabilities[i] = (float)histogram[i] / data.Length;

            var entropy = 0.0f;
            for (var i = 0; i < 256; i++)
            {
                if (probabilities[i] > 0)
                    entropy -= probabilities[i] * MathF.Log(probabilities[i], 2);
            }

            return entropy;
        }

        private static float CalculateColorfulness(LDRImage ldr)
        {
            var mean = ldr.Data.Mean();

            var term = (mean - ldr.Data);

            var totalScalar = (term.Mul(term)).ToMat().Sum();
            var total = (float)(totalScalar.Val0 + totalScalar.Val1 + totalScalar.Val2);

            var colorfulness = total / 3.0f;
            colorfulness = MathF.Sqrt(colorfulness);

            return colorfulness;
        }

        private static float CalcBlurriness(LDRImage ldr)
        {
            //Mat Gx, Gy;
            //Sobel(src, Gx, CV_32F, 1, 0);
            //Sobel(src, Gy, CV_32F, 0, 1);
            //double normGx = norm(Gx);
            //double normGy = norm(Gy);
            //double sumSq = normGx * normGx + normGy * normGy;
            //return static_cast<float>(1. / (sumSq / src.size().area() + 1e-6));

            Mat gx = new(), gy = new();
            Cv2.Sobel(ldr.Data, gx, MatType.CV_32F, 1, 0);
            Cv2.Sobel(ldr.Data, gy, MatType.CV_32F, 0, 1);
            float normGx = (float)Cv2.Norm(gx);
            float normGy = (float)Cv2.Norm(gy);
            float sumSq = normGx * normGx + normGy * normGy;
            return (float)(1.0f / (sumSq / ldr.Data.Size().Width * ldr.Data.Size().Height + 1e-6));
        }
    }
}
