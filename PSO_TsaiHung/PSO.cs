using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSO_TsaiHung
{
    class PSO
    {
        //Initial
        private int Population;//PopulationSize
        private int Dimension;//DimensionSize
        private double LowerBound;
        private double UpperBound;
        private double[][] Velocity;//SPEED
        private double[][] Position;//SITE
        //Run
        //private double w=0.7298, c1 = 1.49445, c2 = 1.49445;//宣告權重變數,
        //private double c1=1.496,c2=1.496;
        private double c1 = 1.49445, c2 = 1.49445;
        //private double W, Wini = 0.7298, Wfin =0.45;
        private double W = 0.7298;
        private int Iteration;
        //部分解會提早收斂 可能是無法到達該問題之最佳解  試過將權重值 w 轉為遞減函數 但好像沒差多少。所以固定為0.7298
        public double GBestFitness;
        private int MaxEvaluation;
        public int CurEvaluation;

        //Evaluation
        private double[] currentFitness;

        private double Vmax;//限制移動的速度  
        private double[] GbestPos;//暫存最佳的粒子值 (用於 update() )
        private double[][] PbestPos;//暫存每個粒子最佳的粒子
        private double[] PbestFitness;//暫存每個例子的最佳Fitness
        Random random = new Random(Guid.NewGuid().GetHashCode());//random

        public void Init(int populationSize,int VariableDimension,double VariableLowerbound,double VariavleUpperbound)
        {
            Vmax =  (VariavleUpperbound - VariableLowerbound) / 10;//上下限差距越大 Vmax越大 空間越大 ，每步移動量越大

            Population = populationSize;    //母體群大小
            Dimension = VariableDimension;  //維度
            LowerBound = VariableLowerbound;//下限
            UpperBound = VariavleUpperbound;//上限

            Position= new double[Population][];//位置母體群
            Velocity= new double[Population][];//速率母體群
            currentFitness = new double[Population];//現在的Fitness

            PbestFitness = new double[Population];//暫存每個例子的最佳Fitness
            PbestPos = new double[Population][];//暫存每個粒子最佳的粒子
            GbestPos = new double[Dimension];//暫存最佳的粒子值 (用於 update() )

            GBestFitness = double.MaxValue;//Global Best Fitness(Local Best Fitness *10  only oen Best is GBest)

            for(int i=0;i<Population;i++)
            {
                Position[i]=new double[Dimension];//初始化位置維度
                Velocity[i] = new double[Dimension];//初始化速率維度
  
          
                PbestPos[i] = new double[Dimension];//將該粒子曾經最好的暫存起來
                PbestFitness[i] = Double.MaxValue; ;//最佳PbestFitness  - 該粒子最好的Fitness (不是整個母體群)

                for (int j = 0; j < Dimension; j++)
                {//隨機初始化速率及位置
                    Position[i][j] =(random.NextDouble() * (UpperBound - LowerBound)) + LowerBound ;//初始位置
                    Velocity[i][j] = random.NextDouble() * (Vmax - (-Vmax)) + (-Vmax);//初始速率        
                }
            }
        }
        public void Run(int Max_Evaluation)
        { 
            MaxEvaluation = Max_Evaluation;
            Iteration = Max_Evaluation / Population;

            //while (true)
            for (int i = 0; i < Iteration;i++ )
            {
                evaluation();
                Update();
            }
            evaluation();
            //Console.WriteLine(GBestFitness);
        }
        //private void blanceW(int i)
        //{//W= Wini   -= Wfin  //計算遞減權重(當W固定為0.7298時 ，用此方法及固定權重的結果相近)
        //    W = ((Wini - Wfin) * (Iteration - i) / Iteration) + Wfin;      
        //}
        private void findPbest()
        {//尋找每個粒子中最好的適合度 並記錄該粒子的值 (用於 update() )
            for (int i = 0; i < Population; i++)
            {
                if (currentFitness[i] < PbestFitness[i])
                {//比原本的適合度好 則取代，並且取最好
                    PbestFitness[i] = currentFitness[i];
                    for (int j = 0; j < Dimension; j++)
                        PbestPos[i][j] = Position[i][j];
                }
            }

        }
        private void findGbest()
        {//尋找
            for (int i = 0; i < Population; i++)
                if (PbestFitness[i] < GBestFitness)
                {
                    GBestFitness = PbestFitness[i];//change Gbestfitness
                    for (int j = 0; j < Dimension;j++ )
                        GbestPos[j] = Position[i][j];//取代 (用於update() )
                }
        }
        private void evaluation()
        {//評估適合度 及尋找pBest 和 gBest
            for (int i = 0; i < Population; i++)
            {
                currentFitness[i] = Fitness(Position[i]);
            }
            findPbest();
            findGbest();
        }
        private void Update()
        {
            for(int i=0;i<Population;i++)
                for (int j = 0; j < Dimension; j++)
                {
                    Velocity[i][j] = W * Velocity[i][j] + c1 * random.NextDouble() * (PbestPos[i][j] - Position[i][j]) + c2 * random.NextDouble() * (GbestPos[j] - Position[i][j]);
                    /* Vid = W * Vid + c1 *Random *(pBest_id - Pos_id) + c2 * Random * (gBest_id - Pos_id)
                     * c1 = c2 ,and W > ( (c1+c2) /2  -1 )   才會收斂 否則發散
                     * W  亦可寫為遞減函數(算到越後面 移動的位置要越小 才能越容易找到答案 否則可能會在答案附近來回跑或其他原因，導致提早收斂，無法找到最佳解)
                     * */
                    if (Velocity[i][j] > Vmax)//如果速率超過上限 則等於速率
                        Velocity[i][j] = Vmax;
                    else if (Velocity[i][j] < -Vmax)//反之
                        Velocity[i][j] = -Vmax;
                            
                    Position[i][j] += Velocity[i][j]; //透過速率移動該粒子

                    if (Position[i][j] > UpperBound)//如果位置超過上限 則等於上限
                        Position[i][j] = UpperBound;
                    else if (Position[i][j] < LowerBound)//反之
                        Position[i][j] = LowerBound;                
                }
        }

        public virtual double Fitness(double[] f)
        {
            return -1;
        }

    }
}
