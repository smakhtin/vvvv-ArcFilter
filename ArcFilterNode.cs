using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace vvvv.Nodes.ArcFilter
{
	public class ArcFilterInstance
	{
		public double FilterTime { get; set; }
		public Vector3D SystemCenter { get; set; }
		public double Radius { get; set; }

		public Vector3D Position { get; private set; }
		public double Angle { get; private set; }

		public EasingDirection EasingDirection { get; set; }
		public EasingType EasingType { get; set; }

		public ArcFilterInstance()
		{
			Position = new Vector3D();
			FillEasings();
		}

		private Vector3D FStartPosition;
		private Vector3D FTargetPos;
		private double FStartTime;

		private readonly Dictionary<EasingDirection, Dictionary<EasingType, Func<double, double>>> FEasingByDirection =
			new Dictionary<EasingDirection, Dictionary<EasingType, Func<double, double>>>();

		private void FillEasings()
		{
			FEasingByDirection.Add(EasingDirection.In, new Dictionary<EasingType, Func<double, double>>());
			FEasingByDirection.Add(EasingDirection.InOut, new Dictionary<EasingType, Func<double, double>>());
			FEasingByDirection.Add(EasingDirection.Out, new Dictionary<EasingType, Func<double, double>>());
			FEasingByDirection.Add(EasingDirection.OutIn, new Dictionary<EasingType, Func<double, double>>());

			FEasingByDirection[EasingDirection.In].Add(EasingType.Back, Tweener.BackEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Bounce, Tweener.BounceEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Circular, Tweener.CircularEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Cubic, Tweener.CubicEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Elastic, Tweener.ElasticEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Exponential, Tweener.ExponentialEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Quad, Tweener.QuadEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Quartic, Tweener.QuarticEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Quintic, Tweener.QuinticEaseIn);
			FEasingByDirection[EasingDirection.In].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseIn);

			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Back, Tweener.BackEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Bounce, Tweener.BounceEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Circular, Tweener.CircularEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Cubic, Tweener.CubicEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Elastic, Tweener.ElasticEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Exponential, Tweener.ExponentialEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Quad, Tweener.QuadEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Quartic, Tweener.QuarticEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Quintic, Tweener.QuinticEaseInOut);
			FEasingByDirection[EasingDirection.InOut].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseInOut);

			FEasingByDirection[EasingDirection.Out].Add(EasingType.Back, Tweener.BackEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Bounce, Tweener.BounceEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Circular, Tweener.CircularEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Cubic, Tweener.CubicEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Elastic, Tweener.ElasticEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Exponential, Tweener.ExponentialEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Quad, Tweener.QuadEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Quartic, Tweener.QuarticEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Quintic, Tweener.QuinticEaseOut);
			FEasingByDirection[EasingDirection.Out].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseOut);

			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Back, Tweener.BackEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Bounce, Tweener.BounceEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Circular, Tweener.CircularEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Cubic, Tweener.CubicEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Elastic, Tweener.ElasticEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Exponential, Tweener.ExponentialEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Quad, Tweener.QuadEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Quartic, Tweener.QuarticEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Quintic, Tweener.QuinticEaseOutIn);
			FEasingByDirection[EasingDirection.OutIn].Add(EasingType.Sinusoidal, Tweener.SinusoidalEaseOutIn);
		}

		public void Reset()
		{
			Position = FTargetPos;
			FStartPosition = FTargetPos;
		}

		public void Update(Vector3D targetPos, double frameTime)
		{
			if (FTargetPos != targetPos)
			{
				FTargetPos = targetPos;
				FStartTime = frameTime;
				FStartPosition = Position;
			}

			var centerStart = FStartPosition - SystemCenter;
			var centerEnd = FTargetPos - SystemCenter;

			var centerStartEnd = centerEnd - centerStart;

			var midPoint = centerStartEnd * 0.5 + centerStart;

			var planeNormal = centerStart.CrossRH(centerEnd);
			planeNormal = ~planeNormal;

			var midStart = FStartPosition - midPoint;

			var perpendicular = midStart.CrossRH(planeNormal);

			perpendicular *= Radius;

			//translate perpendicular to center
			perpendicular += midPoint;

			var startPerp = FStartPosition - perpendicular;
			var endPerp = FTargetPos - perpendicular;

			var yMult = startPerp.CrossRH(endPerp);
			yMult = startPerp.CrossRH(yMult);
			yMult = ~yMult;

			var xMult = ~startPerp;

			var length = !startPerp;

			var divider = !startPerp*!endPerp;
			if (divider == 0) divider = 1;

			var targetAngle = (startPerp | endPerp)/(divider);
			targetAngle = Math.Max(-1, Math.Min(targetAngle, 1));
			targetAngle = Math.Acos(targetAngle);

			var mult = Math.Min((frameTime - FStartTime)/FilterTime, 1.0);
			if (EasingDirection != EasingDirection.None)
			{
				mult = FEasingByDirection[EasingDirection][EasingType](mult);
			}
			Angle = targetAngle*mult;

			var aproxTargetPos = FindPos(length, targetAngle, xMult, yMult, perpendicular);
			if (aproxTargetPos != FTargetPos) Angle *= -1;

			Position = FindPos(length, Angle, xMult, yMult, perpendicular);
		}

		private Vector3D FindPos(double length, double angle, Vector3D xMult, Vector3D yMult, Vector3D perpendicular)
		{
			var x = length * Math.Cos(angle) * xMult;
			var y = length * Math.Sin(angle) * yMult;

			return x + y + perpendicular;
		}
	}

	[PluginInfo(Name = "ArcFilter", Author = "alg", AutoEvaluate = true)]
	public class ArcFilterNode : IPluginEvaluate
	{
		[Input("Go To Position")]
		private ISpread<Vector3D> FGoToPositionIn;

		[Input("FilterTime", DefaultValue = 1)] 
		private ISpread<double> FFilterTimeIn;

		[Input("System Center")] 
		private ISpread<Vector3D> FSystemCenterIn;

		[Input("Radius", DefaultValue = 1)] 
		private ISpread<double> FRadiusIn;

		[Input("Reset", IsBang = true)]
		private ISpread<bool> FResetIn;

		[Input("Easing Direction", DefaultEnumEntry = "None")] 
		private ISpread<EasingDirection> FEasingDirectionIn;

		[Input("Easing Type", DefaultEnumEntry = "Back")]
		private ISpread<EasingType> FEasingTypeIn;
		
		[Output("Position Out")] 
		private ISpread<Vector3D> FPositionOut;

		[Output("Angle")]
		private ISpread<double> FAngleOut;

		

		[Import]
		IHDEHost FHost;
		private readonly List<ArcFilterInstance> FInstances = new List<ArcFilterInstance>();

		public void Evaluate(int spreadMax)
		{

			FPositionOut.SliceCount = FAngleOut.SliceCount = spreadMax;

			while (FInstances.Count > spreadMax)
			{
				FInstances.RemoveAt(FInstances.Count - 1);
			}

			for (var i = 0; i < spreadMax; i++)
			{
				if(FInstances.Count <  spreadMax) 
					FInstances.Add(new ArcFilterInstance());

				FInstances[i].FilterTime = FFilterTimeIn[i];
				FInstances[i].SystemCenter = FSystemCenterIn[i];
				FInstances[i].Radius = FRadiusIn[i];
				FInstances[i].EasingDirection = FEasingDirectionIn[i];
				FInstances[i].EasingType = FEasingTypeIn[i];

				FInstances[i].Update(FGoToPositionIn[i], FHost.FrameTime);

				if (FResetIn[i]) FInstances[i].Reset();

				FPositionOut[i] = FInstances[i].Position;
				FAngleOut[i] = FInstances[i].Angle;
			}	
		}
	}

	public enum EasingDirection
	{
		None,
		In,
		InOut,
		Out,
		OutIn
	}

	public enum EasingType
	{
		Back,
		Bounce,
		Circular,
		Cubic,
		Elastic,
		Exponential,
		Quad,
		Quartic,
		Quintic,
		Sinusoidal
	}
}
