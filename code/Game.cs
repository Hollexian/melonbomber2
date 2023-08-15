
using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace MyGame;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : GameManager
{
	public static IList<Vector2> OpenCoords { get; set; } = new List<Vector2>();

	private HashSet<Vector2> StartingCoords = new HashSet<Vector2>
	{
		new Vector2(0, 0),
		new Vector2(1, 0),
		new Vector2(0, 1),
		new Vector2(0, 14),
		new Vector2(0, 13),
		new Vector2(1, 14),
		new Vector2(14, 0),
		new Vector2(13, 0),
		new Vector2(14, 1),
		new Vector2(14, 14),
		new Vector2(14, 13),
		new Vector2(13, 14)
	};
	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public MyGame()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}

		if ( Game.IsServer )
		{
			for (int y = 0; y < 15; y ++)
			{
				if ( y % 2 == 0 )
				{
					for ( int x = 0; x < 15; x++ )
					{
						OpenCoords.Add( new Vector2( x, y ) );
						//OpenSquares.Add( new Vector3( x * 64, y * 64, 88 ) );
					}
				}
				else
				{
					for ( int x = 0; x < 15; x+= 2 )
					{
						OpenCoords.Add( new Vector2( x, y ) );
						//OpenSquares.Add( new Vector3( x * 64, y * 64, 88 ) );
					}
				}
			}
			Random ran = new Random();
			foreach (Vector2 loc in OpenCoords )
			{
				if (!StartingCoords.Contains( loc ) && ran.Int(10) != 10)
				{
					Crate crate = new Crate()
					{
						Position = GridToCoords( loc)
					};
				}

			}

			
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Pawn();
		client.Pawn = pawn;
		pawn.Respawn();
		pawn.DressFromClient( client );

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}

	public static Vector3 GridToCoords(Vector2 gridcoords )
	{
			return new Vector3( gridcoords.x * 64, gridcoords.y * 64, 88 );
	}
}

