using RS.Widgets.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Standard
{
    public class DpiScale2 : IEquatable<DpiScale2>, IEquatable<DpiScale>
    {
        private DpiScale dpiScale;

        public double DpiScaleX => dpiScale.DpiScaleX;

        public double DpiScaleY => dpiScale.DpiScaleY;

        public double PixelsPerDip => dpiScale.PixelsPerDip;

        public double PixelsPerInchX => dpiScale.PixelsPerInchX;

        public double PixelsPerInchY => dpiScale.PixelsPerInchY;

        public DpiScale2(DpiScale dpiScale)
        {
            this.dpiScale = dpiScale;
        }

        public DpiScale2(double dpiScaleX, double dpiScaleY)
            : this(new DpiScale(dpiScaleX, dpiScaleY))
        {
        }

        public static implicit operator DpiScale(DpiScale2 dpiScale2)
        {
            return dpiScale2.dpiScale;
        }

        public static bool operator !=(DpiScale2 dpiScaleA, DpiScale2 dpiScaleB)
        {
            if ((object)dpiScaleA == null && (object)dpiScaleB != null || (object)dpiScaleA != null && (object)dpiScaleB == null)
            {
                return true;
            }

            return !dpiScaleA.Equals(dpiScaleB);
        }

        public static bool operator ==(DpiScale2 dpiScaleA, DpiScale2 dpiScaleB)
        {
            if ((object)dpiScaleA == null && (object)dpiScaleB == null)
            {
                return true;
            }

            return dpiScaleA.Equals(dpiScaleB);
        }

        public bool Equals(DpiScale dpiScale)
        {
            if (DoubleHelper.AreClose(DpiScaleX, dpiScale.DpiScaleX))
            {
                return DoubleHelper.AreClose(DpiScaleY, dpiScale.DpiScaleY);
            }

            return false;
        }

        public bool Equals(DpiScale2 dpiScale2)
        {
            if ((object)dpiScale2 == null)
            {
                return false;
            }

            return Equals(dpiScale2.dpiScale);
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is DpiScale)
            {
                return Equals((DpiScale)obj);
            }

            if (obj is DpiScale2)
            {
                return Equals((DpiScale2)obj);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ((int)PixelsPerInchX).GetHashCode();
        }

        public static DpiScale2 FromPixelsPerInch(double ppiX, double ppiY)
        {
            if (DoubleHelper.LessThanOrClose(ppiX, 0.0) || DoubleHelper.LessThanOrClose(ppiY, 0.0))
            {
                return null;
            }

            return new DpiScale2(ppiX / 96.0, ppiY / 96.0);
        }
    }
}
