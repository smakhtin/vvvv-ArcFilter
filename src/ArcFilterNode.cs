using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace vvvv.Nodes.ArcFilter
{
	public class ArcFilterInstance
	{
		private Vector3D FStartPosition;
		private Vector3D FTargetPos;
		private double FStartTime;
		

		public double FilterTime { get; set; }
		public Vector3D SystemCenter { get; set; }
		public double Radius { get; set; }

		public Vector3D Position { get; private set; }
		public double Angle { get; set; }

		public ArcFilterInstance()
		{
			Position = new Vector3D();
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

			var planeNormal = centerStart & centerEnd;
			planeNormal = ~planeNormal;

			var midStart = FStartPosition - midPoint;

			var perpendicular = midStart & planeNormal;

			perpendicular *= Radius;

			//translate perpendicular to center
			perpendicular += midPoint;

			var startPerp = FStartPosition - perpendicular;
			var endPerp = FTargetPos - perpendicular;

			var yMult = startPerp & endPerp;
			yMult = startPerp & yMult;
			yMult = ~yMult;

			var xMult = ~startPerp;

			var length = !startPerp;

			var divider = !startPerp*!endPerp;
			if (divider == 0) divider = 1;

			var targetAngle = (startPerp | endPerp)/(divider);
			targetAngle = Math.Max(-1, Math.Min(targetAngle, 1));
			targetAngle = Math.Acos(targetAngle);

			var mult = Math.Min((frameTime - FStartTime)/FilterTime, 1.0);
			Angle = targetAngle*mult;

			//var aproxTargetPos = FindPos(length, targetAngle, xMult, yMult, perpendicular);
			//if (aproxTargetPos != FTargetPos) Angle *= -1;

			//Position = FStartPosition + (FTargetPos - FStartPosition) * Math.Min((frameTime - FStartTime) / FilterTime, 1.0);

			Position = FindPos(length, Angle, xMult, yMult, perpendicular);
		}

		private Vector3D FindPos(double length, double angle, Vector3D xMult, Vector3D yMult, Vector3D perpendicular)
		{
			var x = length * Math.Cos(Angle) * xMult;
			var y = length * Math.Sin(Angle) * yMult;

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

		[Output("Position Out")] 
		private ISpread<Vector3D> FPositionOut;

		[Output("Angle")]
		private ISpread<double> FAngleOut;

		[Import]
		IHDEHost FHost;
		private List<ArcFilterInstance> FInstances = new List<ArcFilterInstance>();
		
		public void Evaluate(int spreadMax)
		{
			for (var i = 0; i < spreadMax; i++)
			{
				if(FInstances.Count <  spreadMax) 
					FInstances.Add(new ArcFilterInstance());

				FInstances[i].FilterTime = FFilterTimeIn[i];
				FInstances[i].SystemCenter = FSystemCenterIn[i];
				FInstances[i].Radius = FRadiusIn[i];

				FInstances[i].Update(FGoToPositionIn[i], FHost.FrameTime);

				if (FResetIn[i]) FInstances[i].Reset();

				FPositionOut[i] = FInstances[i].Position;
				FAngleOut[i] = FInstances[i].Angle;
			}	
		}
	}
}
