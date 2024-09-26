

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







