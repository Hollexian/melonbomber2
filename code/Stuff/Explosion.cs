using MyGame;
using Sandbox.Services;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Stuff
{
	internal class Explosion : ExplosionEntity
	{
		public Pawn Bomber;
		public override void Spawn()
		{
			base.Spawn();
			Transmit = TransmitType.Always;
			Radius = 40;
			Damage = 70;
		}


		public void Start( Entity activator )
		{
			foreach (Pawn player in Pawn.All.OfType<Pawn>())
				if (player.Position.Distance(Position) < 40)
				{
					Log.Info( player + " is killed" );
					player.TakeDamage( DamageInfo.FromExplosion( Position, ForceScale, 100 ).WithAttacker( Bomber ) );
				}
				
			Explode( activator );
		}
	}

}
