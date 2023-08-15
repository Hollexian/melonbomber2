using MyGame;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace melonbomb;

public partial class Kick : Powerup
{
	public override void CreateSprite()
	{
		sprite = Particles.Create( "particles/kick_power.vpcf", this );
	}


	public override void PowerupCollect( Pawn receiver )
	{
		base.PowerupCollect( receiver );

		receiver.AdjustBaseStat( PlayerStat.CanKick, 1f );
	}
}
