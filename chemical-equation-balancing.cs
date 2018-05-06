using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace CodingGame.Puzzles
{
    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    public class Solution
    {
        static void Main(string[] args)
        {
            // H2 + O2 -> H2O
            // 2H2 + O2 -> 2H2O
            string unbalanced;
            if (args.Length == 0)
                unbalanced = Console.ReadLine();
            else
                unbalanced = args[0];

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            var b = new Balancer();
//             var obj = b.YieldCombinationsOfN(4,1,3);
//             Logger.Write(obj.Count());
// return;            
            // Logger.Write(obj);
            var balanced = b.Calculate(unbalanced);

            Console.WriteLine(balanced);
        }
    }


    class Balancer
    {
        public string Calculate(string equation)
        {
            var eq = Parse(equation);
            
            var eqResult = Balance(eq);
            return eqResult.ToString();
        }

        private Equation Parse(string equation)
        {
            Logger.DebugLine($"Parsing: {equation}");

            //H2 + O2 -> H2O
            var sLeft = equation.GetUntilOrEmpty("->").Trim();
            var sRight = equation.GetFromOrEmpty("->").Trim();

            var left = ParseEquationSide(sLeft);
            var right = ParseEquationSide(sRight);

            //Logger.Debug("Parsed LeftSide: ");
            //Logger.Dump(left);
            //Logger.Debug("Parsed RightSide: ");
            //Logger.Dump(right);

            var eq = new Equation();
            eq.LeftSide = left;
            eq.RightSide = right;
            /*eq.LeftSide = new EquationSide()
            {
                Molecules = new List<Molecule>()
                {
                    new Molecule()
                    {
                        Coefficient = 1,
                        Elements = new List<Element>()
                        {
                            new Element()
                            {
                                Name = "H",
                                Quantity = 2
                            }
                        }
                    },
                    new Molecule()
                    {
                        Coefficient = 1,
                        Elements = new List<Element>()
                        {
                            new Element()
                            {
                                Name = "O",
                                Quantity = 2
                            }
                        }
                    }
                }
            };

            eq.RightSide = new EquationSide()
            {
                Molecules = new List<Molecule>()
                {
                    new Molecule()
                    {
                        Coefficient = 1,
                        Elements = new List<Element>()
                        {
                            new Element()
                            {
                                Name = "H",
                                Quantity = 2
                            },
                            new Element()
                            {
                                Name = "O",
                                Quantity = 1
                            }
                        }
                    }
                }
            };*/

            if (eq.ToString() != equation)
            {
                Logger.DebugLine("Errore nel parsing... stringhe non congruenti");
            }

            Logger.DebugLine("Parsing corretto");

            return eq;
        }

        private Equation Balance(Equation eq)
        {
            // Salto normalizzazione (Distribuire Coefficient su Elements)

            var leftElements = eq.LeftSide.Molecules.SelectMany(m => m.Elements);
            var rightElements = eq.RightSide.Molecules.SelectMany(m => m.Elements);
            var numIncognite = eq.LeftSide.Molecules.Count() + eq.RightSide.Molecules.Count();

            var distinctElements = leftElements.Select(e => e.Name).ToList();
            distinctElements.AddRange(rightElements.Select(e => e.Name));
            distinctElements = distinctElements.Distinct().ToList();
            var numElements = distinctElements.Count;

            var Y = new int[numIncognite];
            var X = new int[numElements, numIncognite];

            int i = 0;
            foreach (var el in distinctElements)
            {
                int j = 0;
                foreach (var m in eq.LeftSide.Molecules)
                {
                    var elementsInMol = m.Elements.Where(e => e.Name == el);
                    X[i, j] = elementsInMol.Sum(e => e.Quantity);
                    j++;
                }

                foreach (var m in eq.RightSide.Molecules)
                {
                    var elementsInMol = m.Elements.Where(e => e.Name == el);
                    X[i, j] = -elementsInMol.Sum(e => e.Quantity);
                    j++;
                }


                i++;
            }
            Logger.DebugLine("Matrice coefficienti:");
            Logger.Debug(X);

            var result = Resolve(X);
            if (result == null)
            {
                Console.WriteLine("Risultato non trovato");
                Environment.Exit(0);
            }

            var eqResult = CreaEquazioneBilanciata(eq, result);
            return eqResult;
        }

        private Equation CreaEquazioneBilanciata(Equation eq, int[] result)
        {

            int index = 0;
            foreach(var m in eq.LeftSide.Molecules)
            {
                m.Coefficient = result[index];
                index++;
            }

            foreach (var m in eq.RightSide.Molecules)
            {
                m.Coefficient = result[index];
                index++;
            }

            return eq;
        }

        private int[] Resolve(int[,] matrix)
        {
            var numIncognite = matrix.GetLength(1); //numColumns
            var permutations = YieldCombinationsOfN(numIncognite, 1, 6);

            Logger.DebugLine($"Bruteforce di {permutations.Count()} soluzioni");

            foreach (var p in permutations)
            {
                //var p = new int[] { 2, 1, 2 };
                var isOk = VerificaSoluzione(matrix, p.ToArray());
                if (isOk)
                {
                    Logger.DebugLine("Soluzione trovata");
                    return p.ToArray();
                }
            }

            return null;
        }

        private bool VerificaSoluzione(int[,] arr, int[] sol)
        {
            Logger.DebugLine("Verifica soluzione: ({0})", string.Join(",", sol));
            var rowCount = arr.GetLength(0);
            var colCount = arr.GetLength(1);
            var sum = new int[rowCount];
            
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    sum[row] += arr[row, col] * sol[col];
                }    
            }
            Logger.DebugLine("Risultato: ({0})", string.Join(",", sum));
            return sum.All(x => x == 0);
        }

        private EquationSide ParseEquationSide(string side)
        {
            //Logger.DebugLine($"Side: {side}");
            var eqSide = new EquationSide();
            //H2 + O2
            var molecules = side.Split('+');

            foreach (var sM in molecules)
            {
                //Logger.DebugLine($"Parsing Mol: {sM}");
                var molecule = new Molecule();
                
                var matchCoeff = Regex.Match(sM, @"^\d+");
                int coeffLength;
                if (matchCoeff.Success)
                {
                    molecule.Coefficient = int.Parse(matchCoeff.Value);
                    coeffLength = matchCoeff.Value.Length;
                    //Logger.DebugLine($"Coeff: {molecule.Coefficient}");
                }
                else
                {
                    coeffLength = 0;
                }

                var sElements = sM.Substring(matchCoeff.Value.ToString().Length).Trim();
                //Logger.DebugLine($"Other: {sElements}");


                /*
                // H2
                do
                {
                    // Logger.DebugLine("Ciclo");
                    var element = new Element();
                    element.Name = Regex.Match(sElements, @"^\D+").Value;
                    //Logger.DebugLine($"El Name: {element.Name}");

                    //if(!matchCoeff.Success) Logger.DebugLine("ERR 1");
                    sElements = sElements.Substring(element.Name.Length);
                    //Logger.DebugLine($"Other (Q): {sElements}");

                    var qMatch = Regex.Match(sElements, @"^\d+");
                    int qLength;
                    if (qMatch.Success)
                    {
                        element.Quantity = int.Parse(qMatch.Value);
                        qLength = qMatch.Value.Length;
                    }
                    else
                    {
                        element.Quantity = 1;
                        qLength = 0;
                    }
                    //Logger.DebugLine($"El Quantity: {element.Quantity}");
                    molecule.Elements.Add(element);

                    sElements = sElements.Substring(qLength).Trim();
                    //Logger.DebugLine($"Other Elements: {sElements}");

                }
                while (!string.IsNullOrEmpty(sElements));*/

                var matches = Regex.Matches(sElements, @"[A-Z][a-z]?\d*");
                foreach(Match m in matches)
                {
                    var element = new Element();

                    element.Name = Regex.Match(m.Value, @"[A-Z][a-z]?").Value;

                    var qMatch = Regex.Match(m.Value, @"\d+$");

                    if (qMatch.Success)
                    {
                        element.Quantity = int.Parse(qMatch.Value);
                    }
                    else
                    {
                        element.Quantity = 1;
                    }
                    molecule.Elements.Add(element);
                }

                //Logger.DebugLine("Aggiunta molecola");
                eqSide.Molecules.Add(molecule);
            }

            return eqSide;
        }

        public IEnumerable<List<int>> YieldCombinationsOfN(int places, int digitMin, int digitMax)
        {
            int n = digitMax - digitMin + 1;
            int numericMax = (int)Math.Pow(n, places);

            for (int i = 0; i < numericMax; i++)
            {
                List<int> li = new List<int>(places);
                for (int digit = 0; digit < places; digit++)
                {
                    li.Add(((int)(i / Math.Pow(n, digit)) % n) + digitMin);
                }
                yield return li;
            }
        }

        private void ComputeCoefficents(double[,] X, double[] Y)
        {
            int I, J, K, K1, N;
            N = Y.Length;
            for (K = 0; K < N; K++)
            {
                K1 = K + 1;
                for (I = K; I < N; I++)
                {
                    if (X[I, K] != 0)
                    {
                        for (J = K1; J < N; J++)
                        {
                            X[I, J] /= X[I, K];
                        }
                        Y[I] /= X[I, K];
                    }
                }
                for (I = K1; I < N; I++)
                {
                    if (X[I, K] != 0)
                    {
                        for (J = K1; J < N; J++)
                        {
                            X[I, J] -= X[K, J];
                        }
                        Y[I] -= Y[K];
                    }
                }
            }
            for (I = N - 2; I >= 0; I--)
            {
                for (J = N - 1; J >= I + 1; J--)
                {
                    Y[I] -= X[I, J] * Y[J];
                }
            }
        }
    }

    class Equation
    {
        public EquationSide LeftSide { get; set; }
        public EquationSide RightSide { get; set; }

        public override string ToString()
        {
            return LeftSide.ToString() + " -> " + RightSide.ToString();
        }
    }

    class EquationSide
    {
        public IList<Molecule> Molecules { get; set; }
        public EquationSide()
        {
            Molecules = new List<Molecule>();
        }

        public override string ToString()
        {
            return string.Join(" + ", Molecules);
        }
    }

    class Molecule
    {
        public int Coefficient { get; set; }
        public IList<Element> Elements { get; set; }

        public Molecule()
        {
            Coefficient = 1;
            Elements = new List<Element>();
        }

        public override string ToString()
        {
            return (Coefficient > 1 ? Coefficient.ToString() : "") + string.Join("", Elements);
        }
    }

    class Element
    {
        public string Name { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return Name + (Quantity > 1 ? Quantity.ToString() : "");
        }

        // public override bool Equals(object value)
        // {
        //     Element el = value as Element;

        //     return !Object.ReferenceEquals(null, el)
        //         && String.Equals(Name, el.Name);
        // }
        // public override int GetHashCode() => Name.GetHashCode();
    }

    class Logger
    {
        public static void Debug(int[,] arr)
        {
            var rowCount = arr.GetLength(0);
            var colCount = arr.GetLength(1);
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                    Console.Error.Write(String.Format("{0}\t", arr[row, col]));
                Console.Error.WriteLine();
            }
        }
        public static void Debug(string s)
        {
            Console.Error.Write(s);
        }
        public static void DebugLine(string s)
        {
            Console.Error.WriteLine(s);
        }
        public static void Debug(string format, params object[] arg)
        {
            Console.Error.Write(format, arg);
        }
        public static void DebugLine(string format, params object[] arg)
        {
            Console.Error.WriteLine(format, arg);
        }
        public static void Dump(object obj)
        {
            var dump = ObjectDumper.Dump(obj, 4);
            Logger.Debug(dump);
        }
    }

    static class Extensions
    {
        public static string GetUntilOrEmpty(this string text, string stopAt)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public static string GetFromOrEmpty(this string text, string startAt)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(startAt, StringComparison.Ordinal) + startAt.Length;

                if (charLocation > 0)
                {
                    return text.Substring(charLocation);
                }
            }

            return String.Empty;
        }
    }
    public class ObjectDumper
    {
        private int _level;
        private readonly int _indentSize;
        private readonly StringBuilder _stringBuilder;
        private readonly List<int> _hashListOfFoundElements;

        private ObjectDumper(int indentSize)
        {
            _indentSize = indentSize;
            _stringBuilder = new StringBuilder();
            _hashListOfFoundElements = new List<int>();
        }

        public static string Dump(object element)
        {
            return Dump(element, 2);
        }

        public static string Dump(object element, int indentSize)
        {
            var instance = new ObjectDumper(indentSize);
            return instance.DumpElement(element);
        }

        private string DumpElement(object element)
        {
            if (element == null || element is ValueType || element is string)
            {
                Write(FormatValue(element));
            }
            else
            {
                var objectType = element.GetType();
                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    Write("{{{0}}}", objectType.FullName);
                    _hashListOfFoundElements.Add(element.GetHashCode());
                    _level++;
                }

                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            _level++;
                            DumpElement(item);
                            _level--;
                        }
                        else
                        {
                            if (!AlreadyTouched(item))
                                DumpElement(item);
                            else
                                Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var memberInfo in members)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        var propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo == null && propertyInfo == null)
                            continue;

                        var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                        object value = fieldInfo != null
                                           ? fieldInfo.GetValue(element)
                                           : propertyInfo.GetValue(element, null);

                        if (type.IsValueType || type == typeof(string))
                        {
                            Write("{0}: {1}", memberInfo.Name, FormatValue(value));
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                            Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                            var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                            _level++;
                            if (!alreadyTouched)
                                DumpElement(value);
                            else
                                Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                            _level--;
                        }
                    }
                }

                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    _level--;
                }
            }

            return _stringBuilder.ToString();
        }

        private bool AlreadyTouched(object value)
        {
            if (value == null)
                return false;

            var hash = value.GetHashCode();
            for (var i = 0; i < _hashListOfFoundElements.Count; i++)
            {
                if (_hashListOfFoundElements[i] == hash)
                    return true;
            }
            return false;
        }

        private void Write(string value, params object[] args)
        {
            var space = new string(' ', _level * _indentSize);

            if (args != null)
                value = string.Format(value, args);

            _stringBuilder.AppendLine(space + value);
        }

        private string FormatValue(object o)
        {
            if (o == null)
                return ("null");

            if (o is DateTime)
                return (((DateTime)o).ToShortDateString());

            if (o is string)
                return string.Format("\"{0}\"", o);

            if (o is char && (char)o == '\0')
                return string.Empty;

            if (o is ValueType)
                return (o.ToString());

            if (o is IEnumerable)
                return ("...");

            return ("{ }");
        }
    }
}