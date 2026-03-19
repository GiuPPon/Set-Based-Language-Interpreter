package projectLabo.visitors;

import projectLabo.parser.ast.Block;
import projectLabo.parser.ast.Exp;
import projectLabo.parser.ast.Stmt;
import projectLabo.parser.ast.StmtSeq;
import projectLabo.parser.ast.Variable;

public interface Visitor<T> {
	
	T visitLangProg(StmtSeq stmtSeq);

	T visitEmptyStmtSeq();

	T visitNonEmptyStmtSeq(Stmt first, StmtSeq rest);
	
	T visitAssignStmt(Variable var, Exp exp);
	
	T visitAssertStmt(Exp exp);

	T visitIfStmt(Exp exp, Block thenBlock, Block elseBlock);

	T visitPrintStmt(Exp exp);

	T visitVarStmt(Variable var, Exp exp);

	T visitBlock(StmtSeq stmtSeq);

	T visitAdd(Exp left, Exp right);
	
	T visitAnd(Exp left, Exp right);

	T visitBoolLiteral(boolean value);

	T visitEq(Exp left, Exp right);

	T visitFst(Exp exp);

	T visitIntLiteral(int value);

	T visitMinus(Exp exp);

	T visitMul(Exp left, Exp right);

	T visitNot(Exp exp);

	T visitPairLit(Exp left, Exp right);

	T visitSnd(Exp exp);

	T visitVariable(Variable var); // only in this case more efficient then T visitVariable(String name)
	
	// final project
	T visitSize(Exp exp);
	T visitDiff(Exp left, Exp right);
	T visitUnion(Exp left, Exp right);
	T visitIsIn(Exp left, Exp right);
	T visitSetLit(Exp exp);
	T visitWhileStmt(Exp exp, Block block);
	T visitSetEnum(Variable var, Exp setExp, Exp elemExp);
	
}
