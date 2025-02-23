using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;


namespace RS.Widgets.PixelShaders
{
	
	/// <summary>Adjust the brightness, contrast, and saturation of the image</summary>
	public class BrightnessContrastEffect : ShaderEffect {
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BrightnessContrastEffect), 0);
		public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(BrightnessContrastEffect), new UIPropertyMetadata(((double)(0D)), PixelShaderConstantCallback(0)));
		public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(BrightnessContrastEffect), new UIPropertyMetadata(((double)(0D)), PixelShaderConstantCallback(1)));
		public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(BrightnessContrastEffect), new UIPropertyMetadata(((double)(1D)), PixelShaderConstantCallback(2)));
		public BrightnessContrastEffect() {
			PixelShader pixelShader = new PixelShader();
			pixelShader.UriSource = new Uri("pack://application:,,,/RS.Widgets;component/PixelShaders/BrightnessContrastEffect.ps", UriKind.RelativeOrAbsolute);
			this.PixelShader = pixelShader;

			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(BrightnessProperty);
			this.UpdateShaderValue(ContrastProperty);
			this.UpdateShaderValue(SaturationProperty);
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		public double Brightness {
			get {
				return ((double)(this.GetValue(BrightnessProperty)));
			}
			set {
				this.SetValue(BrightnessProperty, value);
			}
		}
		/// <summary>Contrast of the base image.</summary>
		public double Contrast {
			get {
				return ((double)(this.GetValue(ContrastProperty)));
			}
			set {
				this.SetValue(ContrastProperty, value);
			}
		}
		/// <summary>Saturation of the bloom image.</summary>
		public double Saturation {
			get {
				return ((double)(this.GetValue(SaturationProperty)));
			}
			set {
				this.SetValue(SaturationProperty, value);
			}
		}
	}
}
