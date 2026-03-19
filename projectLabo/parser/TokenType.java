package projectLabo.parser;

public enum TokenType {
	// used internally by the tokenizer, should never be accessed by the parser
	KEYWORD, SKIP, SYMBOL,
	// non singleton categories
	IDENT, NUM,
	// end-of-file
	EOF,
	// symbols
	AND, ASSIGN, CLOSE_BLOCK, CLOSE_PAR, EQ, MINUS, NOT, OPEN_BLOCK, OPEN_PAR, PAIR_OP, PLUS, STMT_SEP, TIMES,
	// keywords
	ASSERT, BOOL, ELSE, FST, IF, PRINT, SND, VAR,

	// final project

	// symbols
	DIFF, EXP_SEP, SIZE, UNION,
	// keywords
	FOR, IN, WHILE,
}
