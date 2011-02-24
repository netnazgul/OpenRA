﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.GameRules;
using OpenRA.Traits;
using OpenRA.Mods.RA.Render;

namespace OpenRA.Mods.RA
{
	public class TakeCoverInfo : ITraitInfo
	{
		public readonly int ProneTime = 100;	/* ticks, =4s */
		public readonly float ProneDamage = .5f;
		public readonly decimal ProneSpeed = .5m;
		public object Create(ActorInitializer init) { return new TakeCover(this); }
	}

	// Infantry prone behavior
	public class TakeCover : ITick, INotifyDamage, IDamageModifier, ISpeedModifier, ISync
	{
		TakeCoverInfo Info;
		[Sync]
		int remainingProneTime = 0;
		
		public TakeCover(TakeCoverInfo info)
		{
			Info = info;
		}

		public bool IsProne { get { return remainingProneTime > 0; } }

		public void Damaged(Actor self, AttackInfo e)
		{
			if (e.Damage > 0 && (e.Warhead == null || !e.Warhead.PreventProne)) /* Don't go prone when healed */
				remainingProneTime = Info.ProneTime;
		}

		public void Tick(Actor self)
		{
			if (IsProne)
				remainingProneTime--;
		}
		
		public float GetDamageModifier(Actor attacker, WarheadInfo warhead )
		{
			return IsProne ? Info.ProneDamage : 1f;
		}

		public decimal GetSpeedModifier()
		{
			return IsProne ? Info.ProneSpeed : 1m;
		}
	}
	
	class RenderInfantryProneInfo : RenderInfantryInfo, ITraitPrerequisite<TakeCoverInfo>
	{
		public override object Create(ActorInitializer init) { return new RenderInfantryProne(init.self, this); }
	}

	class RenderInfantryProne : RenderInfantry
	{
		readonly TakeCover tc;
		bool wasProne;
		
		public RenderInfantryProne(Actor self, RenderInfantryProneInfo info)
			: base(self, info)
		{
			tc = self.Trait<TakeCover>();
		}
		
		protected override string NormalizeInfantrySequence(Actor self, string baseSequence)
		{
			var prefix = tc != null && tc.IsProne ? "prone-" : "";
			
			if (anim.HasSequence(prefix + baseSequence))
				return prefix + baseSequence;
			else
				return baseSequence;
		}
		
		protected override bool AllowIdleAnimation(Actor self)
		{
			return base.AllowIdleAnimation(self) && !tc.IsProne;
		}
		
		public override void Tick (Actor self)
		{
			if (wasProne != tc.IsProne)
				dirty = true;
			
			wasProne = tc.IsProne;
			base.Tick(self);
		}
	}
}
