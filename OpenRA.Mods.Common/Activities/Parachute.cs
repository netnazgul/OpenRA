#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Activities
{
	public class Parachute : Activity
	{
		readonly IPositionable pos;
		readonly WVec fallVector;
		readonly Actor ignore;

		int groundLevel;

		public Parachute(Actor self, Actor ignoreActor = null)
		{
			pos = self.TraitOrDefault<IPositionable>();
			ignore = ignoreActor;

			fallVector = new WVec(0, 0, self.Info.TraitInfo<ParachutableInfo>().FallRate);
			IsInterruptible = false;
		}

		protected override void OnFirstRun(Actor self)
		{
			groundLevel = self.World.Map.CenterOfCell(self.Location).Z;
			foreach (var np in self.TraitsImplementing<INotifyParachute>())
				np.OnParachute(self);
		}

		public override Activity Tick(Actor self)
		{
			var nextPosition = self.CenterPosition - fallVector;
			if (nextPosition.Z < groundLevel)
				return NextActivity;

			pos.SetVisualPosition(self, nextPosition);

			return this;
		}

		protected override void OnLastRun(Actor self)
		{
			var centerPosition = self.CenterPosition;
			pos.SetPosition(self, centerPosition - new WVec(0, 0, groundLevel - centerPosition.Z));

			foreach (var np in self.TraitsImplementing<INotifyParachute>())
				np.OnLanded(self, ignore);
		}
	}
}
