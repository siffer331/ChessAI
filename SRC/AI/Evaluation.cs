using Godot;
using System;

public class Evaluation
{
	static int endgameStart = Pieces.values[Pieces.Rook]*2 +
							  Pieces.values[Pieces.Bishop] +
							  Pieces.values[Pieces.Knight];
	
	public static int Evaluate(Board board) {
		int sum = 0;
		int[] pawnLessScores = {0,0};
		foreach(int tile in board.pieces[0].Keys) {
			int val = Pieces.values[board.board[tile]&7];
			sum += val;
			if((board.board[tile]&7) != Pieces.Pawn) pawnLessScores[0] -= val;
		}
		foreach(int tile in board.pieces[1].Keys) {
			int val = Pieces.values[board.board[tile]&7];
			sum -= val;
			if((board.board[tile]&7) != Pieces.Pawn) pawnLessScores[1] += val;
		}
		float endWeight = EndWeight(-pawnLessScores[0]);
		foreach(int tile in board.pieces[0].Keys) {
			sum += ReadTable(tile, board.board[tile]&7, true, endWeight);
		}
		endWeight = EndWeight(pawnLessScores[1]);
		foreach(int tile in board.pieces[1].Keys) {
			sum -= ReadTable(tile, board.board[tile]&7, false, endWeight);
		}
		sum *= board.turn?1:-1;
		return sum;
	}
	
	
	static float EndWeight (int pawnLessScore) {
		return 1 - Math.Min(1, pawnLessScore/endgameStart);
	}
	
	//favor good positioning
	public static int ReadTable(int tile, int piece, bool color, float endWeight) {
		if(!color) tile = ((7-(tile>>3))<<3)|(tile&7);
		switch(piece) {
			case Pieces.Pawn:
				return pawns[tile];
			case Pieces.Knight:
				return knights[tile];
			case Pieces.Bishop:
				return bishops[tile];
			case Pieces.Rook:
				return rooks[tile];
			case Pieces.Queen:
				return queens[tile];
			case Pieces.King:
				return (int)(endWeight*kingEnd[tile]+(1-endWeight)*kingMiddle[tile]);
		}
		return 0;
	}
	
	
	public static readonly int[] pawns = {
		0,  0,  0,  0,  0,  0,  0,  0,
		50, 50, 50, 50, 50, 50, 50, 50,
		10, 10, 20, 30, 30, 20, 10, 10,
		5,  5, 10, 25, 25, 10,  5,  5,
		0,  0,  0, 20, 20,  0,  0,  0,
		5, -5,-10,  0,  0,-10, -5,  5,
		5, 10, 10,-20,-20, 10, 10,  5,
		0,  0,  0,  0,  0,  0,  0,  0
	};
	
	public static readonly int[] knights = {
		-50,-40,-30,-30,-30,-30,-40,-50,
		-40,-20,  0,  0,  0,  0,-20,-40,
		-30,  0, 10, 15, 15, 10,  0,-30,
		-30,  5, 15, 20, 20, 15,  5,-30,
		-30,  0, 15, 20, 20, 15,  0,-30,
		-30,  5, 10, 15, 15, 10,  5,-30,
		-40,-20,  0,  5,  5,  0,-20,-40,
		-50,-40,-30,-30,-30,-30,-40,-50,
	};
	
	public static readonly int[] bishops = {
		-20,-10,-10,-10,-10,-10,-10,-20,
		-10,  0,  0,  0,  0,  0,  0,-10,
		-10,  0,  5, 10, 10,  5,  0,-10,
		-10,  5,  5, 10, 10,  5,  5,-10,
		-10,  0, 10, 10, 10, 10,  0,-10,
		-10, 10, 10, 10, 10, 10, 10,-10,
		-10,  5,  0,  0,  0,  0,  5,-10,
		-20,-10,-10,-10,-10,-10,-10,-20,
	};
	
	public static readonly int[] rooks = {
		0,  0,  0,  0,  0,  0,  0,  0,
		5, 10, 10, 10, 10, 10, 10,  5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		-5,  0,  0,  0,  0,  0,  0, -5,
		0,  0,  0,  5,  5,  0,  0,  0
	};
	
	public static readonly int[] queens = {
		-20,-10,-10, -5, -5,-10,-10,-20,
		-10,  0,  0,  0,  0,  0,  0,-10,
		-10,  0,  5,  5,  5,  5,  0,-10,
		-5,  0,  5,  5,  5,  5,  0, -5,
		 0,  0,  5,  5,  5,  5,  0, -5,
		-10,  5,  5,  5,  5,  5,  0,-10,
		-10,  0,  5,  0,  0,  0,  0,-10,
		-20,-10,-10, -5, -5,-10,-10,-20
	};
	
	public static readonly int[] kingMiddle = {
		-30,-40,-40,-50,-50,-40,-40,-30,
		-30,-40,-40,-50,-50,-40,-40,-30,
		-30,-40,-40,-50,-50,-40,-40,-30,
		-30,-40,-40,-50,-50,-40,-40,-30,
		-20,-30,-30,-40,-40,-30,-30,-20,
		-10,-20,-20,-20,-20,-20,-20,-10,
		20, 20,  0,  0,  0,  0, 20, 20,
		20, 30, 10,  0,  0, 10, 30, 20
	};
	
	public static readonly int[] kingEnd = {
		-50,-40,-30,-20,-20,-30,-40,-50,
		-30,-20,-10,  0,  0,-10,-20,-30,
		-30,-10, 20, 30, 30, 20,-10,-30,
		-30,-10, 30, 40, 40, 30,-10,-30,
		-30,-10, 30, 40, 40, 30,-10,-30,
		-30,-10, 20, 30, 30, 20,-10,-30,
		-30,-30,  0,  0,  0,  0,-30,-30,
		-50,-30,-30,-30,-30,-30,-30,-50
	};
}
