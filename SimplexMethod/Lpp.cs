using System;
using System.Collections.Generic;

namespace SimplexMethod
{
    public class Lpp
    {
        private List<double> targetFunc;
        private string target;
        private List<List<double>> limitations;
        private List<string> signs;
        private List<double> limits;

        private List<double> startBasis;        // Начальное допустимое базисное решение
        private int countVariable;              // Изначальное количество переменных
        private int countArtificialVariable;    // Количество добавленных искусственных переменных

        #region Create task

        public Lpp()
        {
            targetFunc = new List<double>();
            limitations = new List<List<double>>();
            signs = new List<string>();
            limits = new List<double>();
        }

        public Lpp(List<double> targetFunc, string target, List<List<double>> limitations, List<string> signs,
            List<double> limits)
        {
            this.targetFunc = targetFunc;
            this.target = target;
            this.limitations = limitations;
            this.signs = signs;
            this.limits = limits;
        }

        public void SetTargetFunction(string targetFuncStr)
        {
            try
            {
                if (string.IsNullOrEmpty(targetFuncStr))
                {
                    throw new Exception("целевая функция пустая");
                }

                string[] targetFuncStrAr = targetFuncStr.Split();
                foreach (var t in targetFuncStrAr)
                {
                    targetFunc.Add(int.Parse(t));
                }

                countVariable = targetFunc.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при определении целевой функции: " + ex.Message);
            }
        }

        public void SetTarget(string targetStr)
        {
            try
            {
                if (targetStr == "max" || targetStr == "min")
                {
                    target = targetStr;
                }
                else throw new Exception("цель должна быть max либо min");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при определении цели: " + ex.Message);
            }
        }

        public void AddLimitation(string limitationStr)
        {
            if (string.IsNullOrEmpty(limitationStr))
            {
                throw new Exception("ограничение пустое");
            }

            string[] limitationStrAr = limitationStr.Split();
            try
            {
                limitations.Add(new List<double>());
                for (int i = 0; i < limitationStrAr.Length; i++)
                {
                    if (i < limitationStrAr.Length - 2)
                    {
                        limitations[limitations.Count - 1].Add(int.Parse(limitationStrAr[i]));
                    }

                    if (i == limitationStrAr.Length - 2)
                    {
                        signs.Add(limitationStrAr[i]);
                    }

                    if (i == limitationStrAr.Length - 1)
                    {
                        limits.Add(int.Parse(limitationStrAr[i]));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при определении ограничения: " + ex.Message);
            }
        }

        private void PrintEquation(List<double> coefs)
        {
            for (int i = 0; i < coefs.Count; i++)
            {
                if (i == 0)
                {
                    Console.Write(coefs[i]);
                }
                else
                {
                    Console.Write(coefs[i] < 0 ? " - " + -coefs[i] : " + " + coefs[i]);
                }

                Console.Write("x" + (i + 1));
            }
        }

        private void PrintLimitations()
        {
            for (int i = 0; i < limitations.Count; i++)
            {
                Console.Write(i + 1 + ") ");
                PrintEquation(limitations[i]);
                Console.Write(" " + signs[i] + " " + limits[i]);
                Console.WriteLine();
            }
        }

        public void PrintLPP()
        {
            Console.Write("f =  ");
            PrintEquation(targetFunc);

            Console.WriteLine(" --> " + target);

            Console.WriteLine("Ограничения:");
            PrintLimitations();
        }

        #endregion

        // Приведение ЗЛП к стандартному виду
        private void ToStandartForm()
        {
            // Первое требование - минимизация
            if (target == "max")
            {
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    targetFunc[i] = -targetFunc[i];
                }
            }

            // Второе требование - неотрицательность переменных


            // Третье требование - равенства
            for (int i = 0; i < limitations.Count - countVariable; i++)
            {
                // Определяем, прибавлять, вычитать или не добавлять новую переменную
                int variable = signs[i] == "<=" ? 1 : (signs[i] == ">=" ? -1 : 0);
                // Если ограничение - уже равенство, то переходим к следующему
                if (variable == 0) continue;
                // Иначе добавляем к нему искусственную переменную, а также ко всем остальным ограниченим и целевой функции с коэффициентом 0
                limitations[i].Add(variable);
                countArtificialVariable++;
                for (int j = 0; j < limitations.Count - countVariable; j++)
                {
                    if (i == j) continue;
                    limitations[j].Add(0);
                    targetFunc.Add(0);
                }
            }

            // Четвертое требование - неотрицательность правых частей
            for (int i = 0; i < limitations.Count; i++)
            {
                if (limits[i] < 0)
                {
                    limits[i] = -limits[i];
                    for (int j = 0; j < limitations[i].Count; j++)
                    {
                        limitations[i][j] = -limitations[i][j];
                    }
                }
            }
        }

        // Проверка на то, находится ли ЗЛП в каноническом виде
        private bool IsCanonicalForm()
        {
            // Проходим по каждому ограничению, кроме ограничений на неотрицательность
            for (int i = 0; i < limitations.Count - countVariable; i++)
            {
                // Берем переменную с конца
                for (int j = limitations[i].Count - 1; j >= 0; j--)
                {
                    // Если при ней коэффициент не равен 1, то пропускаем
                    if (limitations[i][j] != 1) continue;
                    bool isExist = false;
                    // Иначе проверяем её наличие в других ограничениях
                    for (int k = 0; k < limitations.Count && !isExist; k++)
                    {
                        if (k != i) continue;
                        // Если она нашлась, то переходим к другой переменной
                        if (limitations[k].Count >= j)
                        {
                            isExist = true;
                        }
                    }

                    // Если она не нашлась ни в одном ограничении, то добавляем в начальный базис
                    if (!isExist)
                    {
                        startBasis[j] = limitations[i][j];
                    }
                }
            }

            // Если количество базисный переменных равняется количеству ограничений, то ЗЛП находится в каноническом виде, иначе - нет
            int countBasis = 0;
            for (int i = 0; i < startBasis.Count; i++)
            {
                if (startBasis[i] != 0)
                {
                    countBasis++;
                }
            }

            return countBasis == limitations.Count - countVariable;
        }

        // Решение ЗЛП симплекс-методом
        public void Solve()
        {
            countArtificialVariable = 0;
            ToStandartForm();

            startBasis = new List<double>();
            for (int i = 0; i < targetFunc.Count; i++)
            {
                startBasis.Add(0);
            }

            if (IsCanonicalForm())
            {
                PrintSimplexTable();
                // Решение одноэтапным методом
            }
            else
            {
                Console.WriteLine(
                    "ЗЛП не находится в каноническом виде, поэтому она решается только двухэтапным симплекс-методом");
            }
        }

        private void PrintSimplexTable()
        {
            // i - строка
            for (int i = -1; i < countArtificialVariable + 1; i++)
            {
                if (i == -1)
                {
                    for (int j = 0; j < limitations[i + 1].Count + 1; j++)
                    {
                        if (j == limitations[i + 1].Count)
                        {
                            Console.Write("\tF(x)");
                            continue;
                        }

                        Console.Write("\tx" + (j + 1));
                    }

                    continue;
                }

                // j - столбец
                for (int j = -1; j < limitations[i].Count + 1; j++)
                {
                    if (j == -1)
                    {
                        int k = 0;
                        while (startBasis[k] == 0)
                        {
                            k++;
                        }

                        Console.Write("x" + startBasis[k]);
                        continue;
                    }

                    Console.Write("\t" + limitations[i][j]);
                    if (j == limitations[i].Count)
                    {
                        Console.Write("\t" + CalculateTargetFunc(limitations[i]));
                    }
                }
            }
        }

        private double CalculateTargetFunc(List<double> x)
        {
            double result = 0.0;
            for (int i = 0; i < targetFunc.Count; i++)
            {
                result += targetFunc[i] * x[i];
            }

            return result;
        }
    }
}