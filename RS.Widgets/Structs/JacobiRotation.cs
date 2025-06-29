using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Structs
{
    // See section 8.4 of Golub & Van Loan "Matrix Computation"
    // Remember, J is this Jacobi rotation.  JT is the transpose.
    // Also section 5.1.8 shows the formula for multiplying by the Jacobi rotation
    // Though as they helpfully point out a Jacobi rotation is the same as a Givens rotation
    public struct JacobiRotation
    {
        public JacobiRotation(int p, int q, double[,] a)
        {
            // Constructs a 2D rotation matrix M as follows
            // Start with identity.
            // Zero the p & q rows & columns
            // Then set the intersections of these rows & columns as follows
            //    [ M_pp M_pq ]  =  [ c s]
            //    [ M_qp M_qq ]     [-s c]
            //
            // Where c & s are a cosine-sine pair calculated so that multiplying a by this will
            // descrease the the off-diagonal elements.

            _p = p;
            _q = q;

            double tau = (a[q, q] - a[p, p]) / (2 * a[p, q]);
            if (tau < Double.MaxValue && tau > -Double.MaxValue)
            {
                double root = Math.Sqrt(1 + tau * tau);
                // Choose the smaller of -tau +/- root
                double t = -tau < 0 ? -tau + root : -tau - root;
                _c = 1 / Math.Sqrt(1 + t * t);
                _s = t * _c;
            }
            else
            {
                _c = 1;
                _s = 0;
            }
        }

        // These functions overwrite & return their argument.

        // returns JT a J
        public double[,] LeftRightMultiply(double[,] a)
        {
            return RightMultiply(LeftMultiplyTranspose(a));
        }

        // returns a J
        public double[,] RightMultiply(double[,] a)
        {
            for (int j = 0; j < 4; ++j)
            {
                double tau1 = a[j, _p];
                double tau2 = a[j, _q];

                a[j, _p] = _c * tau1 - _s * tau2;
                a[j, _q] = _s * tau1 + _c * tau2;
            }

            return a;
        }


        // returns JT a
        public double[,] LeftMultiplyTranspose(double[,] a)
        {
            for (int j = 0; j < 4; ++j)
            {
                double tau1 = a[_p, j];
                double tau2 = a[_q, j];

                a[_p, j] = _c * tau1 - _s * tau2;
                a[_q, j] = _s * tau1 + _c * tau2;
            }

            return a;
        }

        private int _p, _q;
        private double _c, _s;
    }
}
