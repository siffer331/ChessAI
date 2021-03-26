using Godot;
using System;

public static class Pieces {
	
	public const int None = 0;
	public const int King = 1;
	public const int Queen = 2;
	public const int Rook = 5;
	public const int Bishop = 3;
	public const int Knight = 4;
	public const int Pawn = 6;
	
	public static int[] values = {0,128<<6, 900, 300, 300, 500, 100};
	
	public const int White = 0;
	public const int Black = 8;
	
	static Move[] king = {
		new Move(new Vector2(1,1), true, false, false),
		new Move(new Vector2(1,-1), true, false, false),
		new Move(new Vector2(-1,1), true, false, false),
		new Move(new Vector2(-1,-1), true, false, false),
		new Move(new Vector2(1,0), true, false, false),
		new Move(new Vector2(-1,0), true, false, false),
		new Move(new Vector2(0,1), true, false, false),
		new Move(new Vector2(0,-1), true, false, false)
	};
	
	static Move[] queen = {
		new Move(new Vector2(1,1), true, true, false),
		new Move(new Vector2(1,-1), true, true, false),
		new Move(new Vector2(-1,1), true, true, false),
		new Move(new Vector2(-1,-1), true, true, false),
		new Move(new Vector2(1,0), true, true, false),
		new Move(new Vector2(-1,0), true, true, false),
		new Move(new Vector2(0,1), true, true, false),
		new Move(new Vector2(0,-1), true, true, false)
	};
	
	static Move[] rook = {
		new Move(new Vector2(1,0), true, true, false),
		new Move(new Vector2(-1,0), true, true, false),
		new Move(new Vector2(0,1), true, true, false),
		new Move(new Vector2(0,-1), true, true, false)
	};
	
	static Move[] bishop = {
		new Move(new Vector2(1,1), true, true, false),
		new Move(new Vector2(1,-1), true, true, false),
		new Move(new Vector2(-1,1), true, true, false),
		new Move(new Vector2(-1,-1), true, true, false)
	};
	
	static Move[] knight = {
		new Move(new Vector2(1,2), true, false, false),
		new Move(new Vector2(1,-2), true, false, false),
		new Move(new Vector2(-1,2), true, false, false),
		new Move(new Vector2(-1,-2), true, false, false),
		new Move(new Vector2(2,1), true, false, false),
		new Move(new Vector2(2,-1), true, false, false),
		new Move(new Vector2(-2,1), true, false, false),
		new Move(new Vector2(-2,-1), true, false, false)
	};
	
	static Move[] pawn = {
		new Move(new Vector2(0,1), false, false, false),
		new Move(new Vector2(1,1), true, false, true),
		new Move(new Vector2(-1,1), true, false, true)
	};
	
	public static Move[][] moves = {king, queen, bishop, knight, rook, pawn};
	
}
