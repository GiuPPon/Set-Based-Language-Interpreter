package projectLabo.parser.ast;

import projectLabo.visitors.Visitor;

public class IsIn extends BinaryOp {
	public IsIn(Exp left, Exp right) {
		super(left, right);
	}
	
	@Override
	public <T> T accept(Visitor<T> visitor) {
		return visitor.visitIsIn(left, right);
	}
}