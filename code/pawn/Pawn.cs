using melonbomb;
using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MyGame;

public enum ModifierType { Set, Add, Mult }

public class ModifierData
{
	public float value;
	public ModifierType type;

	public ModifierData( float _value, ModifierType _type )
	{
		value = _value;
		type = _type;
	}
}
public enum PlayerStat
{
	Speed, Power, BombNum, CanKick
}
public partial class Pawn : AnimatedEntity
{
	[ClientInput]
	public Vector3 InputDirection { get; set; }
	Sound Music;
	int musicindex = -1;
	int MusicPlayingCoolDown = 5;
	Random ran = new Random();
	private List<String> possiblesongs = new List<String>
	{
		"sounds/bomberman hero dessert hd.sound",
		"sounds/bomberman hero redial hd.sound",
		"sounds/bomberman hero zip hd.sound",
		"sounds/bomberman hero mimesis hd.sound"
	};
	[ClientInput]
	public Angles ViewAngles { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}

	[BindComponent] public PawnController Controller { get; }
	[BindComponent] public PawnAnimator Animator { get; }
	[BindComponent] public PawnCamera Camera { get; }
	//public TimeSince CurrentTime { get; private set; }


	[Net] public IList<MelonBomb> plantedbombs { get; set; }
	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	// STATS
	[Net] public IDictionary<PlayerStat, float> Stats { get; private set; }

	//private List<Status> _statusesToRemove = new List<Status>();


	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		InitStats();
		EnableShadowInFirstPerson = true;
		Health = 100;
		Tags.Add( "kicker" );
	}
	public void InitStats()
	{
		Stats.Clear();

		Stats.Add( KeyValuePair.Create( PlayerStat.Speed, 0F ) );
		Stats.Add( KeyValuePair.Create( PlayerStat.Power, 1F ) );
		Stats.Add( KeyValuePair.Create( PlayerStat.CanKick, 0F ) );
		Stats.Add( KeyValuePair.Create( PlayerStat.BombNum, 1F ) );


	}
	public override async void OnKilled()
	{
		base.OnKilled();
		PlaySound( "sounds/fixedcrowdaww.sound" );

	}
	public void Respawn()
	{
		Components.Create<PawnController>();
		Components.Create<PawnAnimator>();
		Components.Create<PawnCamera>();
		
	}
	void UpdateMusic()
	{
		if ( !Music.IsPlaying && MusicPlayingCoolDown > 4 )
		{
			int nextup = ran.Next( possiblesongs.Count );
			Log.Info( "Tryna play " + possiblesongs[nextup] );
			if ( musicindex == nextup )
				return;
			musicindex = nextup;
			Log.Info( "Playing " + possiblesongs[musicindex] );
			Music = PlaySound( possiblesongs[musicindex] );
			MusicPlayingCoolDown = 0;
		}
		else if ( !Music.IsPlaying )
			++MusicPlayingCoolDown;
	}
	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public override async void Simulate( IClient cl )
	{
		if ( Input.Pressed( "jump" )) {
			Vector3 possPos = new Vector3( (float)(Math.Round( Position.x / 64 ) * 64), (float)(Math.Round( Position.y / 64 ) * 64), Position.z + 5);
		
			//Log.Info( "Attempting to find stuff at " + possPos );
			Log.Info( FindInSphere( possPos, 20 ).Any());
			//Log.Info( "solidcoll " + EnableSolidCollisions );
			if ( FindInSphere( possPos, 20 ).OfType<MelonBomb>().Any() || plantedbombs.Count >= Stats[PlayerStat.BombNum] )
				return;
			Log.Info( "Creating bomb" );
			MelonBomb mBomb = new MelonBomb( this, (int)Stats[PlayerStat.Power], possPos );
			plantedbombs.Add( mBomb );
		
		}
		if (Game.IsClient)
			UpdateMusic();
		SimulateRotation();
		Controller?.Simulate( cl );
		Animator?.Simulate();
	}

	public void AdjustBaseStat( PlayerStat statType, float amount)
	{
		if ( !Stats.ContainsKey( statType ) )
			Stats.Add( statType, 0 );

		Stats[statType] += amount;
	}
	public override void BuildInput()
	{
		Camera?.BuildInput();
	}

	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();
		Camera?.Update();
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		var idealRotation = ViewAngles.ToRotation();
		EyeRotation = Rotation.Slerp( Rotation, idealRotation, Time.Delta * 10f );
		Rotation = EyeRotation;
	}

}
