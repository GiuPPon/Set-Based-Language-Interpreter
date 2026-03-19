package projectLabo.parser.ast;

import projectLabo.visitors.Visitor;

public class Diff extends BinaryOp {
	public Diff(Exp left, Exp right) {
		super(left, right);
	}
	
	@Override
	public <T> T accept(Visitor<T> visitor) {
		return visitor.visitDiff(left, right);
	}
}