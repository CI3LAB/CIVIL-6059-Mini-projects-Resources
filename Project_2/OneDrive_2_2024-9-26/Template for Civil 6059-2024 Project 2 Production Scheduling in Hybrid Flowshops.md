## Template for data collection: Production Scheduling in Hybrid Flowshops 

### *Collaborative Intelligence with Construction Informatics in Construction Industrialization*



### Nomenclature

##### Parameters

$$
\mathcal{N}               \quad\quad\quad \text{Set of the jobs} \\
n                         \quad\quad\quad \text{Number of the jobs}\\
\mathcal{M}               \quad\quad \text{Set of the stages}\\
m                         \quad\quad\quad \text{Number of the stages}\\
M                         \quad\quad \text{A huge number}\\
m_i                       \quad\quad \text{Set of the machines located in stage i}\\
p_{i,j}                   \quad\quad \text{Processing time of job j in stage i}
$$

##### Decision variables

$$
C_{i,j}\qquad \text{Continuous variable which indicates the completion time of job j in stage i}\\
X_{i,j,k} \qquad \text{Binary variable equals 1 if job k is processed before job j at
stage i, and 0 otherwise}\\
Y_{i,l,j} \qquad \text{Binary variable equals 1 if job j is processed at stage i on
machine l, and 0 otherwise}\\
C_{max} \qquad \text{Continuous variable which indicates the makespan}
$$

### Problem description

There is a set $$\mathcal{N}$$ with $n$ jobs and a set $\mathcal{M}$ of $m$ stages, where each stage $i \in \mathcal{M}$ is composed of $m_i, \forall i \in \mathcal{M}$ identical parallel machines. Let $p_{i,j}$ be the processing time of job $j$ in stage $i$. Typically, the problem under discussion is based on the following basic assumptions:

1. ​    All jobs and all machines are available at time zero.      
2. ​    The processing time of all jobs on all machines is known in advance.
3. ​    The setup times of different jobs on the same machine and the transportation times between consecutive stages are negligible. 
4. ​    Each job can be processed on any parallel machine of a stage; moreover, no preemption is permitted for each job at each stage.     
5. ​    No machine can process more than one job and no job can be processed on more than one machine at the same time.
6. ​    No job can be processed unless it has been processed at the previous stage.

In addition, the aim of our problem is to minimise makespan.

### Formulation

From the problem description given above, we can formulate the production scheduling problem as follows:
$$
\begin{flalign}
    & Minimize \quad\quad C_{max} \\
    & \sum_{l \in \mathcal{M}_{i}} Y_{i,l,j}=1 \quad i \in \mathcal{M}, j \in \mathcal{N}\\
    & C_{i,j}\geq C_{i-1,j}+p_{i,j}\\
    & C_{i,j}\geq C_{i,k}+p_{i,j}-M\cdot(3-X_{i,j,k}-Y_{i,l,j}-Y_{i,l,k}) \nonumber \\ 
    &i\in\mathcal{M},l\in\mathcal{M}_i,j\in\mathcal{N},k\in\mathcal{N}|k>j \\
    & C_{i,k}\geq C_{i,j}+p_{i,k}-M\cdot X_{i,j,k}-M\cdot(2-Y_{i,l,j}-Y_{i,l,k}) \nonumber \\ 
    & i\in\mathcal{M},l\in\mathcal{M}_i,j\in\mathcal{N},k\in\mathcal{N}|k>j \\
    &C_{\max}\geq C_{m,j}&& \boldsymbol{j}\in\mathcal{N} \\
    &C_{i,j}\geq0&& \boldsymbol{i}\in\mathcal{M},\boldsymbol{j}\in\mathcal{N} \\
    &X_{i,j,k}\in\{0,1\}&& i\in\mathcal{M},j\in\mathcal{N},k\in\mathcal{N}|k>j \\
    &Y_{i,l,j}\in\{0,1\}&& i\in\mathcal{M},l\in\mathcal{M}_i,j\in\mathcal{N}& 
\end{flalign}
$$
Constraint set (2) enforces that each job is assigned to only one machine at each stage. The completion times for each job are determined by constraints (3), (4), and (5). Specifically, constraint set (2) guarantees that the  $i_{th}$ operation of a job begins only after the completion of its preceding operation. Conversely, constraints (4) and (5) ensure that no two jobs can be processed simultaneously on the same machine and that a job cannot begin before the completion of its previous operations on that machine. $M$ represents a sufficiently large number used in the formulation. Constraint set (6) defines the makespan of the schedule, while constraint sets (7), (8), and (9) specify the decision variables used in the model.

### Validation of the model

You can choose CUROBI or CPLEX to validate your model.

Step 1: convert your model into the solver language:

```python
from gurobipy import *
import gurobipy as gp
import numpy as np
import xlwt
def ps_twostage(M,N,L,P):
    W=100
    model = gp.Model()
    Cij = {( i, j): model.addVar(lb=0, vtype=GRB.CONTINUOUS, name=f"Cij[{i},{j}]") for i in M for j in N}
    Xijk={( i, j,k): model.addVar(lb=0, vtype=GRB.BINARY, name=f"Xijk[{i},{j},{k}]") for i in M for j in N for k in N}
    Yilj={( i, l,j): model.addVar(lb=0, vtype=GRB.BINARY, name=f"Yilk[{i},{l},{j}]") for i in M for l in L[i] for j in N }
    print(Yilj)
    Cmax=model.addVar(lb=0, vtype=GRB.CONTINUOUS, name=f"Cmax")
    model.addConstrs(sum(Yilj[i,l,j] for l in L[i])==1 for i in M for j in N )
    model.addConstrs(Cij[i,j]>=Cij[i-1,j]+P[i,j] for j in N for i in M if i!=0)
    model.addConstrs(Cij[i,j]>=Cij[i,k]+P[i,j]-W*(3-Xijk[i,j,k]-Yilj[i,l,j]-Yilj[i,l,k]) for i in M for l in L[i] for j in N for k in N if k>j)
    model.addConstrs(
        Cij[i, k] >= Cij[i, j] + P[i, k] - W *Xijk[i, j, k]-W* (2 -  Yilj[i, l, j] - Yilj[i, l, k]) for i in M for l in
        L[i] for j in N for k in N if k > j)
    model.addConstrs(Cij[i,j]>=P[i,j] for i in M for j in N)
    model.addConstrs(Cmax>=Cij[i,j] for i in M for j in N)
    model.setObjective(Cmax,GRB.MINIMIZE)
    model.Params.OutputFlag=1
    model.optimize()
    
# you can print the result, including the objective value and the value of the decision variables
    # for x1 in model.getVars():
    #     print(x1.VarName,x1.x)


# you can save the result to excel
    # wb = xlwt.Workbook('pstwostage.xls')
    # she = wb.add_sheet("pstwostage")
    # Pct2 = {}
    # Pct1 = {}
    # i = 0
    # for x in model.getVars():
    #     Pct2[x.VarName] = x.X
    # # result_excel.to_excel (excel_writer)
    # for key, value in Pct2.items():
    #     if value > 0:
    #         she.write(i, 0, key)
    #         she.write(i, 1, value)
    #         i += 1
    # she.write(i, 0, 'obj')
    # she.write(i, 1, model.objVal)
    # wb.save('pstwostage.xls')

    
    
 if __name__ == "__main__":
# Small-scale problem
    M=[0,1]
    N=[0,1,2,3,4]
    L=[[0,1],[0,1,2]]
    P=np.array([[2, 4, 5, 3, 6, 7, 1, 4, 7, 4, 5, 8, 3, 9, 9, 9, 9, 1, 23, 3],[5, 8, 3, 9, 9, 9, 9, 1, 23, 3, 2, 4, 5, 3, 6, 7, 1, 4, 7, 4]])
    ps_twostage(M, N, L, P)

# Medium-scale problem
# M = [0, 1, 2]
# N = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
# L = [[0, 1], [0, 1, 2], [0]]
# P = np.array([[2, 4, 5, 3, 6, 7, 1, 4, 7, 10, 2, 4, 5, 3, 6], [2, 4, 5, 3, 6, 7, 1, 4, 7, 10, 2, 4, 5, 3, 6],
#               [2, 4, 5, 3, 6, 7, 1, 4, 7, 10, 2, 4, 5, 3, 6]])
# ps_twostage(M, N, L, P)

# Large-scale problem
#     M=[0,1,2,3]
#     N=[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29]
#     L=[[0,1],[0,1,2],[0,1,2,3],[0,1,2,3,4]]
#     P=np.array([[2,4,5,3,6,7,1,4,7,10,2,4,5,3,6,7,1,4,7,10,2,4,5,3,6,7,1,4,7,10],[4,3,5,7,1,6,8,2,5,3,4,3,5,7,1,6,8,2,5,3,4,3,5,7,1,6,8,2,5,3],[2,4,5,3,6,7,1,4,7,10,2,4,5,3,6,7,1,4,7,10,2,4,5,3,6,7,1,4,7,10],[4,3,5,7,1,6,8,2,5,3,4,3,5,7,1,6,8,2,5,3,4,3,5,7,1,6,8,2,5,3]])
#     ps_twostage(M, N, L, P)
```

Step 2: attach your running results, including the **running time** and the solution **Gap**:

```python
C:\Users\user\Anaconda\envs\znm_torch\python.exe C:\Users\user\Desktop\HARL-15-new\ps_twostage.py 
Set parameter Username
Academic license - for non-commercial use only - expires 2024-12-02
{(0, 0, 0): <gurobi.Var *Awaiting Model Update*>, (0, 0, 1): <gurobi.Var *Awaiting Model Update*>, (0, 0, 2): <gurobi.Var *Awaiting Model Update*>, (0, 0, 3): <gurobi.Var *Awaiting Model Update*>, (0, 0, 4): <gurobi.Var *Awaiting Model Update*>, (0, 1, 0): <gurobi.Var *Awaiting Model Update*>, (0, 1, 1): <gurobi.Var *Awaiting Model Update*>, (0, 1, 2): <gurobi.Var *Awaiting Model Update*>, (0, 1, 3): <gurobi.Var *Awaiting Model Update*>, (0, 1, 4): <gurobi.Var *Awaiting Model Update*>, (1, 0, 0): <gurobi.Var *Awaiting Model Update*>, (1, 0, 1): <gurobi.Var *Awaiting Model Update*>, (1, 0, 2): <gurobi.Var *Awaiting Model Update*>, (1, 0, 3): <gurobi.Var *Awaiting Model Update*>, (1, 0, 4): <gurobi.Var *Awaiting Model Update*>, (1, 1, 0): <gurobi.Var *Awaiting Model Update*>, (1, 1, 1): <gurobi.Var *Awaiting Model Update*>, (1, 1, 2): <gurobi.Var *Awaiting Model Update*>, (1, 1, 3): <gurobi.Var *Awaiting Model Update*>, (1, 1, 4): <gurobi.Var *Awaiting Model Update*>, (1, 2, 0): <gurobi.Var *Awaiting Model Update*>, (1, 2, 1): <gurobi.Var *Awaiting Model Update*>, (1, 2, 2): <gurobi.Var *Awaiting Model Update*>, (1, 2, 3): <gurobi.Var *Awaiting Model Update*>, (1, 2, 4): <gurobi.Var *Awaiting Model Update*>}
Gurobi Optimizer version 10.0.3 build v10.0.3rc0 (win64)

CPU model: 13th Gen Intel(R) Core(TM) i9-13900KF, instruction set [SSE2|AVX|AVX2]
Thread count: 24 physical cores, 32 logical processors, using up to 32 threads

Optimize a model with 135 rows, 86 columns and 565 nonzeros
Model fingerprint: 0x10d2f3c0
Variable types: 11 continuous, 75 integer (75 binary)
Coefficient statistics:
  Matrix range     [1e+00, 1e+02]
  Objective range  [1e+00, 1e+00]
  Bounds range     [1e+00, 1e+00]
  RHS range        [1e+00, 3e+02]
Found heuristic solution: objective 21.0000000
Presolve removed 15 rows and 35 columns
Presolve time: 0.00s
Presolved: 120 rows, 51 columns, 545 nonzeros
Variable types: 11 continuous, 40 integer (40 binary)

Root relaxation: objective 1.500000e+01, 25 iterations, 0.00 seconds (0.00 work units)

    Nodes    |    Current Node    |     Objective Bounds      |     Work
 Expl Unexpl |  Obj  Depth IntInf | Incumbent    BestBd   Gap | It/Node Time

     0     0   15.00000    0    1   21.00000   15.00000  28.6%     -    0s
H    0     0                      20.0000000   15.00000  25.0%     -    0s
H    0     0                      18.0000000   15.00000  16.7%     -    0s
     0     0   15.00000    0    3   18.00000   15.00000  16.7%     -    0s
     0     0   15.00000    0    2   18.00000   15.00000  16.7%     -    0s
     0     0   15.00000    0    1   18.00000   15.00000  16.7%     -    0s
H    0     0                      17.0000000   15.00000  11.8%     -    0s
     0     0   15.00000    0    2   17.00000   15.00000  11.8%     -    0s
     0     0   15.00000    0    7   17.00000   15.00000  11.8%     -    0s
     0     0   15.00000    0    7   17.00000   15.00000  11.8%     -    0s
     0     2   15.00000    0    7   17.00000   15.00000  11.8%     -    0s
H  457    85                      16.9999993   15.00000  11.8%   2.8    0s

Cutting planes:
  Cover: 2
  Implied bound: 2
  MIR: 10
  StrongCG: 2
  Inf proof: 1
  RLT: 1

Explored 1107 nodes (2971 simplex iterations) in 0.08 seconds (0.04 work units)
Thread count was 32 (of 32 available processors)

Solution count 5: 17 17 18 ... 21

Optimal solution found (tolerance 1.00e-04)
Best objective 1.699999933393e+01, best bound 1.699999933393e+01, gap 0.0000%

Process finished with exit code 0
```

### Building of the algorithm pools

Choose at least one metaheuristics, and solve the proposed problem. The example is given by a Genetic Algorithm:

Step 1: give your algorithm parameters:

```python 
import random
import copy
import numpy as np
import matplotlib.pyplot as plt

# the number of jobs, machines in stage 1 and stage 2
NUM_JOBS = 40
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
POPULATION_SIZE = 200     # the number of chromosomes in the population
NUM_GENERATIONS = 200     # the number of generations
CROSSOVER_RATE = 0.8     # the probability of crossover
MUTATION_RATE = 0.2      # the probability of mutation
TOURNAMENT_SIZE = 5      # the size of tournament selection
```

 Then, give the encode and encode part:

```python
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
```

Then, give your algorithm body:

```python 
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
```

Finally, give your solutions and the Gantt chart:

```python
if __name__ == "__main__":
    best_schedule, best_makespan = genetic_algorithm()
    decode_individual, makespan, start_time, finish_time = calculate_makespan_gantt(best_schedule)
    gatta(decode_individual, start_time, finish_time,makespan)
    print("\nthe best solution：", best_schedule)
    print("the makespan：", best_makespan)
```

```python 
C:\Users\user\Anaconda\envs\znm_torch\python.exe "C:\Users\user\Desktop\HARL-15-new\Genetiic algorithm.py" 
the *************** 1 ********************* generation
 the 1 generation，best_solution is：89.0
the *************** 2 ********************* generation
 the 2 generation，best_solution is：89.0
the *************** 3 ********************* generation
 the 3 generation，best_solution is：89.0
the *************** 4 ********************* generation
 the 4 generation，best_solution is：87.0
the *************** 5 ********************* generation
 the 5 generation，best_solution is：87.0
the *************** 6 ********************* generation
 the 6 generation，best_solution is：87.0
the *************** 7 ********************* generation
 the 7 generation，best_solution is：87.0
the *************** 8 ********************* generation
 the 8 generation，best_solution is：87.0
the *************** 9 ********************* generation
 the 9 generation，best_solution is：87.0
the *************** 10 ********************* generation
 the 10 generation，best_solution is：87.0
the *************** 11 ********************* generation
 the 11 generation，best_solution is：86.0
the *************** 12 ********************* generation
 the 12 generation，best_solution is：86.0
the *************** 13 ********************* generation
 the 13 generation，best_solution is：86.0
the *************** 14 ********************* generation
 the 14 generation，best_solution is：86.0
the *************** 15 ********************* generation
 the 15 generation，best_solution is：86.0
the *************** 16 ********************* generation
 the 16 generation，best_solution is：86.0
the *************** 17 ********************* generation
 the 17 generation，best_solution is：86.0
the *************** 18 ********************* generation
 the 18 generation，best_solution is：86.0
the *************** 19 ********************* generation
 the 19 generation，best_solution is：86.0
the *************** 20 ********************* generation
 the 20 generation，best_solution is：86.0

```

Give the Gantt chart of the best solution:

![text 1](C:\Users\user\Desktop\text 1.png)