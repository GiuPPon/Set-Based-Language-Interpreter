package projectLabo.parser;

import java.io.IOException;

import projectLabo.parser.ast.*;

import static java.util.Objects.requireNonNull;
import static projectLabo.parser.TokenType.*;


/*
Prog ::= StmtSeq EOF
StmtSeq ::= Stmt (STMT_SEP StmtSeq)?
Stmt ::= VAR? IDENT ASSIGN Exp | PRINT Exp | IF OPEN_PAR Exp CLOSE_PAR Block (ELSE Block)? | ASSERT Exp | WHILE OPEN_PAR Exp CLOSE_PAR Block
Block ::= OPEN_BLOCK StmtSeq CLOSE_BLOCK
Exp ::= And (PAIR_OP And)*
And ::= Eq (AND Eq)*
Eq ::= IsIn (EQ IsIn)*
IsIn ::= DiffUnion (IN DiffUnion)* 
DiffUnion ::= Add (SetOp Add)*     
SetOp ::= DIFF | UNION             
Add ::= Mul (PLUS Mul)*
Mul::= Atom (TIMES Atom)*
Atom ::= FST Atom | SND Atom | MINUS Atom | NOT Atom | SIZE Atom | BOOL | NUM | IDENT | OPEN_PAR Exp CLOSE_PAR | SetAtom
SetAtom ::= OPEN_BLOCK (FOR IDENT IN Exp EXP_SEP)? Exp CLOSE_BLOCK
*/

public class Parser implements ParserInterface {

	private final Tokenizer tokenizer; // the tokenizer used by the parser

	// decorates error message with the corresponding line number
	private String lineErrMsg(String msg) {
		return String.format("on line %s: %s", tokenizer.getLineNumber(), msg);
	}

	/*
	 * checks whether the token type of the currently recognized token matches
	 * 'expected'; if not, it throws a corresponding ParserException
	 */
	private void match(TokenType expected) throws ParserException {
		final var found = tokenizer.tokenType();
		if (found != expected)
			throw new ParserException(
					lineErrMsg(String.format("Expecting %s, found %s('%s')", expected, found, tokenizer.tokenString())));
	}

	/*
	 * checks whether the token type of the currently recognized token matches
	 * 'expected'; if so, it reads the next token, otherwise it throws a
	 * corresponding ParserException
	 */
	private void consume(TokenType expected) throws ParserException {
		match(expected);
		tokenizer.next();
	}

	// throws a ParserException because the current token was not expected
	private <T> T unexpectedTokenError() throws ParserException {
		throw new ParserException(lineErrMsg(
				String.format("Unexpected token %s ('%s')", tokenizer.tokenType(), tokenizer.tokenString())));
	}

	// associates the parser with a corresponding non-null  tokenizer
	public Parser(Tokenizer tokenizer) {
		this.tokenizer = requireNonNull(tokenizer);
	}

	/*
	 * parses a program Prog ::= StmtSeq EOF
	 */
	@Override
	public Prog parseProg() throws ParserException {
		tokenizer.next(); // one look-ahead symbol
		final var prog = new LangProg(parseStmtSeq());
		match(EOF); // last token must have type EOF
		return prog;
	}

	@Override
	public void close() throws IOException {
		if (tokenizer != null)
			tokenizer.close();
	}

	/*
	 * parses a non-empty sequence of statements, binary operator STMT_SEP is right
	 * associative StmtSeq ::= Stmt (STMT_SEP StmtSeq)?
	 */
	private StmtSeq parseStmtSeq() throws ParserException {
		final var stmt = parseStmt();
		StmtSeq stmtSeq;
		if (tokenizer.tokenType() == STMT_SEP) {
			tokenizer.next();
			stmtSeq = parseStmtSeq();
		} else
			stmtSeq = new EmptyStmtSeq();
		return new NonEmptyStmtSeq(stmt, stmtSeq);
	}

	/*
	 * parses a statement
	 * Stmt ::= VAR? IDENT ASSIGN Exp | PRINT Exp | IF OPEN_PAR Exp CLOSE_PAR Block (ELSE Block)? | ASSERT Exp
	 */
	private Stmt parseStmt() throws ParserException {
		return switch (tokenizer.tokenType()) {
		case PRINT -> parsePrintStmt();
		case VAR -> parseVarStmt();
		case IDENT -> parseAssignStmt();
		case IF -> parseIfStmt();
		case ASSERT -> parseAssertStmt();
		case WHILE -> parseWhileStmt();	// final project
		default -> unexpectedTokenError();
		};
	}

	/*
	 * parses the print statement
	 * Stmt ::= PRINT Exp
	 */
	private PrintStmt parsePrintStmt() throws ParserException {
		consume(PRINT); // or tokenizer.next() if the method is only called by parseStmt()
		return new PrintStmt(parseExp());
	}

	/*
	 * parses the var statement
	 * Stmt ::= VAR IDENT ASSIGN Exp
	 */
	private VarStmt parseVarStmt() throws ParserException {
		consume(VAR); // or tokenizer.next() if the method is only called by parseStmt()
		final var var = parseVariable();
		consume(ASSIGN);
		return new VarStmt(var, parseExp());
	}

	/*
	 * parses the assignment statement
	 * Stmt ::= IDENT ASSIGN Exp
	 */
	private AssignStmt parseAssignStmt() throws ParserException {
		final var var = parseVariable();
		consume(ASSIGN);
		return new AssignStmt(var, parseExp());
	}

	/*
	 * parses the if-then-else statement
	 * Stmt ::= IF OPEN_PAR Exp CLOSE_PAR Block (ELSE Block)?
	 */
	private IfStmt parseIfStmt() throws ParserException {
		consume(IF); // or tokenizer.next() since IF has already been recognized
		final var exp = parseRoundPar();
		final var thenBlock = parseBlock();
		if (tokenizer.tokenType() != ELSE)
			return new IfStmt(exp, thenBlock);
		tokenizer.next();
		return new IfStmt(exp, thenBlock, parseBlock());
	}

	/*
	 * parses the assert statement
	 * Stmt ::= ASSERT Exp
	 */
	private AssertStmt parseAssertStmt() throws ParserException {
		consume(ASSERT); // or tokenizer.next() since ASSERT has already been recognized
		return new AssertStmt(parseExp());
	}
	
	
	/* final project
	 * parses the while statment
	 * Stmt ::= WHILE OPEN_PAR Exp CLOSE_PAR Block
	 */
	private WhileStmt parseWhileStmt() throws ParserException {
		consume(WHILE);
		consume(OPEN_PAR);
		final var exp = parseExp();
		consume(CLOSE_PAR);
		final var block = parseBlock();
		return new WhileStmt(exp, block);
	}

	/*
	 * parses a block of statements
	 * Block ::= OPEN_BLOCK StmtSeq CLOSE_BLOCK
	 */
	private Block parseBlock() throws ParserException {
		consume(OPEN_BLOCK);
		final var stmts = parseStmtSeq();
		consume(CLOSE_BLOCK);
		return new Block(stmts);
	}

	/*
	 * parses expressions, starting from the lowest precedence operator PAIR_OP
	 * which is left-associative
	 * Exp ::= And (PAIR_OP And)*
	 */

	private Exp parseExp() throws ParserException {
		var exp = parseAnd();
		while (tokenizer.tokenType() == PAIR_OP) {
			tokenizer.next();
			exp = new PairLit(exp, parseAnd());
		}
		return exp;
	}

	/*
	 * parses expressions, starting from the lowest precedence operator AND which is
	 * left-associative
	 * And ::= Eq (AND Eq)*
	 */
	private Exp parseAnd() throws ParserException {
		var exp = parseEq();
		while (tokenizer.tokenType() == AND) {
			tokenizer.next();
			exp = new And(exp, parseEq());
		}
		return exp;
	}

	/*
	FINAL PROJECT
	 * parses expressions, starting from the lowest precedence operator EQ which is
	 * left-associative
	 * Eq ::= IsIn (EQ IsIn)*
	 */
	private Exp parseEq() throws ParserException {
		var exp = parseIsIn();
		while (tokenizer.tokenType() == EQ) {
			tokenizer.next();
			exp = new Eq(exp, parseIsIn());
		}
		return exp;
	}

	/*
	 * FINAL PROJECT
	 * IsIn ::= DiffUnion (IN DiffUnion)*
	 */
	private Exp parseIsIn() throws ParserException {
		var exp = parseDiffUnion(); // costruisco insieme 
		while (tokenizer.tokenType() == IN) {
			tokenizer.next();
			exp = new IsIn(exp, parseDiffUnion());
		}
		return exp;
	}

	/*
	 * FINAL PROJECT
	 * DiffUnion ::= Add (SetOp Add)*
	 */
	private Exp parseDiffUnion() throws ParserException{
		var exp = parseAdd(); // A
		while(tokenizer.tokenType() == UNION || tokenizer.tokenType() == DIFF) {
			TokenType tipo = tokenizer.tokenType(); // ++ or \
			tokenizer.next(); // B
			if(tipo == UNION) {
				exp = new Union(exp, parseAdd());
			} else {
				exp = new Diff(exp, parseAdd());
			}
		}
		return exp;
	}

	/*
	 * parses expressions, starting from the lowest precedence operator PLUS which
	 * is left-associative
	 * Add ::= Mul (PLUS Mul)*
	 */
	private Exp parseAdd() throws ParserException {
		var exp = parseMul();
		while (tokenizer.tokenType() == PLUS) {
			tokenizer.next();
			exp = new Add(exp, parseMul());
		}
		return exp;
	}

	/*
	 * parses expressions, starting from the lowest precedence operator TIMES which
	 * is left-associative
	 * Mul::= Atom (TIMES Atom)*
	 */
	private Exp parseMul() throws ParserException {
		var exp = parseAtom();
		while (tokenizer.tokenType() == TIMES) {
			tokenizer.next();
			exp = new Mul(exp, parseAtom());
		}
		return exp;
	}

	/*
	 * parses expressions of type Atom
	 * Atom ::= FST Atom | SND Atom | MINUS Atom | NOT Atom | BOOL | NUM | IDENT | OPEN_PAR Exp CLOSE_PAR
	 */
	private Exp parseAtom() throws ParserException {
		return switch (tokenizer.tokenType()) {
		case NUM -> parseNum();
		case IDENT -> parseVariable();
		case MINUS -> parseMinus();
		case OPEN_PAR -> parseRoundPar();
		case BOOL -> parseBoolean();
		case NOT -> parseNot();
		case FST -> parseFst();
		case SND -> parseSnd();
		case SIZE -> parseSize();
		case OPEN_BLOCK -> parseSetAtom();
		default -> unexpectedTokenError();
		};
	}
	
	/*
	 * FINAL PROJECT
	 * Atom ::= SIZE Atom
	 */
	private Size parseSize() throws ParserException{
		consume(SIZE); // controlla se abbiamo #
		final var atom = parseAtom();
		return new Size(atom);
	}

	/*
	 * FINAL PROJECT
	 * SetAtom ::= OPEN_BLOCK (FOR IDENT IN Exp EXP_SEP)? Exp CLOSE_BLOCK
	 */
	private Exp parseSetAtom() throws ParserException{
		consume(OPEN_BLOCK);
		if (tokenizer.tokenType() == FOR) {
			consume(FOR);
			final var variable = parseVariable(); // x
			consume(IN);
			final Exp exp = parseExp();
			consume(EXP_SEP);
			final Exp op = parseExp();
			consume(CLOSE_BLOCK);
			return new SetEnum(variable, exp, op);
		} else {
			final Exp exp = parseExp();
			consume(CLOSE_BLOCK);
			return new SetLit(exp);
		}
	}
	
	// parses number literals
	private IntLiteral parseNum() throws ParserException {
		match(NUM); // can be omitted if the method is only called by parseAtom()
		final var val = tokenizer.intValue();
		tokenizer.next(); // if tokenizer.intValue() does not throw an exception, then NUM has been recognized
		return new IntLiteral(val);
	}

	// parses boolean literals
	private BoolLiteral parseBoolean() throws ParserException {
		match(BOOL); // can be omitted if the method is only called by parseAtom()
		final var val = tokenizer.boolValue();
		tokenizer.next(); // if tokenizer.boolValue() does not throw an exception, then BOOL has been recognized
		return new BoolLiteral(val);
	}

	// parses variable identifiers
	private Variable parseVariable() throws ParserException {
		final var name = tokenizer.tokenString();
		consume(IDENT); // this check is necessary for parsing correctly the VAR statement
		return new Variable(name);
	}

	/*
	 * parses expressions with unary operator MINUS
	 * Atom ::= MINUS Atom
	 */
	private Minus parseMinus() throws ParserException {
		consume(MINUS); // can be omitted if the method is only called by parseAtom()
		return new Minus(parseAtom());
	}

	/*
	 * parses expressions with unary operator FST
	 * Atom ::= FST Atom
	 */
	private Fst parseFst() throws ParserException {
		consume(FST); // can be omitted if the method is only called by parseAtom()
		return new Fst(parseAtom());
	}

	/*
	 * parses expressions with unary operator SND
	 * Atom ::= SND Atom
	 */
	private Snd parseSnd() throws ParserException {
		consume(SND); // can be omitted if the method is only called by parseAtom()
		return new Snd(parseAtom());
	}

	/*
	 * parses expressions with unary operator NOT
	 * Atom ::= NOT Atom
	 */
	private Not parseNot() throws ParserException {
		consume(NOT); // can be omitted if the method is only called by parseAtom()
		return new Not(parseAtom());
	}

	/*
	 * parses expressions delimited by parentheses
	 * Atom ::= OPEN_PAR Exp CLOSE_PAR
	 */

	private Exp parseRoundPar() throws ParserException {
		consume(OPEN_PAR); // this check is necessary for parsing correctly the if-then-else statement
		final var exp = parseExp();
		consume(CLOSE_PAR);
		return exp;
	}

}
