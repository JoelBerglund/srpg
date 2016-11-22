﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	public abstract class OverTime { }

	public class DamageOverTime : OverTime {

		protected double BaseDmg;	//per 100 time units
		public StatSet Damages = new StatSet();


		public DamageOverTime(StatSet ss, double BaseDmg, StatType DmgType) {

			var asdfg = new StatSet();
			asdfg.AddSubSet(ss);
			new AdditionMod(DmgType, BaseDmg).Affect(asdfg);

			//snapshot
			foreach (var dmgt in StatTypeStuff.DamageTypes.Select(asgf => asgf | StatType.DamageOverTime)) {
				new AdditionMod(dmgt, asdfg[dmgt]).Affect(Damages);
				var pen = dmgt.AsPenetration();
				new AdditionMod(pen, asdfg[pen]).Affect(Damages);
			}
		}
	}

	public class OverTimeApplier : ITurnHaver {
		protected List<DamageOverTime> DoTs = new List<DamageOverTime>();
		protected Being Target;
		//protected healthregen

		public void Add(DamageOverTime DoT) {
			DoTs.Add(DoT);
		}
		public void Remove(DamageOverTime DoT) {
			DoTs.Remove(DoT);
		}

		private double leftover;
		public double Effect {
			get {
				return DoTs
					.Select(DoT => {
						double r = 0;
						foreach(var DoTT in StatTypeStuff.DamageTypes.Select(asgf => asgf | StatType.DamageOverTime))
							r += DoT.Damages[DoTT] * (1 - (Target[DoTT.AsResistance()].Value - DoT.Damages[DoTT.AsPenetration()]));
						return r;
					})
					.Aggregate(0.0, (a, b) => a + b);
			}
		}

		public OverTimeApplier(Being target) {
			Target = target;
			TurnTracker.Add(this);
		}

		public event EventHandler TurnStarted;

		public event EventHandler TurnFinished;

		public void StartTurn() {
			if (TurnStarted != null) TurnStarted(this, EventArgs.Empty);
			if (TurnFinished != null) TurnFinished(this, EventArgs.Empty);
		}

		public double Speed {
			get {
				return (Effect / 100.0) / (Target.MaxHP / 100.0);
			}
		}

		public double Awaited {
			get { return Target.IsAlive ? (((double)(Target.MaxHP - Target.HP) + leftover)/ (double)Target.MaxHP) *100.0 : 0; }
		}

		public void Await(double time) {
			double dmg = Effect * (time / 100.0) + leftover;
			leftover = dmg - (int)dmg;
			if((int)dmg != 0) Target.TakeRawDamage((int)dmg);
		}
	}
}