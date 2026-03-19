package projectLabo.parser.ast;

import projectLabo.visitors.Visitor;

public class SetLit extends UnaryOp {
    public SetLit(Exp exp) {
        super(exp);
    }

    @Override
    public <T> T accept(Visitor<T> visitor) {
        return visitor.visitSetLit(exp);
    }
}