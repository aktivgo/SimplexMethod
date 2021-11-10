using System;
using System.Collections.Generic;
using System.Linq;

namespace SimplexMethod
{
    public class Lpp
    {
        private List<double> targetFunc;
        private string target;
        private List<List<double>> limitations;
        private List<string> signs;
        private List<double> limits;

        private List<double> startBasis; // Начальное допустимое базисное решение
        private List<double> resultBasis;
        private int countVariable; // Изначальное количество переменных
        private int countArtificialVariable; // Количество добавленных искусственных переменных
        private bool isInvertTarget = false;

        #region Create task

        public Lpp()
        {
            targetFunc = new List<double>();
            limitations = new List<List<double>>();
            signs = new List<string>();
            limits = new List<double>();
            resultBasis = new List<double>();
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
                        limitations[limitations.Count - 1].Add(double.Parse(limitationStrAr[i]));
                    }

                    if (i == limitationStrAr.Length - 2)
                    {
                        signs.Add(limitationStrAr[i]);
                    }

                    if (i == limitationStrAr.Length - 1)
                    {
                        limits.Add(double.Parse(limitationStrAr[i]));
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

        public void PrintLpp()
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
            bool isChanged = false;
            // Первое требование - минимизация
            if (target == "max")
            {
                target = "min";
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    targetFunc[i] = -targetFunc[i];
                }

                isInvertTarget = true;
                isChanged = true;
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
                signs[i] = "=";
                countArtificialVariable++;
                for (int j = 0; j < limitations.Count - countVariable; j++)
                {
                    if (i == j) continue;
                    limitations[j].Add(0);
                }

                targetFunc.Add(0);
                isChanged = true;
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

                    isChanged = true;
                }
            }

            if (isChanged)
            {
                Console.WriteLine("ЗЛП приведена в стандартный вид:");
                PrintLpp();
                Console.WriteLine();
                return;
            }

            Console.Write("ЗЛП уже находится в стандартном виде\n");
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
                    for (int k = 0; k < limitations.Count - countVariable && !isExist; k++)
                    {
                        if (k == i) continue;
                        // Если она нашлась, то переходим к другой переменной
                        if (limitations[k][j] != 0)
                        {
                            isExist = true;
                        }
                    }

                    // Если она не нашлась ни в одном ограничении, то добавляем в начальный базис
                    if (!isExist)
                    {
                        startBasis[j] = limits[i];
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

            if (countBasis == limitations.Count - countVariable)
            {
                Console.WriteLine("ЗЛП в каноническом виде, следовательно решается одноэтапным симплекс-методом");
                Console.Write("НДБР = (");
                for (int i = 0; i < startBasis.Count; i++)
                {
                    Console.Write(" " + startBasis[i] + (i != startBasis.Count - 1 ? "," : " )\n"));
                }

                Console.WriteLine();
                return true;
            }

            return false;
        }

        // Решение ЗЛП симплекс-методом
        public void Solve()
        {
            countArtificialVariable = 0;
            ToStandartForm();

            // Решение одноэтапным методом
            startBasis = new List<double>();
            for (int i = 0; i < targetFunc.Count; i++)
            {
                startBasis.Add(0);
            }

            resultBasis = new List<double>();
            for (int i = 0; i < targetFunc.Count; i++)
            {
                resultBasis.Add(0);
            }

            if (IsCanonicalForm())
            {
                OneStepSimplexMethod();
            }
            else
            {
                Console.WriteLine(
                    "ЗЛП не находится в каноническом виде, поэтому она решается двухэтапным симплекс-методом\n");

                TwoStepSimplexMethod();
            }
        }

        private void OneStepSimplexMethod()
        {
            double coef;
            List<List<double>> simplexTable = CreateSimplexTable();
            PrintStartSimplexTable(simplexTable);

            // Пока в целевой функции есть отрицательные элементы решение не оптимально
            while (SelectColumn(simplexTable[simplexTable.Count - 1]) != -1)
            {
                // Выбираем ведущий столбец
                int selectColumn = SelectColumn(simplexTable[simplexTable.Count - 1]);
                // Ведущая строка
                int selectLine = -1;
                // Максимальный по модулю отрицательный коэффициент
                double minCoef = int.MaxValue;
                for (int i = 0; i < simplexTable.Count - 1; i++)
                {
                    // Если правая часть равна 0, то пропускаем
                    if (simplexTable[i][simplexTable[i].Count - 1] == 0) continue;
                    // Если коэффициент в ведущем столбце равен 0, тоже пропускаем
                    if (simplexTable[i][selectColumn] == 0) continue;
                    // Считаем отношение правой части к коэффициенту ведущего столбца
                    coef = simplexTable[i][simplexTable[i].Count - 1] / simplexTable[i][selectColumn];
                    // Если он положительный и меньше минимального, то сохраняем его вместе с позицией строки
                    if (coef > 0 && minCoef > coef)
                    {
                        minCoef = coef;
                        selectLine = i;
                    }
                }

                if (selectLine == -1)
                {
                    Console.WriteLine("Fmin не ограничена\n");
                    return;
                }

                Console.WriteLine("Столбец " + selectColumn + ", строка " + selectLine + "\n");

                // Делим ведущую строку на коэффициент, расположенный на пересечении выбранных столбца и строки
                coef = simplexTable[selectLine][selectColumn];
                for (int i = 0; i < simplexTable[selectLine].Count; i++)
                {
                    simplexTable[selectLine][i] /= coef;
                }

                // Вычитаем из других строк получившуюся строку с определнным коэффициентом
                for (int i = 0; i < simplexTable.Count; i++)
                {
                    if (i == selectLine) continue;
                    // Считаем коэффициент
                    coef = simplexTable[i][selectColumn];
                    for (int j = 0; j < simplexTable[i].Count; j++)
                    {
                        simplexTable[i][j] -= coef * simplexTable[selectLine][j];
                    }
                }

                List<int> indexesOfBasisVariable = new List<int>();
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    indexesOfBasisVariable.Add(int.MaxValue);
                }

                for (int i = 0; i < targetFunc.Count; i++)
                {
                    bool isBasisVarible = true;
                    int countOne = 0;
                    int indexOfOne = -1;
                    for (int j = 0; j < simplexTable.Count && isBasisVarible; j++)
                    {
                        if (simplexTable[j][i] != 0 && simplexTable[j][i] != 1)
                        {
                            isBasisVarible = false;
                            continue;
                        }

                        if (simplexTable[j][i] == 1)
                        {
                            countOne++;
                            indexOfOne = j;
                        }
                    }

                    if (isBasisVarible && countOne == 1)
                    {
                        resultBasis[i] = simplexTable[indexOfOne][simplexTable[indexOfOne].Count - 1];
                        indexesOfBasisVariable[i] = indexOfOne;
                    }
                }

                PrintSimplexTable(simplexTable, indexesOfBasisVariable);

                indexesOfBasisVariable = new List<int>();
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    indexesOfBasisVariable.Add(int.MaxValue);
                }
            }

            Console.Write("\nОтвет: БР = (");
            for (int i = 0; i < countVariable; i++)
            {
                Console.Write(" " + Math.Round(resultBasis[i], 2) + (i != countVariable - 1 ? "," : ""));
            }

            double result = Math.Round(CalculateTargetFunc(resultBasis), 2);
            Console.Write(" )\n" + (isInvertTarget ? "Fmax = " + result + "\nFmin = " + -result : "F = " + result));
            Console.WriteLine();
        }

        private void TwoStepSimplexMethod()
        {
            List<double> supFunc = new List<double>();
            Console.Write("Вспомогательная функция f' = ");
            for (int i = 0; i < targetFunc.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < limitations.Count - countVariable; j++)
                {
                    sum += limitations[j][i];
                }

                supFunc.Add(-sum);
                Console.Write(supFunc[i] + " ");
            }

            Console.WriteLine("\n");

            double coef;
            List<List<double>> simplexTable = CreateSimplexTableWithSupFunc(supFunc);
            PrintStartSimplexTableWithSupFunc(simplexTable);

            // Пока в доп функции есть отрицательные переменные
            while (SupFuncIsNull(simplexTable[simplexTable.Count - 1]))
            {
                // Выбираем ведущий столбец
                int selectColumn = SelectColumn(simplexTable[simplexTable.Count - 1]);
                // Ведущая строка
                int selectLine = -1;
                // Максимальный по модулю отрицательный коэффициент
                double minCoef = int.MaxValue;
                for (int i = 0; i < simplexTable.Count - 1; i++)
                {
                    // Если правая часть равна 0, то пропускаем
                    if (simplexTable[i][simplexTable[i].Count - 1] == 0) continue;
                    // Если коэффициент в ведущем столбце равен 0, тоже пропускаем
                    if (simplexTable[i][selectColumn] == 0) continue;
                    // Считаем отношение правой части к коэффициенту ведущего столбца
                    coef = simplexTable[i][simplexTable[i].Count - 1] / simplexTable[i][selectColumn];
                    // Если он положительный и меньше минимального, то сохраняем его вместе с позицией строки
                    if (coef > 0 && minCoef > coef)
                    {
                        minCoef = coef;
                        selectLine = i;
                    }
                }

                Console.Write("Столбец " + selectColumn);

                if (selectLine == -1)
                {
                    Console.WriteLine("\nFmin не ограничена\n");
                    return;
                }

                Console.Write(", строка " + selectLine + "\n\n");

                // Делим ведущую строку на коэффициент, расположенный на пересечении выбранных столбца и строки
                coef = simplexTable[selectLine][selectColumn];
                for (int i = 0; i < simplexTable[selectLine].Count; i++)
                {
                    simplexTable[selectLine][i] /= coef;
                }

                // Вычитаем из других строк получившуюся строку с определнным коэффициентом
                for (int i = 0; i < simplexTable.Count; i++)
                {
                    if (i == selectLine) continue;
                    // Считаем коэффициент
                    coef = simplexTable[i][selectColumn];
                    for (int j = 0; j < simplexTable[i].Count; j++)
                    {
                        simplexTable[i][j] -= coef * simplexTable[selectLine][j];
                    }
                }

                List<int> indexesOfBasisVariable = new List<int>();
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    indexesOfBasisVariable.Add(int.MaxValue);
                }

                for (int i = 0; i < targetFunc.Count; i++)
                {
                    bool isBasisVarible = true;
                    int countOne = 0;
                    int indexOfOne = -1;
                    for (int j = 0; j < simplexTable.Count && isBasisVarible; j++)
                    {
                        if (simplexTable[j][i] != 0 && simplexTable[j][i] != 1)
                        {
                            isBasisVarible = false;
                            continue;
                        }

                        if (simplexTable[j][i] == 1)
                        {
                            countOne++;
                            indexOfOne = j;
                        }
                    }

                    if (isBasisVarible && countOne == 1)
                    {
                        resultBasis[i] = simplexTable[indexOfOne][simplexTable[indexOfOne].Count - 1];
                        indexesOfBasisVariable[i] = indexOfOne;
                    }
                }

                PrintSimplexTableWithSupFunc(simplexTable, indexesOfBasisVariable);
            }

            if (SupFuncIsPositive(simplexTable[simplexTable.Count - 1]))
            {
                Console.WriteLine("\nОграничения противоречивы!\n");
                return;
            }

            simplexTable.RemoveAt(simplexTable.Count - 1);

            // Пока в целевой функции есть отрицательные элементы решение не оптимально
            while (SelectColumn(simplexTable[simplexTable.Count - 1]) != -1)
            {
                // Выбираем ведущий столбец
                int selectColumn = SelectColumn(simplexTable[simplexTable.Count - 1]);
                // Ведущая строка
                int selectLine = -1;
                // Максимальный по модулю отрицательный коэффициент
                double minCoef = int.MaxValue;
                for (int i = 0; i < simplexTable.Count - 1; i++)
                {
                    // Если правая часть равна 0, то пропускаем
                    if (simplexTable[i][simplexTable[i].Count - 1] == 0) continue;
                    // Если коэффициент в ведущем столбце равен 0, тоже пропускаем
                    if (simplexTable[i][selectColumn] == 0) continue;
                    // Считаем отношение правой части к коэффициенту ведущего столбца
                    coef = simplexTable[i][simplexTable[i].Count - 1] / simplexTable[i][selectColumn];
                    // Если он положительный и меньше минимального, то сохраняем его вместе с позицией строки
                    if (coef > 0 && minCoef > coef)
                    {
                        minCoef = coef;
                        selectLine = i;
                    }
                }

                if (selectLine == -1)
                {
                    Console.WriteLine("Fmin не ограничена\n");
                    return;
                }

                Console.WriteLine("Столбец " + selectColumn + ", строка " + selectLine + "\n");

                // Делим ведущую строку на коэффициент, расположенный на пересечении выбранных столбца и строки
                coef = simplexTable[selectLine][selectColumn];
                for (int i = 0; i < simplexTable[selectLine].Count; i++)
                {
                    simplexTable[selectLine][i] /= coef;
                }

                // Вычитаем из других строк получившуюся строку с определнным коэффициентом
                for (int i = 0; i < simplexTable.Count; i++)
                {
                    if (i == selectLine) continue;
                    // Считаем коэффициент
                    coef = simplexTable[i][selectColumn];
                    for (int j = 0; j < simplexTable[i].Count; j++)
                    {
                        simplexTable[i][j] -= coef * simplexTable[selectLine][j];
                    }
                }

                List<int> indexesOfBasisVariable = new List<int>();
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    indexesOfBasisVariable.Add(int.MaxValue);
                }

                for (int i = 0; i < targetFunc.Count; i++)
                {
                    for (int j = 0; j < resultBasis.Count; j++)
                    {
                        resultBasis[i] = 0;
                    }

                    bool isBasisVarible = true;
                    int countOne = 0;
                    int indexOfOne = -1;
                    for (int j = 0; j < simplexTable.Count && isBasisVarible; j++)
                    {
                        if (simplexTable[j][i] != 0 && simplexTable[j][i] != 1)
                        {
                            isBasisVarible = false;
                            continue;
                        }

                        if (simplexTable[j][i] == 1)
                        {
                            countOne++;
                            indexOfOne = j;
                        }
                    }

                    if (isBasisVarible && countOne == 1)
                    {
                        resultBasis[i] = simplexTable[indexOfOne][simplexTable[indexOfOne].Count - 1];
                        indexesOfBasisVariable[i] = indexOfOne;
                    }
                }

                PrintSimplexTable(simplexTable, indexesOfBasisVariable);

                indexesOfBasisVariable = new List<int>();
                for (int i = 0; i < targetFunc.Count; i++)
                {
                    indexesOfBasisVariable.Add(int.MaxValue);
                }
            }

            Console.Write("\nОтвет: БР = (");
            for (int i = 0; i < resultBasis.Count; i++)
            {
                Console.Write(" " + Math.Round(resultBasis[i], 2) + (i != resultBasis.Count - 1 ? ";" : ""));
            }

            double result =
                Math.Round(simplexTable[simplexTable.Count - 1][simplexTable[simplexTable.Count - 1].Count - 1], 2);
            //double result = Math.Round(CalculateTargetFunc(resultBasis), 2);
            Console.Write(" )\n" + (isInvertTarget ? "Fmax = " + result + "\nFmin = " + -result : "F = " + result));
            Console.WriteLine();
        }

        private bool SupFuncIsPositive(List<double> supFunc)
        {
            for (int i = 0; i < supFunc.Count - 1; i++)
            {
                if (Math.Round(supFunc[i], 1) > 0.0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool SupFuncIsNull(List<double> supFunc)
        {
            for (int i = 0; i < supFunc.Count - 1; i++)
            {
                if (Math.Round(supFunc[i], 1) < 0.0)
                {
                    return true;
                }
            }

            return false;
        }

        private List<List<double>> CreateSimplexTableWithSupFunc(List<double> supFunc)
        {
            List<List<double>> simplexTable = new List<List<double>>();

            // i - строка
            for (int i = 0; i < limitations.Count - countVariable; i++)
            {
                simplexTable.Add(new List<double>());

                // j - столбец, выводим коэффициенты ограничений
                for (int j = 0; j < limitations[i].Count; j++)
                {
                    simplexTable[i].Add(limitations[i][j]);
                }

                simplexTable[i].Add(limits[i]);
            }

            simplexTable.Add(new List<double>());
            // Строка с F
            for (int i = 0; i < targetFunc.Count; i++)
            {
                simplexTable[simplexTable.Count - 1].Add(targetFunc[i]);
            }

            simplexTable[simplexTable.Count - 1].Add(CalculateTargetFunc(startBasis));

            simplexTable.Add(new List<double>());
            // Строка с F'
            for (int i = 0; i < supFunc.Count; i++)
            {
                simplexTable[simplexTable.Count - 1].Add(supFunc[i]);
            }

            double sum = 0;
            for (int i = 0; i < limitations.Count - countVariable; i++)
            {
                sum += limits[i];
            }

            simplexTable[simplexTable.Count - 1].Add(-sum);

            return simplexTable;
        }

        private List<List<double>> CreateSimplexTable()
        {
            List<List<double>> simplexTable = new List<List<double>>();

            int k = 0;
            // i - строка
            for (int i = 0; i < limitations.Count - countVariable; i++)
            {
                simplexTable.Add(new List<double>());

                // j - столбец, выводим коэффициенты ограничений
                for (int j = 0; j < limitations[i].Count; j++)
                {
                    simplexTable[i].Add(limitations[i][j]);
                }

                do
                {
                    k++;
                } while (k < startBasis.Count && startBasis[k] == 0);

                // Добавляем значение функции
                if (k < startBasis.Count)
                {
                    simplexTable[i].Add(startBasis[k]);
                }
            }

            simplexTable.Add(new List<double>());
            // Строка с F
            for (int i = 0; i < targetFunc.Count; i++)
            {
                simplexTable[simplexTable.Count - 1].Add(targetFunc[i]);
            }

            simplexTable[simplexTable.Count - 1].Add(CalculateTargetFunc(startBasis));

            return simplexTable;
        }

        private void PrintStartSimplexTableWithSupFunc(List<List<double>> simplexTable)
        {
            // Строка с обозначением столбцов
            for (int i = 0; i < limitations[0].Count; i++)
            {
                Console.Write("\tx" + (i + 1));
            }

            Console.Write("\tF(x)\n");

            int k = 0;
            for (int i = 0; i < simplexTable.Count; i++)
            {
                // Выводим F'
                if (i == simplexTable.Count - 1)
                {
                    Console.Write("F'");
                }
                // Выводим F
                else if (i == simplexTable.Count - 2)
                {
                    Console.Write("F");
                }
                // Выводим базисную переменную
                else
                {
                    do
                    {
                        k++;
                    } while (k < startBasis.Count && startBasis[k] == 0);

                    if (k < startBasis.Count)
                    {
                        Console.Write("x" + (k + 1));
                    }
                }

                // j - столбец, выводим коэффициенты
                for (int j = 0; j < simplexTable[i].Count; j++)
                {
                    Console.Write("\t" + Math.Round(simplexTable[i][j], 2));
                }

                Console.WriteLine();
            }
        }

        private void PrintStartSimplexTable(List<List<double>> simplexTable)
        {
            // Строка с обозначением столбцов
            for (int i = 0; i < limitations[0].Count; i++)
            {
                Console.Write("\tx" + (i + 1));
            }

            Console.Write("\tF(x)\n");

            int k = 0;
            for (int i = 0; i < simplexTable.Count; i++)
            {
                // Выводим F
                if (i == simplexTable.Count - 1)
                {
                    Console.Write("F");
                }
                // Выводим базисную переменную
                else
                {
                    do
                    {
                        k++;
                    } while (k < startBasis.Count && startBasis[k] == 0);

                    Console.Write("x" + (k + 1));
                }

                // j - столбец, выводим коэффициенты
                for (int j = 0; j < simplexTable[i].Count; j++)
                {
                    Console.Write("\t" + Math.Round(simplexTable[i][j], 2));
                }

                Console.WriteLine();
            }
        }

        private void PrintSimplexTableWithSupFunc(List<List<double>> simplexTable, List<int> indexesOfOne)
        {
            // Строка с обозначением столбцов
            for (int i = 0; i < limitations[0].Count; i++)
            {
                Console.Write("\tx" + (i + 1));
            }

            Console.Write("\tF(x)\n");

            for (int i = 0; i < simplexTable.Count; i++)
            {
                // Выводим F'
                if (i == simplexTable.Count - 1)
                {
                    Console.Write("F'");
                }
                // Выводим F
                else if (i == simplexTable.Count - 2)
                {
                    Console.Write("F");
                }
                // Выводим базисную переменную
                else
                {
                    int x = indexesOfOne.Min();
                    Console.Write("x" + (indexesOfOne.IndexOf(x) + 1));
                    indexesOfOne[indexesOfOne.IndexOf(x)] = int.MaxValue;
                }

                // j - столбец, выводим коэффициенты
                for (int j = 0; j < simplexTable[i].Count; j++)
                {
                    Console.Write("\t" + Math.Round(simplexTable[i][j], 2));
                }

                Console.WriteLine();
            }
        }

        private void PrintSimplexTable(List<List<double>> simplexTable, List<int> indexesOfOne)
        {
            // Строка с обозначением столбцов
            for (int i = 0; i < limitations[0].Count; i++)
            {
                Console.Write("\tx" + (i + 1));
            }

            Console.Write("\tF(x)\n");

            for (int i = 0; i < simplexTable.Count; i++)
            {
                // Выводим F
                if (i == simplexTable.Count - 1)
                {
                    Console.Write("F");
                }
                // Выводим базисную переменную
                else
                {
                    int x = indexesOfOne.Min();
                    Console.Write("x" + (indexesOfOne.IndexOf(x) + 1));
                    indexesOfOne[indexesOfOne.IndexOf(x)] = int.MaxValue;
                }

                // j - столбец, выводим коэффициенты
                for (int j = 0; j < simplexTable[i].Count; j++)
                {
                    Console.Write("\t" + Math.Round(simplexTable[i][j], 2));
                }

                Console.WriteLine();
            }
        }

        private double CalculateTargetFunc(List<double> x)
        {
            double result = 0.0;
            for (int i = 0; i < targetFunc.Count; i++)
            {
                if (targetFunc[i] == 0) continue;
                result += (isInvertTarget ? -targetFunc[i] : targetFunc[i]) * x[i];
            }

            return result;
        }

        private int SelectColumn(List<double> targetFunc)
        {
            List<double> buf = new List<double>();
            for (int i = 0; i < targetFunc.Count - 1; i++)
            {
                buf.Add(targetFunc[i]);
            }

            double minElement = buf.Min();
            if (minElement >= 0) return -1;
            return buf.IndexOf(minElement);
        }
    }
}