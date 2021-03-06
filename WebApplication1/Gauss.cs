﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sle.Solver {
    public class SolverException : Exception {
        public const string NO_EQUATIONS = "No equations";
        public const string NO_OR_INFINITE = "The system has no or infinite number of solutions.";
        public const string OVERDEFINED = "The system is overdefined (number of unknowns is less than the number of equations).";
        public SolverException(String message) : base(message) {  }
    }

    public class GaussianElimination {
        private static double EPSILON = 1e-10;

        // Gaussian elimination with partial pivoting
        public static double[] lsolve(double[][] A, double[] b) {
                       
            int N = b.Length;

            for (int p = 0; p < N; p++) {

                // find pivot row and swap
                int max = p;
                for (int i = p + 1; i < N; i++) {
                    if (Math.Abs(A[i][p]) > Math.Abs(A[max][p])) {
                        max = i;
                    }
                }
                double[] temp = A[p]; A[p] = A[max]; A[max] = temp;
                double t = b[p]; b[p] = b[max]; b[max] = t;

                // singular or nearly singular
                if (Math.Abs(A[p][p]) <= EPSILON) {
                    throw new SolverException(SolverException.NO_OR_INFINITE);
                }

                // pivot within A and b
                for (int i = p + 1; i < N; i++) {
                    double alpha = A[i][p] / A[p][p];
                    b[i] -= alpha * b[p];
                    for (int j = p; j < N; j++) {
                        A[i][j] -= alpha * A[p][j];
                    }
                }
            }

            // back substitution
            double[] x = new double[N];
            for (int i = N - 1; i >= 0; i--) {
                double sum = 0.0;
                for (int j = i + 1; j < N; j++) {
                    sum += A[i][j] * x[j];
                }
                x[i] = (b[i] - sum) / A[i][i];
            }
            return x;
        }
    }
}