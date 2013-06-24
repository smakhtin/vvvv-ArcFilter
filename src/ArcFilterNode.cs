using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace ArcFilter
{
	public class ArcFilterInstance
	{
		private Vector3D FTargetPos;

		public double FilterTime { get; set; }
		public Vector3D SystemCenter { get; set; }
		public double Radius { get; set; }

		public Vector3D Position { get; private set; }
		private double FEndTime;

		public ArcFilterInstance()
		{
			Position = new Vector3D();
		}

		public void Reset()
		{
			Position = FTargetPos;
		}

		public void Update(Vector3D targetPos, double frameTime)
		{
			if (!FTargetPos.Equals(targetPos))
			{
				FTargetPos = targetPos;
				FEndTime = frameTime;
			}

			
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

		[Input("Radius")] 
		private ISpread<double> FRadiusIn;

		[Input("Reset", IsBang = true)]
		private ISpread<bool> FResetIn;

		[Output("Position Out")] 
		private ISpread<Vector3D> FPositionOut;

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
			}	
		}
	}
}
