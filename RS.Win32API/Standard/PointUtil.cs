using RS.Win32API.Structs;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace RS.Win32API.Standard
{
    public static class PointUtil
    {
        
        public static Point ClientToRoot(Point point, PresentationSource presentationSource)
        {
            bool success = true;
            return TryClientToRoot(point, presentationSource, throwOnError: true, out success);
        }
     
        public static Point TryClientToRoot(Point point, PresentationSource presentationSource, bool throwOnError, out bool success)
        {
            if (throwOnError || presentationSource != null && presentationSource.CompositionTarget != null && !presentationSource.IsDisposed)
            {
                point = presentationSource.CompositionTarget.TransformFromDevice.Transform(point);
                point = TryApplyVisualTransform(point, presentationSource.RootVisual, inverse: true, throwOnError, out success);
                return point;
            }

            success = false;
            return new Point(0.0, 0.0);
        }
        
        public static Point RootToClient(Point point, PresentationSource presentationSource)
        {
            point = ApplyVisualTransform(point, presentationSource.RootVisual, inverse: false);
            point = presentationSource.CompositionTarget.TransformToDevice.Transform(point);
            return point;
        }

        public static Point ApplyVisualTransform(Point point, Visual v, bool inverse)
        {
            bool success = true;
            return TryApplyVisualTransform(point, v, inverse, throwOnError: true, out success);
        }

        public static Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out bool success)
        {
            success = true;
            if (v != null)
            {
                Matrix visualTransform = GetVisualTransform(v);
                if (inverse)
                {
                    if (!throwOnError && !visualTransform.HasInverse)
                    {
                        success = false;
                        return new Point(0.0, 0.0);
                    }

                    visualTransform.Invert();
                }

                point = visualTransform.Transform(point);
            }

            return point;
        }

        public static Matrix GetVisualTransform(Visual v)
        {
            if (v != null)
            {
                Matrix matrix = Matrix.Identity;
                Transform transform = VisualTreeHelper.GetTransform(v);
                if (transform != null)
                {
                    Matrix value = transform.Value;
                    matrix = Matrix.Multiply(matrix, value);
                }

                Vector offset = VisualTreeHelper.GetOffset(v);
                matrix.Translate(offset.X, offset.Y);
                return matrix;
            }

            return Matrix.Identity;
        }

     
        public static Point ClientToScreen(Point pointClient, PresentationSource presentationSource)
        {
            if (!(presentationSource is HwndSource hwndSource))
            {
                return pointClient;
            }

            HandleRef handleRef = new HandleRef(hwndSource, hwndSource.Handle);
           POINT pt = FromPoint(pointClient);
           POINT pt2 = AdjustForRightToLeft(pt, handleRef);
            NativeMethods.ClientToScreen(handleRef, pt2);
            return ToPoint(pt2);
        }


        public static Point ScreenToClient(Point pointScreen, PresentationSource presentationSource)
        {
            if (!(presentationSource is HwndSource hwndSource))
            {
                return pointScreen;
            }

            HandleRef handleRef = new HandleRef(hwndSource, hwndSource.Handle);
            POINT pt = FromPoint(pointScreen);
            NativeMethods.ScreenToClient(handleRef, pt);
            pt = AdjustForRightToLeft(pt, handleRef);
            return ToPoint(pt);
        }


        public static Rect ElementToRoot(Rect rectElement, Visual element, PresentationSource presentationSource)
        {
            GeneralTransform generalTransform = element.TransformToAncestor(presentationSource.RootVisual);
            return generalTransform.TransformBounds(rectElement);
        }


        public static Rect RootToClient(Rect rectRoot, PresentationSource presentationSource)
        {
            CompositionTarget compositionTarget = presentationSource.CompositionTarget;
            Matrix visualTransform = GetVisualTransform(compositionTarget.RootVisual);
            Rect rect = Rect.Transform(rectRoot, visualTransform);
            Matrix transformToDevice = compositionTarget.TransformToDevice;
            return Rect.Transform(rect, transformToDevice);
        }


        public static Rect ClientToScreen(Rect rectClient, HwndSource hwndSource)
        {
            Point point = ClientToScreen(rectClient.TopLeft, hwndSource);
            Point point2 = ClientToScreen(rectClient.BottomRight, hwndSource);
            return new Rect(point, point2);
        }

        public static POINT AdjustForRightToLeft(POINT pt, HandleRef handleRef)
        {
            nint windowStyle = NativeMethods.GetWindowStyle(handleRef, exStyle: true);
            if ((windowStyle & 0x400000) == 4194304)
            {
                RECT rect = default;
               NativeMethods.GetClientRect(handleRef, ref rect);
                pt.x = rect.Right - pt.x;
            }

            return pt;
        }

        public static RECT AdjustForRightToLeft(RECT rc, HandleRef handleRef)
        {
            nint windowStyle = NativeMethods.GetWindowStyle(handleRef, exStyle: true);
            if ((windowStyle & 0x400000) == 4194304)
            {
                RECT rect = default;
                NativeMethods.GetClientRect(handleRef, ref rect);
                int num = rc.Right - rc.Left;
                rc.Right = rect.Right - rc.Left;
                rc.Left = rc.Right - num;
            }

            return rc;
        }

        public static POINT FromPoint(Point point)
        {
            return new POINT(DoubleUtil.DoubleToInt(point.X), DoubleUtil.DoubleToInt(point.Y));
        }

        public static Point ToPoint(POINT pt)
        {
            return new Point(pt.x, pt.y);
        }

        public static RECT FromRect(Rect rect)
        {
            RECT result = default;
            result.Top = DoubleUtil.DoubleToInt(rect.Y);
            result.Left = DoubleUtil.DoubleToInt(rect.X);
            result.Bottom = DoubleUtil.DoubleToInt(rect.Bottom);
            result.Right = DoubleUtil.DoubleToInt(rect.Right);
            return result;
        }

        public static Rect ToRect(RECT rc)
        {
            Rect result = default;
            result.X = rc.Left;
            result.Y = rc.Top;
            result.Width = rc.Right - rc.Left;
            result.Height = rc.Bottom - rc.Top;
            return result;
        }
    }
}
