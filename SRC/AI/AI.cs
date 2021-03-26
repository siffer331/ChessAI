using Godot;
using System;
using System.Collections.Generic;


public class AI
{
	
	Board board;
	public AI(Board _board) {
		board = _board;
	}
	
	public int CountMoves(int plies, bool print = false) {
		if(plies == 0) return 1;
		int sum = 0;
		foreach(BoardMove move in board.GetAllMoves()) {
			board.MakeMove(move);
			int s = CountMoves(plies-1);
			if(s == -1) return -1;
			sum += s;
			if(print) GD.Print(Board.ToReadable(move), " ", s);
			board.UndoMove(move);
		}
		return sum;
	}
	
	
	public void OrderMoves(List<BoardMove> moves) {
		for(int i = 0; i < moves.Count; i++) {
			BoardMove move = moves[i];
			int score = 0;
			int movePiece = board.board[move.fromB]&7;
			int capturedPiece = board.board[move.toB]&7;
			//capturing with low value pieces are gooood
			if(capturedPiece != 0) score = (
				10*Pieces.values[capturedPiece] - Pieces.values[capturedPiece]
			);
			//Promoting is gooood
			if(move.promotion != -1) score += Pieces.values[move.promotion];
			//moving a non pawn into a position thretened by a pawn is no good
			if(movePiece != Pieces.Pawn) {
				foreach(int piece in board.GetThreatened(
					move.toB, Pieces.Pawn | (board.turn?0:8)
				)) if((piece&7) == Pieces.Pawn) {
					score -= 350;
					break;
				}
			}
			move.score = score;
			moves[i] = move;
		}
		moves.Sort((x, y) => y.score - x.score);
	}
	
	private int searched = 0;
	
	public BoardMove GetBestMove(int depth) {
		searched = 0;
		BoardMove bestMove = new BoardMove(65,0,0,0,0);
		int bestValue = -2000000000;
		foreach(BoardMove move in board.GetAllMoves()) {
			board.MakeMove(move);
			int val = -Evaluate(depth-1, -bestValue);
			if(val >= bestValue) {
				bestMove = move;
				bestValue = val;
			}
			board.UndoMove(move);
		}
		GD.Print(searched);
		return bestMove;
	}
	
	public int Evaluate(int depth, int alpha) {
		if(depth == 0) {
			searched++;
			return board.Evaluate();
		}
		int best = -2000000000;
		List<BoardMove> moves = board.GetAllMoves();
		OrderMoves(moves);
		foreach(BoardMove move in moves) {
			board.MakeMove(move);
			int val = -Evaluate(depth-1, -best);
			best = Math.Max(best, val);
			board.UndoMove(move);
			if(best > alpha) return best;
		}
		if(moves.Count == 0) {
			if(board.IsCheck()) return best;
			return 0;
		}
		return best;
	}
}
