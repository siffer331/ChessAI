using Godot;
using System;
using System.Collections.Generic;



public class AI
{
	
	const int positiveInfinity = 9999999;
	const int negativeInfinity = -positiveInfinity;
	Board board;
	public const int immediateMateScore = 100000;
	public BoardMove bestMove = new BoardMove(65,0,0,0,0);
	
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
		Search(depth, 0, negativeInfinity, positiveInfinity);
		GD.Print(searched);
		return bestMove;
	}
	
	public int Search(int depth, int plyFromRoot, int alpha, int beta) {
		searched++;
		if(depth == 0) return -SearchCaptures(alpha, beta);
		if(plyFromRoot != 0) {
			//position cant be worse than mate
			alpha = Math.Max(alpha, -immediateMateScore + plyFromRoot);
			beta = Math.Min(beta, immediateMateScore - plyFromRoot);
			if(alpha >= beta) return alpha;
		}
		List<BoardMove> moves = board.GetAllMoves();
		//OrderMoves(moves);
		foreach(BoardMove move in moves) {
			board.MakeMove(move);
			int val = -Search(depth-1, plyFromRoot+1, -beta, -alpha);
			board.UndoMove(move);
			if(alpha < val && plyFromRoot == 0) bestMove = move;
			alpha = Math.Max(alpha, val);
			//if(val >= beta) return alpha;
		}
		if(moves.Count == 0) {
			//encourage quiker mates
			if(board.IsCheck()) return plyFromRoot - immediateMateScore;
			return 0;
		}
		return alpha;
	}
	
	public int SearchCaptures(int alpha, int beta) {
		searched++;
		int evaluation = Evaluation.Evaluate(board);
		if(evaluation >= alpha) return evaluation;
		List<BoardMove> moves = board.GetAllMoves(true);
		OrderMoves(moves);
		foreach(BoardMove move in moves) {
			board.MakeMove(move);
			int val = -SearchCaptures(-beta, -alpha);
			board.UndoMove(move);
			if(val >= beta) return beta;
			alpha = Math.Max(alpha, val);
		}
		return alpha;
	}
}
