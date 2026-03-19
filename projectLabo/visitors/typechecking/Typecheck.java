package projectLabo.visitors.typechecking;

import static projectLabo.visitors.typechecking.AtomicType.*;

import projectLabo.environments.EnvironmentException;
import projectLabo.parser.ast.Block;
import projectLabo.parser.ast.Exp;
import projectLabo.parser.ast.Stmt;
import projectLabo.parser.ast.StmtSeq;
import projectLabo.parser.ast.Variable;
import projectLabo.visitors.Visitor;

public class Typecheck implements Visitor<Type> {

	private final StaticEnv env = new StaticEnv();

	// useful to typecheck binary operations where operands must have the same type
	private void checkBinOp(Exp left, Exp right, Type type) {
		type.checkEqual(left.accept(this));
		type.checkEqual(right.accept(this));
	}

	// static semantics for programs; no value returned by the visitor

	@Override
	public Type visitLangProg(StmtSeq stmtSeq) {
		try {
			stmtSeq.accept(this);
		} catch (EnvironmentException e) { // undeclared variable
			throw new TypecheckerException(e);
		}
		return null;
	}

	// static semantics for sequences of statements
	// no value returned by the visitor

	@Override
	public Type visitEmptyStmtSeq() {
		return null;
	}

	@Override
	public Type visitNonEmptyStmtSeq(Stmt first, StmtSeq rest) {
		first.accept(this);
		rest.accept(this);
		return null;
	}
	
	// static semantics for statements; no value returned by the visitor

	@Override
	public Type visitAssertStmt(Exp exp) {
		BOOL.checkEqual(exp.accept(this));
		return null;
	}

	@Override
	public Type visitAssignStmt(Variable var, Exp exp) {
		var found = env.lookup(var);
		found.checkEqual(exp.accept(this));
		return null;
	}

	@Override
	public Type visitIfStmt(Exp exp, Block thenBlock, Block elseBlock) {
		BOOL.checkEqual(exp.accept(this));
		thenBlock.accept(this);
		if (elseBlock != null)
			elseBlock.accept(this);
		return null;
	}
	
	@Override
	public Type visitPrintStmt(Exp exp) {
		exp.accept(this);
		return null;
	}

	@Override
	public Type visitVarStmt(Variable var, Exp exp) {
		env.dec(var, exp.accept(this));
		return null;
	}

	@Override
	public Type visitBlock(StmtSeq stmtSeq) {
		env.enterLevel();
		stmtSeq.accept(this);
		env.exitLevel();
		return null;
	}

	// static semantics of expressions; a type is returned by the visitor

	@Override
	public AtomicType visitAdd(Exp left, Exp right) {
		checkBinOp(left, right, INT);
		return INT;
	}

	@Override
	public AtomicType visitAnd(Exp left, Exp right) {
		checkBinOp(left, right, BOOL);
		return BOOL;
	}

	@Override
	public AtomicType visitBoolLiteral(boolean value) {
		return BOOL;
	}

	@Override
	public AtomicType visitEq(Exp left, Exp right) {
		left.accept(this).checkEqual(right.accept(this));
		return BOOL;
	}

	@Override
	public Type visitFst(Exp exp) {
		return exp.accept(this).toPairType().fstType();
	}

	@Override
	public AtomicType visitIntLiteral(int value) {
		return INT;
	}

	@Override
	public AtomicType visitMinus(Exp exp) {
		INT.checkEqual(exp.accept(this));
		return INT;
	}
	
	@Override
	public AtomicType visitMul(Exp left, Exp right) {
		checkBinOp(left, right, INT);
		return INT;
	}

	@Override
	public AtomicType visitNot(Exp exp) {
		BOOL.checkEqual(exp.accept(this));
		return BOOL;
	}

	@Override
	public PairType visitPairLit(Exp left, Exp right) {
		return new PairType(left.accept(this), right.accept(this));
	}

	@Override
	public Type visitSnd(Exp exp) {
		return exp.accept(this).toPairType().sndType();
	}
	
	@Override
	public Type visitVariable(Variable var) {
		return env.lookup(var);
	}

	// FINAL PROJECT
	@Override
	public Type visitDiff(Exp left, Exp right) {
		Type leftType = left.accept(this);
		Type rightType = right.accept(this);

	
		leftType.toSetType(); 
		leftType.checkEqual(rightType);
		
		return leftType;
	}
	
	@Override
	public Type visitWhileStmt(Exp exp, Block block){
		BOOL.checkEqual(exp.accept(this)); 
		block.accept(this);
		return null;
	}

	@Override
    public Type visitIsIn(Exp left, Exp right) {
        Type leftType = left.accept(this);        
        Type rightType = right.accept(this);      

        // Verifico che a destra ci sia un Insieme
        rightType.toSetType(); 

        // Se a sinistra ho un INT, mi aspetto che a destra ci sia un INT set
        SetType expectedSetType = new SetType(leftType);

        // Confronto i due insiemi
        // Questo genera l'errore: Found BOOL set, expected INT set nel test prog02.txt
        expectedSetType.checkEqual(rightType);

        return BOOL;
    }

	@Override
	public Type visitUnion(Exp left, Exp right) { 
		Type leftType = left.accept(this);
		Type rightType = right.accept(this);
		
		leftType.toSetType(); 
		leftType.checkEqual(rightType);

		return leftType;
	}

	@Override
	public Type visitSize(Exp exp) {
		Type exp_type = exp.accept(this);
		exp_type.toSetType();
		return INT;
	}

	@Override
	public SetType visitSetLit(Exp exp) {
		Type exp_Type = exp.accept(this);
		return new SetType(exp_Type);
	}

	@Override
	public SetType visitSetEnum(Variable var, Exp setExp, Exp elemExp) {

		// prendiamo il tipo di setEXP
		Type setType = setExp.accept(this);
		SetType realSetType = setType.toSetType();		// verifica che sia un Set 
		Type elemType = realSetType.elType();			// tipo insieme

		// for
		// controllo se l'operazione sia dello stesso tipo della variabile su cui facciamo l'operazione

		env.enterLevel();	// alloca memoria per salvare variabili in locale (siccome siamo in un for, deve vivere dentro al for)
		env.dec(var, elemType); // creo nella tabella symbol table una variabile di tipo elemType

		// Tipo dell’espressione interna
		Type resultElemType = elemExp.accept(this);
		env.exitLevel();
		// Il risultato è un insieme di quel tipo
		return new SetType(resultElemType);
	}

}

