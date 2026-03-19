package projectLabo.parser.ast;

import static java.util.Objects.requireNonNull;

import projectLabo.visitors.Visitor;

public class LangProg implements Prog {
	private final StmtSeq stmtSeq;

	public LangProg(StmtSeq stmtSeq) {
		this.stmtSeq = requireNonNull(stmtSeq);
	}

	@Override
	public String toString() {
		return String.format("%s(%s)", getClass().getSimpleName(), stmtSeq);
	}
	
	@Override
	public <T> T accept(Visitor<T> visitor) {
		return visitor.visitLangProg(stmtSeq);
	}
}
