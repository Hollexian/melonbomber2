using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Internal;
using Sandbox.Stuff;

namespace MyGame;
public partial class MelonBomb : Prop
{
	public TimeUntil Countdown = 3F;
	public Pawn Bomber { get; set; }
	bool Colliding = false;
	Explosion exploder = new Explosion();
	float distance;
	public IList<Vector2> Directions = new List<Vector2>
	{
		Vector2.Up, Vector2.Left, Vector2.Right, Vector2.Down
	};

	public int Power;
	public MelonBomb()
	{

	}
	public MelonBomb(Pawn bomber, int power, Vector3 pos )
	{
		Bomber = bomber;
		Power = power;
		Position = pos;
		Health = 1000;
		Transmit = TransmitType.Always;
	}
	public override void Spawn()
	{
		base.Spawn();

		//Log.Info( "Position is " + Position );
		exploder.Bomber = Bomber;

		Model = Cloud.Model( "caro.melon" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "melon" );
		SetCollisions( false );
		if ( Game.IsServer )
		{
		}


	}
	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );

		Log.Info( "collided with " + eventData.Other.Entity );
	}
	protected override void OnDestroy()
	{
		
		Log.Info( this + " bomb destroyed by " + this.LastAttacker );
		if ( Game.IsServer )
		{
			Bomber.plantedbombs.Remove( this );
		}
		base.OnDestroy();
	}

	[GameEvent.Tick]
	public void Tick()
	{
		if ( Countdown < 0)
		{
			if (Game.IsServer)
			{
				exploder.Position = Position;
				exploder.Start( this );
				Vector2 gridpos = new Vector2( (int) Position.x / 64, (int) Position.y / 64 );
				Log.Info( "og explosion at  " + gridpos);
				foreach (Vector2 dir in Directions)
					{
					var PossPos = gridpos;
					for (int i = 0; i < Power; i++)
					{
						PossPos += dir;
						if ( MyGame.OpenCoords.Contains( PossPos ) )
						{
							//Log.Info( "Exploding at " + MyGame.GridToCoords( PossPos ) );
							Explosion ChildExplode = new Explosion()
							{
								Position = MyGame.GridToCoords( PossPos ),
								Bomber = Bomber
							};
							if ( FindInSphere( MyGame.GridToCoords( PossPos ), 2 ).OfType<Crate>().Any() )
							{
								ChildExplode.Start( this );
								break;
							}
							ChildExplode.Start( this );
						}
						else
						{
							break;
						}
					}

					}


			}
							
			DeleteInput();
			Log.Info( "removing." );
		}
		if ( Bomber != null && !Colliding )
		{
			//Log.Info( "distance check" );
			distance = Position.Distance( Bomber.Position );
			if ( distance > 35 )
			{
				Colliding = true;
				SetCollisions( true );
				SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

				//Log.Info( "out of range" );
			}
		}
	}

}
