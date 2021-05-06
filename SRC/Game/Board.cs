using Godot;
using System;
using System.Collections.Generic;

public class Board {
	
	public const String startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w";
	
	public int[] board = new int[64];
	public bool turn = false;
	public int whiteKing;
	public int blackKing;
	public long hash;
	public Dictionary<int,int>[] pieces = {new Dictionary<int,int>(), new Dictionary<int,int>()};
	
	private long[] hashingValues = new long[777];
	private Stack<long> turns = new Stack<long>();
	private HashSet<long> turnSet = new HashSet<long>();
	
	private int[] moved = new int[64];
	private int enPessan = 65;
	
	public static String ToReadable(BoardMove move) {
		String letters = "abcdefgh";
		String numbers = "87654321";
		String piecesS = " kqbnrp";
		String res = "";
		res += letters[move.fromB&7];
		res += numbers[move.fromB>>3];
		res += letters[move.toB&7];
		res += numbers[move.toB>>3];
		if(move.promotion != -1) res += piecesS[move.promotion];
		return res;
	}
	
	//get vector coordinat from integer
	static int ToI(Vector2 v) {
		if(v.x < 0 || v.x >= 8 || v.y < 0 || v.y >= 8) return -1;
		return (int)v.x+((int)v.y<<3);
	}
	
	//get integer coordinat from vector
	static Vector2 ToV(int i) {
		return new Vector2(i&7, i>>3);
	}
	
	void Print() {
		String s = "";
		for(int i = 0; i < 64; i++) s += board[i].ToString() + " ";
		GD.Print(s);
	}
	
	public bool IsCheck() {
		return IsThreatened(turn ? whiteKing : blackKing, turn);
	}
	
	//check if tile is thretened by a color
	public bool IsThreatened(int tile, bool color) {
		foreach(int piece in GetThreatened(tile, Pieces.Rook|(color?0:8))) {
			if((board[piece]&7) == Pieces.Rook || (board[piece]&7) == Pieces.Queen)
				return true;
		}
		foreach(int piece in GetThreatened(tile, Pieces.Bishop|(color?0:8))) {
			if((board[piece]&7) == Pieces.Bishop || (board[piece]&7) == Pieces.Queen)
				return true;
		}
		foreach(int piece in GetThreatened(tile, Pieces.Knight|(color?0:8))) {
			if((board[piece]&7) == Pieces.Knight) return true;
		}
		foreach(int piece in GetThreatened(tile, Pieces.Pawn|(color?0:8))) {
			if((board[piece]&7) == Pieces.Pawn) return true;
		}
		foreach(int piece in GetThreatened(tile, Pieces.King|(color?0:8))) {
			if((board[piece]&7) == Pieces.King) return true;
		}
		return false;
	}
	
	//get tiles a moveset from a tile is threatening
	public List<int> GetThreatened(int tile, int piece) {
		List<int> canHit = new List<int>();
		Vector2 p = ToV(tile);
		int color = piece>>3;
		int forward = color == 0 ? -1: 1;
		piece = (piece&7)-1;
		for(int i = 0; i < Pieces.moves[piece].Length; i++) {
			Move m = Pieces.moves[piece][i];
			Vector2 d = m.dir;
			d.y *= forward;
			for(int j = 1; ToI(p+j*d)>-1 && (j == 1 || m.line); j++) {
				if(
					m.kill &&
					board[ToI(p+j*d)] != 0 &&
					(board[ToI(p+j*d)]>>3) != color
				) canHit.Add(ToI(p+j*d));
				if(board[ToI(p+j*d)] != 0) break;
			}
		}
		return canHit;
	}
	
	//get possible moves from a tile
	public List<BoardMove> GetMoves(int tile, bool onlyKilling = false) {
		List<BoardMove> moves = new List<BoardMove>();
		if(board[tile] == 0 || (((board[tile]>>3) == 1) == turn)) return moves;
		Vector2 p = ToV(tile);
		int piece = (board[tile]&7)-1;
		bool color = (board[tile]>>3) == 0;
		int forward = color ? -1: 1;
		for(int i = 0; i < Pieces.moves[piece].Length; i++) {
			Move m = Pieces.moves[piece][i];
			Vector2 d = m.dir;
			d.y *= forward;
			for(int j = 1; ToI(p+j*d) > -1 && (j == 1 || m.line); j++) {
				int newTile = ToI(p+j*d);
				if(
					(board[newTile] == 0 && !m.exclusive && !onlyKilling) || 
					(
						m.kill && board[newTile] != 0 &&
						(board[newTile]>>3) != (board[tile]>>3)
					) ||
					(newTile == enPessan && m.exclusive)
				) {
					int enPessanMove = -1;
					if(newTile == enPessan && m.exclusive) enPessanMove = enPessan+(turn?8:-8);
					BoardMove move = new BoardMove(
						enPessan,
						tile, board[tile],
						newTile, board[newTile],
						-1, -1, enPessanMove
					);
					MakeMove(move, false);
					long moveHash = hash;
					if(
						!turnSet.Contains(hash) &&
						!IsThreatened(color ? whiteKing: blackKing, color)
					) {
						if(
							piece == Pieces.Pawn-1 &&
							((newTile>>3) == 7 || (newTile>>3) == 0)
						) {
							foreach(int promotion in Move.promotions) {
								move.promotion = promotion;
								moves.Add(move);
							}
						}
						else moves.Add(move);
					};
					UndoMove(move, false);
				}
				if(board[newTile] != 0) break;
			}
		}
		if(onlyKilling) return moves;
		//pawn start move
		if(
			piece+1 == Pieces.Pawn && moved[tile] == 0 &&
			board[tile+(forward<<3)] == 0 && board[tile+(forward<<4)] == 0
		) {
			BoardMove move = new BoardMove(
				enPessan,
				tile, board[tile],
				tile+(forward<<4), board[tile+(forward<<4)],
				-1, -1, -1, true
			);
			MakeMove(move, false);
			long moveHash = hash;
			if(!turnSet.Contains(hash) && !IsThreatened(
				color ? whiteKing: blackKing,
				color
			)) moves.Add(move);
			UndoMove(move, false);
		}
		//castleling
		if(
			(board[tile]&7) == Pieces.King &&
			moved[tile] == 0 &&
			!IsThreatened(tile, color)
		) {
			if(
				moved[tile-4] == 0 && board[tile-1] == 0 &&
				board[tile-2] == 0 && board[tile-3] == 0 &&
				!IsThreatened(tile-1, color) && !IsThreatened(tile-2, color)
			) {
				moves.Add(new BoardMove(
					enPessan,
					tile, board[tile],
					tile-2, board[tile-2],
					tile-4, tile-1
				));
			}
			if(
				moved[tile+3] == 0 && board[tile+1] == 0 && board[tile+2] == 0 &&
				!IsThreatened(tile+1, color) && !IsThreatened(tile+2, color)
			) {
				moves.Add(new BoardMove(
					enPessan,
					tile, board[tile],
					tile+2, board[tile+2],
					tile+3, tile+1
				));
			}
		}
		return moves;
	}
	
	public List<BoardMove> GetAllMoves(bool onlyKilling = false) {
		List<BoardMove> moves = new List<BoardMove>();
		int[] keys = new int[pieces[turn?0:1].Count];
		int i= 0;
		foreach(int key in pieces[turn?0:1].Keys) {
			keys[i] = key;
			i++;
		}
		foreach(int tile in keys) {
			foreach(BoardMove move in GetMoves(tile, onlyKilling)) {
				moves.Add(move);
			}
		}
		return moves;
	}
	
	//get FENstring from board
	private const String PIECES = " KQBNRP  kqbnrp";
	public String ToString() {
		String res = "";
		for(int i = 0; i < 8; i++) {
			int space = 0;
			for(int j = 0; j < 8; j++) {
				if(board[j+(i<<3)] == 0) space++;
				else {
					if(space != 0) {
						res += space.ToString();
						space = 0;
					}
					res += PIECES[board[j+(i<<3)]];
				}
			}
			if(space != 0) res += space.ToString();
			if(i != 7) res += "/";
		}
		res += " ";
		if(turn) res += "w";
		else res += "b";
		return res;
	}
	
	//make move
	public void MakeMove(BoardMove move, bool remember = true) {
		if(move.enPessan == 66) return;
		int color = turn?0:1;
		//move
		Hash(move.toB);
		Hash(move.fromB);
		board[move.toB] = board[move.fromB];
		board[move.fromB] = 0;
		moved[move.fromB]++;
		moved[move.toB]++;
		turn = !turn;
		//castleling
		if(move.rookFrom != -1) {
			Hash(move.rookTo);
			Hash(move.rookFrom);
			board[move.rookTo] = board[move.rookFrom];
			board[move.rookFrom] = 0;
			Hash(move.rookTo);
			Hash(move.rookFrom);
			pieces[color][move.rookTo] = board[move.rookTo];
			pieces[color].Remove(move.rookFrom);
		}
		if(move.toP != 0) pieces[1^color].Remove(move.toB);
		//promotion
		if(move.promotion != -1) board[move.toB] = move.promotion | (color<<3);
		pieces[color][move.toB] = board[move.toB];
		pieces[color].Remove(move.fromB);
		//kill en pessan
		if(move.pawnDead != -1) {
			Hash(move.pawnDead);
			board[move.pawnDead] = 0;
			pieces[1^color].Remove(move.pawnDead);
		}
		if(enPessan < 65) hash ^= hashingValues[enPessan&7];
		//set en pessan
		enPessan = 65;
		if(move.startPawnMove) enPessan = (move.toB+move.fromB)>>1;
		if(enPessan < 65) hash ^= hashingValues[enPessan&7];
		//keep track of kings
		if(board[move.toB] == Pieces.King+8) blackKing = move.toB;
		if(board[move.toB] == Pieces.King) whiteKing = move.toB;
		//GD.Print(ToString());
		hash ^= hashingValues[0];
		Hash(move.toB);
		Hash(move.fromB);
		if(remember) {
			turns.Push(hash);
			turnSet.Add(hash);
		}
	}
	
	public void UndoMove(BoardMove move, bool remember = true) {
		if(remember) turnSet.Remove(turns.Pop());
		int color = turn?1:0;
		if(enPessan < 65) hash ^= hashingValues[enPessan&7];
		enPessan = move.enPessan;
		if(enPessan < 65) hash ^= hashingValues[enPessan&7];
		Hash(move.toB);
		Hash(move.fromB);
		board[move.fromB] = move.fromP;
		board[move.toB] = move.toP;
		Hash(move.toB);
		Hash(move.fromB);
		moved[move.fromB]--;
		moved[move.toB]--;
		pieces[color][move.fromB] = move.fromP;
		pieces[color].Remove(move.toB);
		if(move.toP != 0) pieces[1^color][move.toB] = move.toP;
		if(move.pawnDead != -1) {
			board[move.pawnDead] = Pieces.Pawn|((color^1)<<3);
			pieces[1^color][move.pawnDead] = board[move.pawnDead];
		}
		if(move.rookFrom != -1) {
			board[move.rookFrom] = board[move.rookTo];
			board[move.rookTo] = 0;
			pieces[color][move.rookFrom] = board[move.rookFrom];
			pieces[color].Remove(move.rookTo);
		}
		if(board[move.fromB] == Pieces.King+8) blackKing = move.fromB;
		if(board[move.fromB] == Pieces.King) whiteKing = move.fromB;
		turn = !turn;
		hash ^= hashingValues[0];
	}
	
	//get board after a move
	public Board GetMove(BoardMove move) {
		Board res = new Board(ToString());
		for(int i = 0; i < 64; i++) res.moved[i] = moved[i];
		res.MakeMove(move);
		return res;
	}
	
	//create board from FENstring
	public Board(String fen = startFEN) {
		turn = fen.Split(' ')[1] == "w";
		int tile = 0;
		foreach(char symbol in fen.Split(' ')[0]) {
			if(symbol != '/') {
				if(char.IsDigit(symbol)) tile += (int)char.GetNumericValue(symbol);
				else {
					int color = char.IsUpper(symbol) ? Pieces.White : Pieces.Black;
					int piece = 0;
					switch(char.ToLower(symbol)) {
						case 'r':
							piece = Pieces.Rook;
							moved[tile] = (((tile>>3) == 7-7*(color>>3)) && 
								((tile&7) == 0 || (tile&7) == 7))?0:1;
							break;
						case 'n':
							piece = Pieces.Knight;
							break;
						case 'b':
							piece = Pieces.Bishop;
							break;
						case 'q':
							piece = Pieces.Queen;
							break;
						case 'k':
							piece = Pieces.King;
							moved[tile] = (((tile>>3)==7-7*(color>>3)) &&
								((tile&7)==4))?0:1;
							if(symbol == 'K') whiteKing = tile;
							else blackKing = tile;
							break;
						case 'p':
							piece = Pieces.Pawn;
							moved[tile] = ((tile>>3)==6-5*(color>>3))?0:1;
							break;
					}
					pieces[color>>3][tile] = color | piece;
					board[tile] = color | piece;
					tile++;
				}
			}
		}
		Random random = new Random();
		byte[] bytes = new byte[8];
		for(int i = 0; i < 777; i++) {
			random.NextBytes(bytes);
			hashingValues[i] = BitConverter.ToInt64(bytes,0);
		}
		foreach(int t in pieces[0].Keys) 
			hash ^= hashingValues[9+tile*(pieces[0][t]-1)];
		foreach(int t in pieces[1].Keys) 
			hash ^= hashingValues[9+tile*(pieces[1][t]-3)];
		if(turn) hash ^= hashingValues[0];
		turns.Push(hash);
		turnSet.Add(hash);
	}
	
	void Hash(int tile) {
		if(board[tile] == 0) return;
		if(board[tile] > 8) hash ^= hashingValues[9+tile*(board[tile]-3)];
		else hash ^= hashingValues[9+tile*(board[tile]-1)];
	}
}
