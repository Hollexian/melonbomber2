using Editor;
using MyGame;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace melonbomb;

public partial class Powerup : Entity
{
	public Particles sprite { get; set; }
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		Log.Info( "Creating sprite" );
		CreateSprite();
		//Log.Info( "pos is " + Position );
		Health = 10;
	}

	public virtual void CreateSprite()
	{
		sprite = Particles.Create( "particles/basic_particle.vpcf", this );
	}
	public override void OnKilled()
	{
		base.OnKilled();
		Log.Info( "Destroying sprite" );
		Delete();

	}

	public virtual void PowerupCollect(Pawn receiver)
	{
		Log.Info( "yahoo!" );
		TakeDamage( DamageInfo.FromExplosion( Position, 1, 100 ) );

	}

	[GameEvent.Tick]
	public void Tick()
	{
		if ( Game.IsServer )
		{
			foreach ( Pawn player in Pawn.All.OfType<Pawn>() )
			{
				if ( player.Position.Distance( Position ) < 35 )
					PowerupCollect( player );
			}
		}
	}
}
