import random
import copy
import numpy as np
import matplotlib.pyplot as plt

# the number of jobs, machines in stage 1 and stage 2
NUM_JOBS = 30
NUM_STAGES = 2
STAGE=[1,3,6]
NUM_MACHINES_STAGE1 = 2
NUM_MACHINES_STAGE2 = 3

# the processing times of each job in stage 1 and stage 2

# processing_times_stage1 = [random.randint(1, 10) for _ in range(NUM_JOBS)]
# processing_times_stage2 = [random.randint(1, 10) for _ in range(NUM_JOBS)]


# small-scale problem
# processing_times_stage1 = [2, 4, 5, 3, 6, 7, 1, 4, 7, 4]
# processing_times_stage2 = [5, 8, 3, 9, 9, 9, 9, 1, 23, 3]

# medium-scale problem
# processing_times_stage1 = [2, 4, 5, 3, 6, 7, 1, 4, 7, 4, 5, 8, 3, 9, 9, 9, 9, 1, 23, 3]
# processing_times_stage2 = [5, 8, 3, 9, 9, 9, 9, 1, 23, 3, 2, 4, 5, 3, 6, 7, 1, 4, 7, 4]

# large-scale problem
processing_times_stage1 = [2, 4, 5, 3, 6, 7, 1, 4, 7, 4, 5, 8, 3, 9, 9, 9, 9, 1, 23, 3, 2, 4, 5, 3, 6, 7, 1, 4, 7, 4, 5, 8, 3, 9, 9, 9, 9, 1, 23, 3]
processing_times_stage2 = [5, 8, 3, 9, 9, 9, 9, 1, 23, 3, 2, 4, 5, 3, 6, 7, 1, 4, 7, 4, 2, 4, 5, 3, 6, 7, 1, 4, 7, 4, 5, 8, 3, 9, 9, 9, 9, 1, 23, 3]

# the parameters of genetic algorithm
POPULATION_SIZE = 20     # the number of chromosomes in the population
NUM_GENERATIONS = 20     # the number of generations
CROSSOVER_RATE = 0.8     # the probability of crossover
MUTATION_RATE = 0.2      # the probability of mutation
TOURNAMENT_SIZE = 5      # the size of tournament selection

# generate the initial population with the random-key representation
# random key representation: each gene is a random number between the lower and upper bounds of the corresponding job
def generate_initial_population():
    population = []
    for i in range(POPULATION_SIZE):
        base_chromosome = np.array([])
        for j in range(NUM_STAGES):
            base_chromosome=np.concatenate((base_chromosome, np.random.uniform(STAGE[j], STAGE[j+1], NUM_JOBS)))
        population.append(list(base_chromosome))
    return population

# decode the chromosome and calculate the makespan
def calculate_makespan(chromosome):
    decoded_chromosome = [[] for _ in range(NUM_MACHINES_STAGE1 + NUM_MACHINES_STAGE2)]

    start_time = [[] for _ in range(NUM_MACHINES_STAGE1 + NUM_MACHINES_STAGE2)]
    finish_time = [[] for _ in range(NUM_MACHINES_STAGE1 + NUM_MACHINES_STAGE2)]

    chromosome = np.array(chromosome)
    for i in range(len(decoded_chromosome)):
        filtered_values = chromosome[(chromosome >= (i+1)) & (chromosome <= (i+2))]
        indices = np.where((chromosome >= (i+1)) & (chromosome <= (i+2)))
        filtered_values_with_indices = list(zip(indices[0], filtered_values))
        sorted_filtered_values_with_indices = sorted(filtered_values_with_indices, key=lambda x: x[1])
        decoded_chromosome[i] = [x[0] for x in sorted_filtered_values_with_indices]

    chromosome = list(chromosome)



# Buffer for the jobs finished in stage 1
    for i in range(NUM_MACHINES_STAGE1):
        for j in range(len(decoded_chromosome[i])):
            if j == 0:
                start_time[i].extend([0])
            else:
                start_time[i].extend([finish_time[i][j-1]])
            finish_time[i].extend([start_time[i][-1] + processing_times_stage1[decoded_chromosome[i][j]]])

    for i in range(NUM_MACHINES_STAGE2):
        for j in range(len(decoded_chromosome[i+NUM_MACHINES_STAGE1])):
            exists_in_machine1 = np.where(decoded_chromosome[0] == (decoded_chromosome[i+NUM_MACHINES_STAGE1][j]-NUM_JOBS))[0]
            exists_in_machine2 = np.where(decoded_chromosome[1] == (decoded_chromosome[i+NUM_MACHINES_STAGE1][j]-NUM_JOBS))[0]
            if len(exists_in_machine1) >0:
                # print("*********** the first machine ************")
                start_time[i+NUM_MACHINES_STAGE1].extend([finish_time[0][exists_in_machine1[0]]])
                finish_time[i+NUM_MACHINES_STAGE1].extend([start_time[i+NUM_MACHINES_STAGE1][-1] + processing_times_stage2[decoded_chromosome[i+NUM_MACHINES_STAGE1][j]-NUM_JOBS]])
            elif len(exists_in_machine2) >0:
                # print("*********** the second machine ************")
                start_time[i+NUM_MACHINES_STAGE1].extend([finish_time[1][exists_in_machine2[0]]])
                finish_time[i+NUM_MACHINES_STAGE1].extend([start_time[i+NUM_MACHINES_STAGE1][-1] + processing_times_stage2[decoded_chromosome[i+NUM_MACHINES_STAGE1][j]-NUM_JOBS]])

    maxspan=[]


    for i in range(len(finish_time)):
        if len(finish_time[i]) == 0:
            maxspan.extend([0])
        else:
            maxspan.extend([max(finish_time[i])])

    makespan = max(maxspan)

    return makespan, start_time, finish_time


def calculate_makespan_gantt(chromosome):
    decoded_chromosome = [[] for _ in range(NUM_MACHINES_STAGE1 + NUM_MACHINES_STAGE2)]

    start_time = [[] for _ in range(NUM_MACHINES_STAGE1 + NUM_MACHINES_STAGE2)]
    finish_time = [[] for _ in range(NUM_MACHINES_STAGE1 + NUM_MACHINES_STAGE2)]

    chromosome = np.array(chromosome)
    for i in range(len(decoded_chromosome)):
        filtered_values = chromosome[(chromosome >= (i+1)) & (chromosome <= (i+2))]
        indices = np.where((chromosome >= (i+1)) & (chromosome <= (i+2)))
        filtered_values_with_indices = list(zip(indices[0], filtered_values))
        sorted_filtered_values_with_indices = sorted(filtered_values_with_indices, key=lambda x: x[1])
        decoded_chromosome[i] = [x[0] for x in sorted_filtered_values_with_indices]

    chromosome = list(chromosome)



# Buffer for the jobs finished in stage 1
    for i in range(NUM_MACHINES_STAGE1):
        for j in range(len(decoded_chromosome[i])):
            if j == 0:
                start_time[i].extend([0])
            else:
                start_time[i].extend([finish_time[i][j-1]])
            finish_time[i].extend([start_time[i][-1] + processing_times_stage1[decoded_chromosome[i][j]]])

    for i in range(NUM_MACHINES_STAGE2):
        for j in range(len(decoded_chromosome[i+NUM_MACHINES_STAGE1])):
            exists_in_machine1 = np.where(decoded_chromosome[0] == (decoded_chromosome[i+NUM_MACHINES_STAGE1][j]-NUM_JOBS))[0]
            exists_in_machine2 = np.where(decoded_chromosome[1] == (decoded_chromosome[i+NUM_MACHINES_STAGE1][j]-NUM_JOBS))[0]
            if j==0:
                if len(exists_in_machine1) > 0:
                    # print("*********** the first machine ************")
                    start_time[i + NUM_MACHINES_STAGE1].extend([finish_time[0][exists_in_machine1[0]]])
                    finish_time[i + NUM_MACHINES_STAGE1].extend([start_time[i + NUM_MACHINES_STAGE1][-1] +
                                                                 processing_times_stage2[
                                                                     decoded_chromosome[i + NUM_MACHINES_STAGE1][
                                                                         j] - NUM_JOBS]])
                elif len(exists_in_machine2) > 0:
                    # print("*********** the second machine ************")
                    start_time[i + NUM_MACHINES_STAGE1].extend([finish_time[1][exists_in_machine2[0]]])
                    finish_time[i + NUM_MACHINES_STAGE1].extend([start_time[i + NUM_MACHINES_STAGE1][-1] +
                                                                 processing_times_stage2[
                                                                     decoded_chromosome[i + NUM_MACHINES_STAGE1][
                                                                         j] - NUM_JOBS]])
            else:
                start_time1=0
                start_time2=0
                if len(exists_in_machine1) > 0:
                    # print("*********** the first machine ************")
                    start_time1=finish_time[0][exists_in_machine1[0]]
                elif len(exists_in_machine2) > 0:
                    # print("*********** the second machine ************")
                    start_time2=finish_time[1][exists_in_machine2[0]]
                start_time[i + NUM_MACHINES_STAGE1].extend([max(max(start_time1, start_time2), finish_time[i + NUM_MACHINES_STAGE1][-1])])
                finish_time[i + NUM_MACHINES_STAGE1].extend([start_time[i + NUM_MACHINES_STAGE1][-1] +
                                                             processing_times_stage2[
                                                                 decoded_chromosome[i + NUM_MACHINES_STAGE1][
                                                                     j] - NUM_JOBS]])


    maxspan=[]


    for i in range(len(finish_time)):
        if len(finish_time[i]) == 0:
            maxspan.extend([0])
        else:
            maxspan.extend([max(finish_time[i])])

    makespan = max(maxspan)

    return decoded_chromosome, makespan, start_time, finish_time

# calculate the fitness of a chromosome
def fitness(population):
    makespan, _, _ = calculate_makespan(population)
    return 1.0 / makespan

# tournament selection
def tournament_selection(population):
    selected = []
    for _ in range(POPULATION_SIZE):
        tournament = random.sample(population, TOURNAMENT_SIZE)
        best = max(tournament, key=fitness)
        selected.append(best)
    return selected

# partially mapped crossover
def pmx_crossover(parent1, parent2):
    size = len(parent1)
    p1, p2 = parent1[:], parent2[:]

    # select two crossover points
    cxpoint1 = random.randint(0, size - 2)
    cxpoint2 = random.randint(cxpoint1 + 1, size - 1)

    # copy the crossover segment from parents to children
    temp = p1[cxpoint1:cxpoint2]
    p1[cxpoint1:cxpoint2] = p2[cxpoint1:cxpoint2]
    p2[cxpoint1:cxpoint2] = temp

    return list(p1), list(p2)

# swap mutation
def swap_mutation(chromosome):
# the mutation frenquency is 6
    k=2
    for i in range(k):
        idx = random.sample(range(len(chromosome)), 1)[0]
        if idx < NUM_JOBS:
            chromosome[idx] = np.random.uniform(STAGE[0], STAGE[1], 1)[0]
        else:
            chromosome[idx] = np.random.uniform(STAGE[1], STAGE[2], 1)[0]

# algorithm main body
def gatta(decode_chromosome, start_time, finish_time, make_span):
    Machines = decode_chromosome
    start = start_time
    finish = finish_time
    Makespan = make_span
    plt.rcParams['font.sans-serif'] = ['Times New Roman']
    plt.rcParams['axes.unicode_minus'] = False
    M = ['blue', 'salmon','skyblue','springgreen','palegoldenrod', 'pink', 'Thistle', 'RoyalBlue', 'Cyan',  'floralwhite', 'ghostwhite',  'mediumslateblue', 'navajowhite',  'sandybrown', 'red',
         'blue', 'salmon','skyblue','springgreen','palegoldenrod', 'pink', 'Thistle', 'RoyalBlue', 'Cyan',  'floralwhite', 'ghostwhite',  'mediumslateblue', 'navajowhite',  'sandybrown', 'red',
         'blue', 'salmon','skyblue','springgreen','palegoldenrod', 'pink', 'Thistle', 'RoyalBlue', 'Cyan',  'floralwhite', 'ghostwhite',  'mediumslateblue', 'navajowhite',  'sandybrown', 'red',
         'blue', 'salmon','skyblue','springgreen','palegoldenrod', 'pink', 'Thistle', 'RoyalBlue', 'Cyan',  'floralwhite', 'ghostwhite',  'mediumslateblue', 'navajowhite',  'sandybrown', 'red']

    Job_text = ['J' + str(i + 1) for i in range(100)]

    # if you want to add some special jobs, you can add them here
    Job_text.append("PM")
    Job_text.append("CM")

    Machine_text = ['M' + str(i + 1) for i in range(50)]

    for i in range(len(Machines)):
        if i<NUM_MACHINES_STAGE1:
            for j in range(len(start[i])):
                if finish[i][j] - start[i][j]!= 0:
                    plt.barh(i, width=finish[i][j] - start[i][j],
                             height=0.8, left=start[i][j],
                             color=M[Machines[i][j]],
                             edgecolor='black',label="Machine")
                    if(Machines[i][j]>=0):
                        plt.text(x=start[i][j]+(finish[i][j] - start[i][j])/2 - 0.1,
                                 y=i,
                                 s=Job_text[Machines[i][j]],
                                 fontsize=12)
                    elif Machines[i][j]==-1:
                        plt.text(x=start[i][j]+(finish[i][j] - start[i][j])/2 - 0.1,
                                 y=i,
                                 s=Job_text[100],
                                 fontsize=12)
                    else:
                        plt.text(x=start[i][j]+(finish[i][j] - start[i][j])/2 - 0.1,
                                 y=i,
                                 s=Job_text[101],
                                 fontsize=12)
        else:
            for j in range(len(start[i])):
                if finish[i][j] - start[i][j] != 0:
                    plt.barh(i, width=finish[i][j] - start[i][j],
                             height=0.8, left=start[i][j],
                             color=M[Machines[i][j]-NUM_JOBS],
                             edgecolor='black')
                    if (Machines[i][j] >= 0):
                        plt.text(x=start[i][j] + (finish[i][j] - start[i][j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[Machines[i][j]-NUM_JOBS],
                                 fontsize=12)
                    elif Machines[i][j] == -1:
                        plt.text(x=start[i][j] + (finish[i][j] - start[i][j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[100],
                                 fontsize=12)
                    else:
                        plt.text(x=start[i][j] + (finish[i][j] - start[i][j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[101],
                                 fontsize=12)
    plt.yticks([0, 1, 2, 3, 4], ['Machine 1 - Stage 1', 'Machine 2 - Stage 1', 'Machine 3 - Stage 2', 'Machine 4 - Stage 2', 'Machine 5 - Stage 2'], fontsize=16)
    plt.xlabel('Time of the production process', fontsize=19)
    plt.xticks(range(0, makespan+10, 2), [str(i) for i in range(0, makespan+10, 2)])
    plt.show()
def genetic_algorithm():
    population = generate_initial_population()
    best_chromosome = None
    best_makespan = float('inf')

    makespan=[]

    for generation in range(NUM_GENERATIONS):
        print("the *************** %d ********************* generation" % (generation + 1))
        # evaluate the fitness of each chromosome
        population_fitness = [fitness(chrom) for chrom in population]

        # update the best solution found so far
        for i in range(len(population)):
            current_makespan = 1/population_fitness[i]
            if current_makespan < best_makespan:
                best_makespan = current_makespan
                best_chromosome = population[i]

        makespan.extend([best_makespan])


        # output the best makespan in each generation
        print(f" the {generation+1} generation，best_solution is：{best_makespan}")

        # selection
        selected = tournament_selection(population)


        # uopdate the population
        next_generation = []
        while len(next_generation) < POPULATION_SIZE:
            if random.random() < CROSSOVER_RATE:
                parent1, parent2 = random.sample(selected, 2)
                offspring1, offspring2 = pmx_crossover(parent1, parent2)
            else:
                offspring1, offspring2 = random.sample(selected, 2)
            # mutation
            if random.random() < MUTATION_RATE:
                swap_mutation(offspring1)
            if random.random() < MUTATION_RATE:
                swap_mutation(offspring2)
            next_generation.extend([offspring1, offspring2])

        population = next_generation[:POPULATION_SIZE]
    # output the best solution found
    return best_chromosome, best_makespan




# 运行遗传算法
if __name__ == "__main__":
    best_schedule, best_makespan = genetic_algorithm()
    decode_individual, makespan, start_time, finish_time = calculate_makespan_gantt(best_schedule)
    gatta(decode_individual, start_time, finish_time,makespan)
    print("\nthe best solution：", best_schedule)
    print("the makespan：", best_makespan)


