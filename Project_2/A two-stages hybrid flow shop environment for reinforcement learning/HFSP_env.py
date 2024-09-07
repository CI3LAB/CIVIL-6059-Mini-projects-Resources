import copy
import matplotlib.pyplot as plt
import numpy as np
import random
import math
import scipy.integrate
from Instance_Generator import Generate

class Machine:
    def __init__(self,idx):
        self.idx=idx     #机器编号
        self.start=[]    #机器开始加工的时间
        self.finish=[]   #机器停止加工的时间
        self._on=[]#
        self.end=0
        self.state=[0]   #机器的hazard rate
        self.custate=[0] #机器的累计误差率
        self.age=[0]     #机器的虚拟年龄
        self.PM_count=0
        self.CM_count=0
        self.DN_count=0

    def handling(self,Ji,pt,state_new,systime,vtime,custate):
        #s=self.insert_real(Ji,pt)
        # s=max(Ji.end,self.end)
        s=systime
        e=s+pt
        self.start.append(s)
        self.finish.append(e)
        self.start.sort()
        self.finish.sort()
        self._on.insert(self.start.index(s),Ji.idx)
        # self._on.append(Ji.idx)
        if self.end<e:
            self.end=e
        self.state.append(state_new)
        self.custate.append((custate+self.custate[-1]))
        self.age.append(vtime)
        Ji.update(s,e,self.idx)

    def handing_PM(self,PM,state_new,vtime,custate):
        s=PM.start
        # s=max(Ji.end,self.end)
        e=PM.end
        self.start.append(s)
        self.finish.append(e)
        self.start.sort()
        self.finish.sort()
        self._on.insert(self.start.index(s),-1)
        # self._on.append(Ji.idx)
        if self.end<e:
            self.end=e
        self.state.append(state_new)
        self.custate.append(custate+self.custate[-1])
        self.age.append(vtime)
        self.PM_count+=1

    def handing_CM(self,CM,state_new,vtime,custate):
        s=CM.start
        e=CM.end
        self.start.append(s)
        self.finish.append(e)
        self.start.sort()
        self.finish.sort()
        self._on.insert(self.start.index(s),-2)#在集齐队列中，小数代表PM、CM操作
        # self._on.append(Ji.idx)
        if self.end<e:
            self.end=e
        self.state.append(state_new)
        self.custate.append(custate)
        self.age.append(vtime)
        self.CM_count+=1

    def handing_DN(self,DN_R,state_new,vtime,custate):
        s=DN_R.start
        # s=max(Ji.end,self.end)
        e=DN_R.end
        self.start.append(s)
        self.finish.append(e)
        self.start.sort()
        self.finish.sort()
        self._on.insert(self.start.index(s),-3)#在集齐队列中，小数代表PM、CM操作
        # self._on.append(Ji.idx)
        if self.end<e:
            self.end=e
        self.state.append(state_new)
        self.custate.append(custate+self.custate[-1])
        self.age.append(vtime)
        self.DN_count+=1


    def Gap(self):
        Gap=0
        if self.start==[]:
            return 0
        else:
            Gap+=self.start[0]
            if len(self.start)>1:
                G=[self.start[i+1]-self.finish[i] for i in range(0,len(self.start)-1)]
                return Gap+sum(G)
            return Gap

    def judge_gap(self,t):
        Gap = []
        if self.start == []:
            return Gap
        else:
            if self.start[0]>0 and self.start[0]>t:
                Gap.append([0,self.start[0]])
            if len(self.start) > 1:
                Gap.extend([[self.finish[i], self.start[i + 1]] for i in range(0, len(self.start) - 1) if
                            self.start[i + 1] - self.finish[i] > 0 and self.start[i + 1] > t])
                return Gap
            return Gap

    def insert(self,Ji,pt):
        start=max(Ji.end[-1] if Ji.end!=[] else 0,self.end)
        Gap=self.judge_gap(Ji.end)
        if Gap!=[]:
            for Gi in Gap:
                if Gi[0]>=Ji.end and Gi[1]-Gi[0]>=pt:
                    return Gi[0]
                elif Gi[0]<Ji.end and Gi[1]-Ji.end>=pt:
                    return Ji.end
        return start

    def insert_real(self,Ji,pt):
        start=max(Ji.end[-1] if Ji.end!=[] else 0 ,self.end)
        return start

class Job:
    def __init__(self,idx,max_ol):
        self.idx=idx#工单编号
        self.start=[]#工单的开始时间
        self.end=[]#工单的结束时间
        self.op=0#工单的当前操作工序数
        self.max_ol=max_ol#最大工序数量
        self.Gap=0#工单的等待时间
        self.l=0#总加工时间
        self.T=[]#每阶段加工时间
        self.Job_M=[]

    def wether_end(self):
        if self.op<self.max_ol:
            return False
        else:
            return True

    def update(self,s,e,M):
        self.op+=1
        self.end.append(e)
        self.start.append(s)
        self.l=self.l+e-s
        self.T.append(e-s)
        self.Job_M.append(M)

class PM:
    def __init__(self,inx,st,end,M):
        self.inx=inx
        self.start=st#工单的开始时间
        self.end=end#工单的结束时间
        self.pm_machine=M

class CM:
    def __init__(self,inx,st,end,M):
        self.inx=inx
        self.start=st#工单的开始时间
        self.end=end#工单的结束时间
        self.cm_machine=M

class DN_R:
    def __init__(self,inx,st,end,M):
        self.inx=inx
        self.start=st#工单的开始时间
        self.end=end#工单的结束时间
        self.DN_machine=M

class HFS_Env:
    def __init__(self,n,m,p,PT,MT):
        """
        混合流水车间调度问题的解码
        :param n: 工件数量
        :param m: 机器数量
        :param p: 工序数量
        :param MT: 加工机器
        :param PT: 加工时间
        """
        #代表工单加工时间（系统设定部分）
        self.PT=PT
        self.M=MT
        self.n = n
        self.m = m
        self.p = p
        self.length = n * p

        #系统状态记录部分
        self.machine_state = np.zeros(self.m)  # 记录机器的状态，hazard rate，与维护动作有关（pm,cm）
        self.machine_age = np.zeros(self.m) #记录机器的虚拟时间，与真实加工时间有关
        self.machine_custate=np.zeros(self.m)#记录机器的累计误差率，用来进行CM

        self.job_avaliable1 = [] # 第一阶段当前可加工工单
        self.job_avaliable2 = []  # 第二阶段当前可加工工单
        self.job_finish = []  # 当前可加工工单
        self.PM_sq=[]#记录当前环境的PM队列
        self.CM_sq=[]#记录当前队列的CM队列

        #系统参数部分
        #与hazard rate有关，
        self.pmfactor_a = np.zeros(self.m)  # 机器pm修复因子(与hazard rate)相关
        self.pmfactor_b = np.zeros(self.m)  # 机器pm修复因子(与hazard rate)相关

        self.machine_threshold = np.zeros(self.m)  # 每一个machine挂机的阈值

        self.machine_c_shape1 = np.zeros(self.m)  # 威布尔函数参数
        self.machine_c_scale1 = np.zeros(self.m)

        self.machine_c_shape2 = np.zeros(self.m)  # 威布尔函数参数（用来建立混合退化模型）
        self.machine_c_scale2 = np.zeros(self.m)

        self.model_factor=0.2   #混合退化模型的混合因子，可以设计成每个机器单独的
        self.time_factor=0.1    #记录机器年龄对实际加工时间的影响


        self.prt_list_avr = np.zeros(self.m)  # 每个机器加工工单的平均时间
        self.cm_cost = np.zeros(self.m)       # 每个机器进行一次cm所耗资
        self.pm_cost = np.zeros(self.m)
        self.pmcost_record = np.zeros(self.m)  # 记录每个机器修复的花费
        self.cmcost_record = np.zeros(self.m)
        self.pmcost_time = np.zeros(self.m)  # 每个机器修复的所消耗的时间
        self.cmcost_time = np.zeros(self.m)
        self.pm_fix_time = np.zeros(self.m)
        self.pm_cost=np.zeros(self.m)
        self.totalcost_record = 0


        #标准状态对比
        self.machinelist_Z = []  # 记录不同机器上的退化影响
        self.machinelist_ZC = []
        self.machinelist_ZT = []  # 记录不同机器上的退化时间间隔
        self.prt_list_avr = [0.8, 0.705, 1.2, 1, 1]

    def machine_Z(self,):#计算每个机器的退化状态，不考虑PM,CM
        for i in range(self.m):
            self.machinelist_Z[i].append(0)            #记录hazard rate
            self.machinelist_ZT[i].append(0)           #记录机器age，以及时间节点
            self.machinelist_ZC[i].append(0)           #记录机器的累计磨损率，用来做CM 决策
        for i in range(self.m):
            P_t1=self.prt_list_avr[i]
            M_cm=0
            while(M_cm>=0):
                P_t=P_t1*(1+self.time_factor*max(self.machinelist_ZT[i]))

                #记录机器退化状态

                time=self.machinelist_ZT[i][-1]+P_t
                #记录下一时间的hazard rate
                Z_temp = ((1-self.model_factor)*(self.machine_c_shape1[i]/self.machine_c_scale1[i]*(time/self.machine_c_scale1[i])**(self.machine_c_shape1[i]-1)*math.exp(-((time/self.machine_c_scale1[i])**(self.machine_c_shape1[i]))))+\
                                self.model_factor*(self.machine_c_shape2[i]/self.machine_c_scale2[i]*(time/self.machine_c_scale2[i])**(self.machine_c_shape2[i]-1)*math.exp(-((time/self.machine_c_scale2[i])**(self.machine_c_shape2[i])))))/\
                                (self.model_factor*(math.exp(-(time/self.machine_c_scale2[i])**self.machine_c_shape2[i])-math.exp(-(time/self.machine_c_scale1[i])**self.machine_c_shape1[i]))+math.exp(-(time/self.machine_c_scale1[i])**self.machine_c_shape1[i]))
                #lamda 表达式用来计算累计及其退化状态
                f = lambda x: ((1-self.model_factor)*(self.machine_c_shape1[i]/self.machine_c_scale1[i]*((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale1[i])**(self.machine_c_shape1[i]-1)*math.exp(-(((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale1[i])**(self.machine_c_shape1[i]))))+\
                                self.model_factor*(self.machine_c_shape2[i]/self.machine_c_scale2[i]*((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale2[i])**(self.machine_c_shape2[i]-1)*math.exp(-(((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale2[i])**(self.machine_c_shape2[i])))))/\
                                (self.model_factor*(math.exp(-((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale2[i])**self.machine_c_shape2[i])-math.exp(-((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale1[i])**self.machine_c_shape1[i]))+math.exp(-((x+self.pmfactor_a[i]*self.cumacine_time[i])/self.machine_c_scale1[i])**self.machine_c_shape1[i]))

                custate=scipy.integrate.quad(f,self.machinelist_ZT[i][-1],time)[0]

                self.machinelist_Z[i].append(Z_temp)
                a=self.machinelist_ZT[i][-1]+P_t
                self.machinelist_ZT[i].append(a)
                b=self.machinelist_ZC[i][-1]+custate
                self.machinelist_ZC[i].append(b)
                M_cm =self.machine_threshold[i]-b



    def reset(self):
        #系统参数部分
        self.machine_pmax=np.zeros([self.n,self.m])
        #self.machine_threshold = [0.5, 0.5, 0.5, 0.5, 0.5]#先暂时沿用这个阈值，用来决定是否进行CM
        self.machine_threshold = [1.5, 1.5, 1.5, 1.5, 1.5]#先暂时沿用这个阈值，用来决定是否进行CM
        self.machinetime_threshold = [7,7,6,7,7]#用不到之前的量
        self.updata_PMax()                      #更新最大时间
        self.machine_state = np.zeros(self.m)   # 记录机器的状态
        self.PR=copy.deepcopy(self.PT)          #记录原始加工时间

        #三个时间量
        self.machine_time=np.zeros(self.m)      #记录pm时间间隔里的时间
        self.cumacine_time=np.zeros(self.m)     #记录上次PM的时间间隔
        self.machine_age = np.zeros(self.m)     #记录机器的虚拟时间
        self.machine_pmtime=np.zeros(self.m)    #记录上一个PM的时间点
        self.mahcine_custate=np.zeros(self.m)   #记录机器的累积误差率，用来CM，传递给强化学习的部分有三块，hazard rate、machine age、粗machine time


        self.pm_cost = [275, 230, 195, 315 , 240]  #机器维护的代价（涉及PM,CM,DN）
        self.cm_cost = [1028, 876, 832, 1164,888]
        self.dn_cost = [514, 438, 416, 582, 444]
        self.pm_fixcost = 100

        self.pmcost_time = [3.2,4.9,5.7,5.9,6]  # 每个机器修复的所消耗的时间
        self.cmcost_time = [13.5,12.2,12.28,13.49,12.2]
        self.pm_fix_time = [1.2, 1.1, 1.5, 1.5, 1.4]
        self.DN_time = [1.44, 1.27, 2.16,1.8, 1.8]  # 执行不选择工单时候的等待时间，可以作为系统合理安排的中间缓冲

        self.pmfactor_a = [0.05,0.04 ,0.04 ,0.025 ,0.035]#有关HAZARD rate的维护因子
        self.pmfactor_b = [1.02 ,1.03 ,1.02 ,1.01 ,1.03 ]
        self.pmage_factor=0.9
        # 每个机器连续退化部分服从威布尔分布的参数
        self.machine_c_scale1 = [6, 5 ,8 ,8.1 ,8 ]#正常退化状态下的威布尔参数
        self.machine_c_shape1 = [1.3, 1.2 ,1.4 ,1.25 ,1.26]

        self.machine_c_scale2 = [5 ,4 ,7 ,6.9, 7.1]#较坏条件退化状态下的威布尔参数
        self.machine_c_shape2 = [1.3, 1.2 ,1.4 ,1.25 ,1.26 ]

        self.totalcost_record = 0

        self.prt_list_avr = [0.8, 0.705, 1.2, 1, 1]

        #标准化机器退化状态，加工时间，以及加工的工单数量
        for i in range(self.m):
            self.machinelist_Z.append([])
            self.machinelist_ZT.append([])
            self.machinelist_ZC.append([])
        self.machine_Z()

        self.pm_count=np.zeros(self.m)#记录每个CMinterval中间的PM次数
        self.cm_count=np.zeros(self.m)#记录全局的CM次数
        # 系统状态记录部分 求解每个machine的加工工单的平均时间
        self.pmcost_record = np.zeros(self.m)  # 记录每个机器修复的花费
        self.cmcost_record = np.zeros(self.m)
        self.dncost_record= np.zeros(self.m)

        #初始化工单以及机器类别
        self.Jobs=[]
        for i in range(self.n):
            Ji=Job(i,len(self.PT[i]))
            self.Jobs.append(Ji)
        self.Machines=[]
        for i in range(self.m):
            Mi=Machine(i)
            self.Machines.append(Mi)

        self.job_avaliable1=copy.copy(self.Jobs)# 第一阶段当前可加工工单
        self.job_avaliable2 = []  # 第二阶段当前可加工工单
        self.job_finish=[]
        #self.mahcine_importance=0.277*np.ones[self.m]
        self.machine_actual_time=copy.copy(self.PT)#初始化每台机器的真是加工时间
        self.due_data=1.095# due-data=L*(1-R-Y/2) 其中R Y 都取为0.5
        self.PM_sq=[]
        self.CM_sq=[]
        self.PM_countreal=np.zeros(self.m)
        self.ind_job_stage1=self.n*[0]
        self.ind_job_stage2=self.n*[0]
        self.time=0
        self.state=[]
        self.obs=[]
        #初始化全局状态以及每个机器的局部观察
        for j in range(self.m):
            self.obs.append([])
        for i in range(self.m):
            for j in range(self.n):
                self.obs[i].extend([(self.PT[j][i]-self.PT[j][i])/(self.machine_pmax[j][i]-self.PT[j][i])])
        for i in range(self.m):
            if(i<2):#               队列工单数
                self.obs[i].extend(np.ones(self.n))
            else:
                self.obs[i].extend(np.zeros(self.n))



        EF1_best=1/(self.prt_list_avr[0]*(1+self.time_factor*self.machinetime_threshold[0]))+1/(self.prt_list_avr[1]*(1+self.time_factor*self.machinetime_threshold[1]))
        EF2_best=1 / (self.prt_list_avr[2]*(1+self.time_factor*self.machinetime_threshold[2])) + 1 / (self.prt_list_avr[3]*(1+self.time_factor*self.machinetime_threshold[3])) + 1 / (self.prt_list_avr[4]*(1+self.time_factor*self.machinetime_threshold[4]))

        #计算对应阶段的生产效率
        EF1=1/(self.prt_list_avr[0])+1/(self.prt_list_avr[1])
        EF2=1/(self.prt_list_avr[2])+1/(self.prt_list_avr[3])+1/(self.prt_list_avr[4])

        self.EF=[EF1,EF1,EF2,EF2,EF2]
        self.EF_best = [EF1_best, EF1_best, EF2_best, EF2_best, EF2_best]

        aa=(EF1 - EF1) / (EF1 - EF1_best)
        bb=(EF2-EF2)/(EF2-EF2_best)

        for i in range(self.m):
            if(i<2):#                队列工单数  队列工单数比值 机器退化状态    是否运行    PM次数  CM次数         阶段生产效率       机器的虚拟时间   机器的累计状态
                self.obs[i].extend([   1,        1,        0,          0,       0,    0,     (EF1-EF1_best)/(EF1-EF1_best)     ,0 ,        0       ])
            else:
                self.obs[i].extend([  0,       0,          0,          0,       0,    0,     (EF2-EF2_best)/(EF2-EF2_best)     ,0  ,       0 ])


        #初始化全局信息，维度为245
        for j in range(self.n):
            self.state.extend(self.PT[j])
        self.state.extend(np.ones(self.n))
        self.state.extend(np.zeros(self.n))
        #分别对应：            队列等待工单          工单比值       退化状态         PM次数       CM次数          是否运行            阶段效率     机器虚拟时间  机器的累计状态
        self.state.extend([  1,1,0,0,0,      1,1,0,0,0,   0,0,0,0,0,    0,0,0,0,0,    0,0,0,0,0,      0,0,0,0,0,     aa,aa,bb,bb,bb  , 0,0,0,0,0 ,  0,0,0,0,0,                ])

        self.action_num=self.n+4    #动作空间的数量

        self.machine_op=np.zeros(self.m)   #进行同时同步执行时，需要的中间数组，用来确定是否比较过,用来确定下一个决策的时间步

        done=False #控制门，用来说明是否已经分配完

        self.DN_count=np.zeros(self.m)  #用来记录DN_R的数量

        return self.state,done

    def get_state(self):
        return self.state

    def get_obs(self):
        return self.obs
    #异步执行的代码
    def is_process(self):
        end_time=[]
        index_machine=[]
        for i in range(self.m):
            if len(self.Machines[i]._on)>self.machine_op[i]:
                    end_time.append(self.Machines[i].finish[int(self.machine_op[i] if self.machine_op[i]<len(self.Machines[i].finish) else -1)])
            else:
                    end_time.append(9999999)

        index_min=np.argmin(end_time)
        self.time=end_time[index_min]
        end_time=np.array(end_time)
        # print("时间轴：")
        # print(self.time)
        # print(end_time)
        for i in range(len(end_time)):
            if(end_time[i]==self.time):
                self.machine_op[i]+=1
                index_machine.append(i)
        # print(index_machine)
        # print(len(index_machine))
        return index_machine
    def updata_PMax(self):
        for i in range(self.n):
            for j in range(self.m):
                self.machine_pmax[i][j]=self.PT[i][j]*(1+self.time_factor*self.machinetime_threshold[j])
    def updata_PR(self):
        for i in range(self.n):
            for j in range(self.m):
                self.PR[i][j]=self.PT[i][j]*(1+self.time_factor*self.Machines[j].age[int(self.machine_op[j] if self.machine_op[j]<len(self.Machines[j].age) else -1)])
    def updata_all(self,):
        index_machine=self.is_process()

        for index in range(len(index_machine)):
            index_min=index_machine[index]
            self.obs[index_min][2 *self.n+3]=0
        # print(index_min)
            if index_min<2:
                if len(self.Machines[index_min]._on)!=0 :
                    if self.Machines[index_min]._on[-1]>=0:
                        self.job_avaliable2.append(self.ind_job_stage1[self.Machines[index_min]._on[-1]])
                        self.ind_job_stage1[self.Machines[index_min]._on[-1]]=0

                        self.obs[2][self.n + self.Machines[index_min]._on[-1]] = 1
                        self.obs[2][2 * self.n] = len(self.job_avaliable2) / self.n
                        self.obs[2][2 * self.n + 1] = len(self.job_avaliable2) / self.n
                        self.obs[3][self.n + self.Machines[index_min]._on[-1]] = 1
                        self.obs[3][2 * self.n] = len(self.job_avaliable2) / self.n
                        self.obs[3][2 * self.n + 1] = len(self.job_avaliable2) / self.n
                        self.obs[4][self.n + self.Machines[index_min]._on[-1]] = 1
                        self.obs[4][2 * self.n] =len(self.job_avaliable2) / self.n
                        self.obs[4][2 * self.n + 1] = len(self.job_avaliable2) / self.n
            else:
                if len(self.Machines[index_min]._on)!=0:
                    if self.Machines[index_min]._on[-1]>=0:
                        self.job_finish.append(self.ind_job_stage2[self.Machines[index_min]._on[-1]])
                        self.ind_job_stage2[self.Machines[index_min]._on[-1]]=0
        self.updata_PR()
        for i in range(self.m):
            for j in range(self.n):
                self.obs[i][j]=(self.PR[j][i]-self.PT[j][i])/(self.machine_pmax[j][i]-self.PT[j][i])
        #更新状态组，更新机器退化状态
        for i in range(self.m):
            self.obs[i][2*self.n+2] = ((self.Machines[i].state[int(self.machine_op[i] if self.machine_op[i]<len(self.Machines[i].state) else -1)]))/self.machine_threshold[i]

        #更新状态组，更新机器PM\CM次数
        pm=0
        cm=0
        for i in range(self.m):
            for j in range(int(self.machine_op[i] if self.machine_op[i]<len(self.Machines[i]._on) else len(self.Machines[i]._on))):
                if self.Machines[i]._on[j]==-1:
                    pm+=1
                elif self.Machines[i]._on[j]==-2:
                    cm+=1
            self.obs[i][2*self.n+4] = pm
            self.obs[i][2 * self.n + 5] = cm

        #更新状态组，更新阶段生产效率
        EF11=((1/(self.prt_list_avr[0]*(1+self.time_factor*self.Machines[0].age[int(self.machine_op[0])]))+1/(self.prt_list_avr[1]*(1+self.time_factor*self.Machines[1].age[int(self.machine_op[1])])))-self.EF_best[0])/(self.EF[0]-self.EF_best[0])
        EF22 = ((1 / (self.prt_list_avr[2]*(1 + self.time_factor * self.Machines[2].age[int(self.machine_op[2])])) + \
               1 / (self.prt_list_avr[3]*(1 + self.time_factor * self.Machines[3].age[int(self.machine_op[3])])) + \
               1 / (self.prt_list_avr[4]*(1 + self.time_factor * self.Machines[4].age[int(self.machine_op[4])])))-self.EF_best[2])/(self.EF[2]-self.EF_best[2])
        for i in range (self.m):
            if i<2:
                self.obs[i][2 * self.n + 6]=EF11
            else:
                self.obs[i][2 * self.n + 6]=EF22

        for i in range(self.m):
            self.obs[i][2 * self.n + 7]=self.Machines[i].age[int(self.machine_op[i])]/self.machinetime_threshold[i]

        for i in range(self.m):
            self.obs[i][2 * self.n + 8]=self.Machines[i].custate[int(self.machine_op[i])]

        self.pull_state()
        # print("状态更新之后：")
        # print(self.job_avaliable1)
        # print(self.job_avaliable2)
        # print(self.ind_job_stage1)
        # print(self.ind_job_stage2)
        # print(self.Machines[1]._on)
        # print(self.Machines[1].start)
        # print(self.Machines[1].finish)
        # print("状态更新之后的退化状态及运行情况：")
        # print(self.state[7*self.n+15:7*self.n+20])
        # print(self.state[7*self.n+10:7*self.n+15])
        # print("状态更新之后的虚拟时间以及累计误差率")
        # print(self.state[7*self.n+35:7*self.n+40])
        # print(self.state[7*self.n+40:7*self.n+45])
        # # for i in range(self.m):
        # #     print(self.Machines[i]._on)
        # print()

    def pull_state(self):
        for s in range(self.n):
            for i in range(self.m):
                self.state[s*self.m+i]=self.obs[i][s]

        for i in range(self.n):
            self.state[5*self.n+i]=self.obs[0][self.n+i]

        for i in range(self.n):
            self.state[6*self.n+i]=self.obs[2][self.n+i]

        for i in range(9):
            for j in range(self.m):
                 self.state[7*self.n+i*self.m+j]=self.obs[j][2*self.n+i]

    def step(self, action,agnetID):  # 基于最早完工时刻规则的解码算法
        done=False
        r=0
        r1=0
        if len(self.job_finish)!=self.n :
            action_real=self.Dispatch_rule(action,agnetID)
            # print("真实的动作索引")
            # print(action)
            # print(action_real)
            if(agnetID<2) and action_real<=self.n:
                jobtemp = self.job_avaliable1[action_real].idx
                #做状态变化的存储
                prttemp = self.PT[jobtemp][agnetID]
                ptratemp = round(prttemp * (1 + self.time_factor*(self.Machines[agnetID].age[int(self.machine_op[agnetID])])),8)
                # 判断机器下一阶段会不会break
                self.machine_time[agnetID]+=ptratemp
                self.machine_age[agnetID]+=ptratemp
                #用来更新hazard rate

                time=self.machine_time[agnetID]+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID]
                state_curtemp = ((1-self.model_factor)*(self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*(time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)*math.exp(-((time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]))))+\
                                self.model_factor*(self.machine_c_shape2[agnetID]/self.machine_c_scale2[agnetID]*(time/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID]-1)*math.exp(-((time/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID])))))/\
                                (self.model_factor*(math.exp(-(time/self.machine_c_scale2[agnetID])**self.machine_c_shape2[agnetID])-math.exp(-(time/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))+math.exp(-(time/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))

                f = lambda x: ((1-self.model_factor)*(self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)*math.exp(-(((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]))))+\
                                self.model_factor*(self.machine_c_shape2[agnetID]/self.machine_c_scale2[agnetID]*((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID]-1)*math.exp(-(((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID])))))/\
                                (self.model_factor*(math.exp(-((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale2[agnetID])**self.machine_c_shape2[agnetID])-math.exp(-((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))+math.exp(-((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))

                custate=scipy.integrate.quad(f,self.machine_time[agnetID]-ptratemp,self.machine_time[agnetID])[0]


                self.machine_state[agnetID]=state_curtemp
                self.Machines[agnetID].handling(self.job_avaliable1[action_real],ptratemp,state_curtemp,self.time,self.machine_age[agnetID],custate)
                self.ind_job_stage1[self.job_avaliable1[action_real].idx]=(self.job_avaliable1[action_real])
                # r  = self.due_data-(self.time+ptratemp)
                r = -max(0,(self.time+ptratemp-self.due_data)/10)
                # r = 3 - (self.time + ptratemp)
                r1= (self.time + ptratemp)-3
                self.job_avaliable1.pop(action_real)

                #我们需要提前改变环境层面的变量，动作一执行，立马做出影响
                self.obs[0][self.n+jobtemp]=0
                self.obs[0][2*self.n] = len(self.job_avaliable1)/self.n
                self.obs[0][2 * self.n+1] = len(self.job_avaliable1)/self.n


                self.obs[1][self.n+jobtemp]=0
                self.obs[1][2*self.n] = len(self.job_avaliable1)/self.n
                self.obs[1][2 * self.n+1] = len(self.job_avaliable1)/self.n

                self.obs[agnetID][2 * self.n + 3] = 1

                self.pull_state()

            elif agnetID<5 and agnetID >=2 and action_real<=self.n:
                jobtemp = self.job_avaliable2[action_real].idx
                # 做状态变化的存储
                prttemp = self.PT[jobtemp][agnetID]
                ptratemp = round(prttemp *( 1 + self.time_factor * (self.Machines[agnetID].age[int(self.machine_op[agnetID])])),8)
                self.machine_time[agnetID]+=ptratemp
                self.machine_age[agnetID] +=ptratemp
                #用来更新hazard rate
                time=self.machine_time[agnetID]+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID]

                state_curtemp = ((1-self.model_factor)*(self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*(time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)*math.exp(-((time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]))))+\
                                self.model_factor*(self.machine_c_shape2[agnetID]/self.machine_c_scale2[agnetID]*(time/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID]-1)*math.exp(-((time/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID])))))/\
                                (self.model_factor*(math.exp(-(time/self.machine_c_scale2[agnetID])**self.machine_c_shape2[agnetID])-math.exp(-(time/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))+math.exp(-(time/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))

                f = lambda x: ((1-self.model_factor)*(self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)*math.exp(-(((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]))))+\
                                self.model_factor*(self.machine_c_shape2[agnetID]/self.machine_c_scale2[agnetID]*((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID]-1)*math.exp(-(((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID])))))/\
                                (self.model_factor*(math.exp(-((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale2[agnetID])**self.machine_c_shape2[agnetID])-math.exp(-((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))+math.exp(-((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))



                custate=scipy.integrate.quad(f,self.machine_time[agnetID]-ptratemp,self.machine_time[agnetID])[0]

                self.machine_state[agnetID]=state_curtemp
                self.Machines[agnetID].handling(self.job_avaliable2[action_real],ptratemp,state_curtemp,self.time,self.machine_age[agnetID],custate)

                self.ind_job_stage2[self.job_avaliable2[action_real].idx]=(self.job_avaliable2[action_real])
                r = -max(0,(self.time+ptratemp-self.due_data)/10)
                # r = 3 - (self.time + ptratemp)
                r1=(self.time + ptratemp)-30
                self.job_avaliable2.pop(action_real)

                # 我们需要提前改变环境层面的变量，动作一执行，立马做出影响
                self.obs[2][self.n + jobtemp] = 0
                self.obs[2][2 * self.n] = len(self.job_avaliable2) / self.n
                self.obs[2][2 * self.n + 1] = len(self.job_avaliable2) / self.n
                self.obs[3][self.n + jobtemp] = 0
                self.obs[3][2 * self.n] = len(self.job_avaliable2) / self.n
                self.obs[3][2 * self.n + 1] = len(self.job_avaliable2) / self.n
                self.obs[4][self.n + jobtemp] = 0
                self.obs[4][2 * self.n] = len(self.job_avaliable2) / self.n
                self.obs[4][2 * self.n + 1] = len(self.job_avaliable2) / self.n

                self.obs[agnetID][2 * self.n + 3] = 1

                self.pull_state()
            # 进行的是DN_R
            elif action_real == 555:
                self.obs[agnetID][2 * self.n + 3] = 1
                self.DN_count[agnetID]+=1
                self.pull_state()
                DN_end = self.time + self.DN_time[agnetID]
                DN_ind = DN_R(self.DN_count[agnetID], self.time, DN_end, agnetID)

                self.machine_time[agnetID]+=self.DN_time[agnetID]
                self.machine_age[agnetID] +=self.DN_time[agnetID]

                time=self.machine_time[agnetID]+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID]
                state_curtemp = self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*(time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)

                f = lambda x: self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*((x+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID])/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)
                custate=scipy.integrate.quad(f,self.machine_time[agnetID]-self.DN_time[agnetID],self.machine_time[agnetID])[0]

                self.machine_state[agnetID]=state_curtemp

                self.Machines[agnetID].handing_DN(DN_ind, self.machine_state[agnetID],self.machine_age[agnetID],custate)
                self.dncost_record[agnetID]+=self.dn_cost[agnetID]
                r = -self.dn_cost[agnetID]/40
                r1= self.dn_cost[agnetID]
            # 进行的是PM
            elif action_real==666:

                self.obs[agnetID][2 * self.n + 3] = 1
                self.pull_state()

                self.cumacine_time[agnetID]=self.time-self.machine_pmtime[agnetID]+self.pmcost_time[agnetID]+self.pm_fix_time[agnetID]
                self.machine_pmtime[agnetID]=self.time+self.pmcost_time[agnetID]+self.pm_fix_time[agnetID]
                self.machine_time[agnetID] = 0
                # self.machine_age[agnetID]+=self.pm_fix_time[agnetID] + self.pmcost_time[agnetID]
                self.machine_age[agnetID] +=0
                self.machine_age[agnetID]=(1-self.pmage_factor)*self.machine_age[agnetID]

                time=self.machine_time[agnetID]+self.pmfactor_a[agnetID]*self.cumacine_time[agnetID]
                # state_curtemp = ((1-self.model_factor)*(self.machine_c_shape1/self.machine_c_scale1*(time/self.machine_c_scale1)**(self.machine_c_shape1-1)*math.exp(-((time/self.machine_c_scale1)**(self.machine_c_shape1))))+\
                #                 self.model_factor*(self.machine_c_shape2/self.machine_c_scale2*(time/self.machine_c_scale2)**(self.machine_c_shape2-1)*math.exp(-((time/self.machine_c_scale2)**(self.machine_c_shape2)))))/\
                #                 (1-self.model_factor*math.exp(-(time/self.machine_c_scale2)**self.machine_c_shape2)-(1-self.model_factor)*math.exp(-(time/self.machine_c_scale1)**self.machine_c_shape1))
                #
                state_curtemp = ((1-self.model_factor)*(self.machine_c_shape1[agnetID]/self.machine_c_scale1[agnetID]*(time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]-1)*math.exp(-((time/self.machine_c_scale1[agnetID])**(self.machine_c_shape1[agnetID]))))+\
                                self.model_factor*(self.machine_c_shape2[agnetID]/self.machine_c_scale2[agnetID]*(time/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID]-1)*math.exp(-((time/self.machine_c_scale2[agnetID])**(self.machine_c_shape2[agnetID])))))/\
                                (self.model_factor*(math.exp(-(time/self.machine_c_scale2[agnetID])**self.machine_c_shape2[agnetID])-math.exp(-(time/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))+math.exp(-(time/self.machine_c_scale1[agnetID])**self.machine_c_shape1[agnetID]))

                custate =0
                self.machine_state[agnetID] = state_curtemp
                #self.machine_state_pmpre[agnetID] = self.machine_state[agnetID]  # 记录每次pm更新后machine的状态
                self.pm_count[agnetID] += 1  # 更新pmgroup_list中的machine进行的PM次数更新
                self.PM_countreal[agnetID]+=1

                self.pmcost_record[agnetID] += self.pm_cost[agnetID] + self.pm_fixcost # 机器进行pm修复所耗费
                # 更新机器下一段可加工时间
                #self.machine_avalible[agnetID] += self.pm_fix_time[agnetID] + self.pmcost_time[agnetID]
                PM_end=self.time+self.pm_fix_time[agnetID] + self.pmcost_time[agnetID]
                PM_ind=PM(self.PM_countreal[agnetID],self.time,PM_end,agnetID)
                self.PM_sq.append(PM_ind)
                self.Machines[agnetID].handing_PM(PM_ind, self.machine_state[agnetID],self.machine_age[agnetID],custate)
                r = -self.pm_cost[agnetID]/200
                r1= self.pm_cost[agnetID]

            #进行的是CM
            elif action_real==777:

                self.obs[agnetID][2 * self.n + 3] = 1
                self.pull_state()
                # self.cft[i] = self.cft[i] + self.cmcost_time[i]
                # job_num = len(self.machine_index[machine]) #该机器加工了多少个工单
                self.machine_state[agnetID] = 0
                self.machine_age[agnetID]=0
                self.machine_time[agnetID]=0
                self.cumacine_time[agnetID]=0
                self.cm_count[agnetID] += 1
                self.pm_count[agnetID]=0
                # self.PM_current[machine] -=1
                self.cmcost_record[agnetID] += self.cm_cost[agnetID]
                custate=0
                # 更新机器下一段可加工时间
                # self.machine_avalible[agnetID] += self.cmcost_time[agnetID]
                CM_end=self.time+self.cmcost_time[agnetID]

                CM_ind=CM(self.cm_count[agnetID],self.time,CM_end,agnetID)
                self.CM_sq.append(CM_ind)
                self.Machines[agnetID].handing_CM(CM_ind, self.machine_state[agnetID],self.machine_age[agnetID],custate)
                r = -self.cm_cost[agnetID]/100
                r1=self.cm_cost[agnetID]

            #进行非强制性的DN
            else:
                r=0
                r1=0

        else:
            done=True
        if agnetID==4 and done!=True:
            self.updata_all()
        return r, done,r1

    def C_max(self):
        m=0
        for Mi in self.Machines:
            if Mi.end>m:
                m=Mi.end
        return m

    def Dispatch_rule(self,a, agnetID):
        if a < self.n:
            return self.job_select(a,agnetID)
        elif a == self.n:
            return self.PM(agnetID)
        elif a == (self.n+1):
            return self.DN_R(agnetID)
        elif a == (self.n+2):
            return self.CM(agnetID)
        elif a == self.n+3:
            return self.DN(agnetID)

    # select the job with the shortest processing time
    def job_select(self,a,agnetID):
        if (agnetID < 2):
            for i in range(len(self.job_avaliable1)):
                if self.job_avaliable1[i].idx==a:
                    return i
        else:
            for i in range(len(self.job_avaliable2)):
                if self.job_avaliable2[i].idx==a:
                    return i

    def PM(self, agnetID):
        return 666

    def CM(self, agnetID):
        return 777

    def DN(self, agnetID):
        return 888

    def DN_R(self, agnetID):
        return 555

    def close(self):
        print("环境关闭！！！")
        return 0
    def get_avail_agent_actions(self,agentID):
        action_index=np.zeros(self.action_num)
        #如果其中的累计退化状态超过1，进行CM
        dv = self.Machines[agentID].custate[-1]
        index_custate=0
        index1=0
        index2=0
        index_job=0
        index_pm=0
        index_dn=0
        # 判断机器下一阶段会不会break
        if dv<self.machine_threshold[agentID]:
            if agentID < 2:
                if self.obs[agentID][2 * self.n + 3] == 0 and len(self.job_avaliable1) >= 1:
                    for i in range(self.n):
                        action_index[i] = self.obs[agentID][self.n+i]

                    for i in range (len(self.Machines[agentID]._on)):
                        if self.Machines[agentID]._on[len(self.Machines[agentID]._on)-i-1]!=-2:
                            if self.Machines[agentID]._on[len(self.Machines[agentID]._on)-i-1]!=-1:
                                index_job+=1
                            else:
                                break
                        else:
                            break
                    #有关pm的位置
                    if index_job>=1:
                        action_index[self.n]=1
                else:
                    action_index[self.n+3] = 1

                action_index[self.n+2] = 0
                return action_index
            else:
                if self.obs[agentID][2 * self.n + 3] == 0 and len(self.job_avaliable2) >= 1:
                    for i in range(self.n):
                        action_index[i] = self.obs[agentID][self.n+i]

                    for i in range(len(self.Machines[agentID]._on)):
                        if self.Machines[agentID]._on[len(self.Machines[agentID]._on) - i - 1] != -2:
                            if self.Machines[agentID]._on[len(self.Machines[agentID]._on) - i - 1] != -1:
                                index_job += 1
                            else:
                                break
                        else:
                            break
                    # 有关pm的位置
                    if index_job >= 1:
                        action_index[self.n] = 1
                else:
                    action_index[self.n+3] = 1

                action_index[self.n+2] = 0

                return action_index
        else:
            if agentID < 2:
                if self.obs[agentID][2 * self.n + 3] == 0 and len(self.job_avaliable1) >= 1:
                    action_index[self.n+2] = 1
                    action_index[self.n+1] = 1
                else:
                    action_index[self.n+3] = 1
                return action_index
            else:
                if self.obs[agentID][2 * self.n + 3] == 0 and len(self.job_avaliable2) >= 1:
                    action_index[self.n+2] = 1
                    action_index[self.n+1] = 1
                else:
                    action_index[self.n+3] = 1
                return action_index

    def Gantt(self,):
        Machines=self.Machines
        plt.rcParams['font.sans-serif'] = ['Times New Roman']  # 如果要显示中文字体,则在此处设为：SimHei
        plt.rcParams['axes.unicode_minus'] = False  # 显示负号
        M = ['red', 'blue', 'yellow', 'orange', 'green', 'palegoldenrod', 'purple', 'pink', 'Thistle', 'Magenta',
             'SlateBlue', 'RoyalBlue', 'Cyan', 'Aqua', 'floralwhite', 'ghostwhite', 'goldenrod', 'mediumslateblue',
             'navajowhite', 'navy', 'sandybrown', 'moccasin',
             'red', 'blue', 'yellow', 'orange', 'green', 'palegoldenrod', 'purple', 'pink', 'Thistle', 'Magenta',
             'SlateBlue', 'RoyalBlue', 'Cyan', 'Aqua', 'floralwhite', 'ghostwhite', 'goldenrod', 'mediumslateblue',
             'navajowhite', 'navy', 'sandybrown', 'moccasin',
             ]
        Job_text = ['J' + str(i + 1) for i in range(100)]
        Job_text.append("PM")
        Job_text.append("CM")
        Job_text.append("DN")
        Machine_text = ['M' + str(i + 1) for i in range(50)]
        print("开始画图")
        print(len(Machines))
        for i in range(len(Machines)):
            if i==1:
                print("对应机器号：")
                print(len(Machines[i].start))
                print(Machines[i]._on)
                print(Machines[i].start)
                print(Machines[i].finish)


        for i in range(len(Machines)):
            for j in range(len(Machines[i].start)):
                if Machines[i].finish[j] - Machines[i].start[j] != 0:
                    plt.barh(i, width=Machines[i].finish[j] - Machines[i].start[j],
                             height=0.8, left=Machines[i].start[j],
                             color=M[Machines[i]._on[j]],
                             edgecolor='black')
                    if (Machines[i]._on[j] >= 0):
                        plt.text(x=Machines[i].start[j] + (Machines[i].finish[j] - Machines[i].start[j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[Machines[i]._on[j]],
                                 fontsize=12)
                    elif Machines[i]._on[j] == -1:
                        plt.text(x=Machines[i].start[j] + (Machines[i].finish[j] - Machines[i].start[j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[100],
                                 fontsize=12)
                    elif Machines[i]._on[j] == -2:
                        plt.text(x=Machines[i].start[j] + (Machines[i].finish[j] - Machines[i].start[j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[101],
                                 fontsize=12)
                    else:
                        plt.text(x=Machines[i].start[j] + (Machines[i].finish[j] - Machines[i].start[j]) / 2 - 0.1,
                                 y=i,
                                 s=Job_text[102],
                                 fontsize=12)
        plt.show()

def Gantt(Machines):
    plt.rcParams['font.sans-serif'] = ['Times New Roman']  # 如果要显示中文字体,则在此处设为：SimHei
    plt.rcParams['axes.unicode_minus'] = False  # 显示负号
    M = ['red', 'blue', 'yellow', 'orange', 'green', 'palegoldenrod', 'purple', 'pink', 'Thistle', 'Magenta',
         'SlateBlue', 'RoyalBlue', 'Cyan', 'Aqua', 'floralwhite', 'ghostwhite', 'goldenrod', 'mediumslateblue',
         'navajowhite', 'navy', 'sandybrown', 'moccasin']
    Job_text = ['J' + str(i + 1) for i in range(100)]
    Job_text.append("PM")
    Job_text.append("CM")
    Machine_text = ['M' + str(i + 1) for i in range(50)]
    print("开始画图")
    print(len(Machines))
    for i in range(len(Machines)):
        print(len(Machines[i].start))

    for i in range(len(Machines)):
        for j in range(len(Machines[i].start)):
            if Machines[i].finish[j] - Machines[i].start[j]!= 0:
                plt.barh(i, width=Machines[i].finish[j] - Machines[i].start[j],
                         height=0.8, left=Machines[i].start[j],
                         color=M[Machines[i]._on[j]],
                         edgecolor='black')
                if(Machines[i]._on[j]>=0):
                    plt.text(x=Machines[i].start[j]+(Machines[i].finish[j] - Machines[i].start[j])/2 - 0.1,
                             y=i,
                             s=Job_text[Machines[i]._on[j] ],
                             fontsize=12)
                elif Machines[i]._on[j]==-1:
                    plt.text(x=Machines[i].start[j]+(Machines[i].finish[j] - Machines[i].start[j])/2 - 0.1,
                             y=i,
                             s=Job_text[100],
                             fontsize=12)
                else:
                    plt.text(x=Machines[i].start[j]+(Machines[i].finish[j] - Machines[i].start[j])/2 - 0.1,
                             y=i,
                             s=Job_text[101],
                             fontsize=12)
    plt.show()









