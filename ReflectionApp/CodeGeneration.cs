using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ReflectionApp
{
    public class CodeGeneration
    {
        /// <summary>
        /// Returns the functions that returns vectors' scalar product:
        /// (a1, a2,...,aN) * (b1, b2, ..., bN) = a1*b1 + a2*b2 + ... + aN*bN
        /// Generally-обычно, CLR does not allow to implement such a method via generics to have one function for various number types:
        /// int, long, float. double.
        /// But it is possible to generate the method in the run time! 
        /// See the idea of code generation using Expression Tree at: 
        /// http://blogs.msdn.com/b/csharpfaq/archive/2009/09/14/generating-dynamic-methods-with-expression-trees-in-visual-studio-2010.aspx
        /// </summary>
        /// <typeparam name="T">number type (int, long, float etc)</typeparam>
        /// <returns>
        ///   The function that return scalar product of two vectors
        ///   The generated dynamic method should be equal to static MultuplyVectors (see below).   
        /// </returns>
        public static Func<T[], T[], T> GetVectorMultiplyFunction<T>() where T : struct
        {
            var arParam1 = Expression.Parameter(typeof(T[]), "ar1");
            var arParam2 = Expression.Parameter(typeof(T[]), "ar2");
            var arIndex= Expression.Parameter(typeof(int), "i");
           
            
            var result = Expression.Parameter(typeof(T), "result");
            LabelTarget label = Expression.Label(typeof(T));


            BlockExpression block = Expression.Block(new[] {result , arIndex },
                Expression.Assign(result , Expression.Constant(default(T))), Expression.Assign(arIndex, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.And(
                            Expression.LessThan(arIndex, Expression.ArrayLength(arParam1)),
                            Expression.LessThan(arIndex, Expression.ArrayLength(arParam2))),
                        Expression.AddAssign(result,
                            Expression.Multiply(
                               Expression.ArrayIndex(arParam1, arIndex),
                                Expression.ArrayIndex(arParam2, Expression.PostIncrementAssign(arIndex))
                                )),
                        Expression.Break(label, result)),
                    label));



            return Expression.Lambda<Func<T[], T[], T>>(block, arParam1, arParam2).Compile();


            // TODO : Implement GetVectorMultiplyFunction<T>.
            //throw new NotImplementedException();
        }


        // Static solution to check performance benchmarks
        public static int MultuplyVectors(int[] first, int[] second)
        {
            int result = 0;
            for (int i = 0; i < first.Length; i++)
            {
                result += first[i] * second[i];
            }
            return result;
        }

    }
}
