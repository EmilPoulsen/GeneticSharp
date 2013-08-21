using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Selections;
using HelperSharp;
using System.Linq;

namespace GeneticSharp.Domain.Populations
{
	/// <summary>
	/// Represents a population of candidate solutions (chromosomes).
	/// </summary>
	public class Population
    {
		#region Events
		/// <summary>
		/// Occurs when best chromosome changed.
		/// </summary>
		public event EventHandler BestChromosomeChanged;
		#endregion

        #region Fields
        private IChromosome m_adamChromosome;   
		private Generation m_previousGeneration;
        #endregion

        #region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="GeneticSharp.Domain.Populations.Population"/> class.
		/// </summary>
		/// <param name="minSize">The minimum size (chromosomes).</param>
		/// <param name="maxSize">The maximum size (chromosomes).</param>
		/// <param name="adamChromosome">The original chromosome of all population ;).</param>
	    public Population(int minSize, 
                          int maxSize,
                          IChromosome adamChromosome)
		{
            if (minSize < 2)
            {
                throw new ArgumentOutOfRangeException("minSize", "The minimum size for a population is 2 chromosomes.");
            }

            if (maxSize < minSize)
            {
                throw new ArgumentOutOfRangeException("maxSize", "The maximum size for a population should be equal or greater than minimum size.");
            }

            ExceptionHelper.ThrowIfNull("adamChromosome", adamChromosome);
        
			MinSize = minSize;
            MaxSize = maxSize;
            EliteNumber = 2;
            m_adamChromosome = adamChromosome;
			Generations = new List<Generation> ();
            GenerationStrategy = new TrackingGenerationStrategy();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the generations.
        /// <remarks>
        /// The information of Generations can vary depending of the IGenerationStrategy used.
        /// </remarks>
		/// </summary>
		/// <value>The generations.</value>
		public IList<Generation> Generations { get; private set; }

		/// <summary>
		/// Gets the current generation.
		/// </summary>
		/// <value>The current generation.</value>
		public Generation CurrentGeneration { get; private set; }

        /// <summary>
        /// Gets the total number of generations executed.
        /// <remarks>
        /// Use this information to know how many generations have been executed, because Generations.Count can vary depending of the IGenerationStrategy used.
        /// </remarks>
        /// </summary>
        public int GenerationsNumber { get; private set; }

		/// <summary>
		/// Gets the minimum size.
		/// </summary>
		/// <value>The minimum size.</value>
		public int MinSize { get; private set; }

		/// <summary>
		/// Gets the size of the max.
		/// </summary>
		/// <value>The size of the max.</value>
        public int MaxSize { get; private set; }

        /// <summary>
        /// Gets or sets the number of elite chromosomes that will be copied to each generation.
        /// </summary>
        /// <remarks>
        /// The default value is 2.
        /// </remarks>
        public int EliteNumber { get; set; }

		/// <summary>
		/// Gets the best chromosome.
		/// </summary>
		/// <value>The best chromosome.</value>
		public IChromosome BestChromosome { get; private set; }

        /// <summary>
        /// Gets os sets the generation strategy.
        /// </summary>
        public IGenerationStrategy GenerationStrategy { get; set; }
		#endregion

		#region Public methods
		/// <summary>
		/// Creates the initial generation.
		/// </summary>
		/// <returns>The initial generation.</returns>
		public void CreateInitialGeneration ()
		{
			var chromosomes = new List<IChromosome> ();

			for(int i = 0; i < MinSize; i++)
			{
				var c = m_adamChromosome.CreateNew ();
				chromosomes.Add (c);
			}

			CreateNewGeneration(chromosomes);
		}
	
		/// <summary>
		/// Creates a new generation.
		/// </summary>
		/// <returns>The new generation.</returns>
		/// <param name="chromosomes">Chromosomes.</param>
		public void CreateNewGeneration(IList<IChromosome> chromosomes)
		{
            ExceptionHelper.ThrowIfNull("chromosomes", chromosomes);

			m_previousGeneration = CurrentGeneration;

            CurrentGeneration = new Generation(++GenerationsNumber, chromosomes);
			Generations.Add (CurrentGeneration);
            GenerationStrategy.RegisterNewGeneration(this);
		}

		/// <summary>
		/// Ends the current generation.
		/// </summary>		
		public void EndCurrentGeneration()
		{
			if (m_previousGeneration != null)
			{
				var eliteChromosomes = m_previousGeneration.Chromosomes.Take(EliteNumber);

				foreach (var e in eliteChromosomes)
				{
					CurrentGeneration.Chromosomes.Add(e.Clone());
				}
			}

			CurrentGeneration.End (MaxSize);

			if (BestChromosome != CurrentGeneration.BestChromosome) {
				BestChromosome = CurrentGeneration.BestChromosome;

				if (BestChromosomeChanged != null) {
					BestChromosomeChanged (this, EventArgs.Empty);
				}
			}
		}
		#endregion
	}
}