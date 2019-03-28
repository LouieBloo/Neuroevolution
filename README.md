# Neuroevolution
Playing around training neural networks with genetic algorithms. Currently have the retro game "Snake" built and have successfully trained a neural network using a standard GA approach (fitness,selection,crossover,mutation) to suruvie and score upwards 70+ food eaten.

## Snake
### 20x20 grid, Touching walls or tail is death
- Population: 100
- Selection: Top 10%
- Fitness function: # of food eaten
- Mutation rate: 10%
- Mutation amount: 0.1