using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyGame;
using Sandbox;
using Sandbox.Internal;
using Sandbox.ModelEditor.Nodes;
using melonbomb;

namespace MyGame;

public partial class Crate : Prop
{
	Random ran = new Random();
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		//Log.Info( "Position is " + Position );
		Model = Cloud.Model( "facepunch.wooden_crate" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Health = 10;
		Scale = 2;

	}
	protected override void OnDestroy()
	{

		base.OnDestroy();
		if ( Game.IsClosing || Game.IsClient )
			return;
		Kick test = new Kick()
		{
			Position = new Vector3( Position.x, Position.y, Position.z + 20 )
		};
	}

	/*	public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );

			Log.Info( "TAKEN DAMAGE AT " + info.Position );
		}*/
}
