using Godot;
using System;

public struct BoardMove {
	public readonly int enPessan;
	public readonly int fromB;
	public readonly int fromP;
	public readonly int toB;
	public readonly int toP;
	public readonly int rookFrom;
	public readonly int rookTo;
	public readonly int pawnDead;
	public readonly bool startPawnMove;
	public int promotion;
	public int score;
	public BoardMove(int a, int b, int c, int d, int e, int f = -1, int g = -1, int h = -1, bool i = false, int j = -1) {
		enPessan = a;
		fromB = b; fromP = c;
		toB = d; toP = e;
		rookFrom = f; rookTo = g;
		pawnDead = h;
		startPawnMove = i;
		promotion = j;
		score = 0;
	}
}

public class Move : Godot.Object
{
	[Export]
	public Vector2 dir;
	public bool kill, exclusive, line;
	
	public static int[] promotions = {Pieces.Queen, Pieces.Bishop, Pieces.Knight, Pieces.Rook}; 
	
	public Move(Vector2 _dir, bool _kill, bool _line, bool _exclusive) {
		dir = _dir;
		kill = _kill;
		line = _line;
		exclusive = _exclusive;
	}
	
	public Move() {
		dir = Vector2.Zero;
		kill = false;
		line = false;
		exclusive = false;
	}
}
